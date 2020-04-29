using LighthouseControlCmd;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Storage.Streams;

namespace LighthouseControlCore
{
	public class LighthousePowerController : IDisposable
    {
		private const byte ON = 0x01;
		private const byte OFF = 0x00;
		private readonly Guid _powerGuid = Guid.Parse("00001523-1212-efde-1523-785feabcd124");
		private readonly Guid _powerCharacteristic = Guid.Parse("00001525-1212-efde-1523-785feabcd124");

		private byte _command;
		private HashSet<Lighthouse> _lighthouses = new HashSet<Lighthouse>();
		private readonly ILogger _logger;
		private readonly int _minLighthouses = 2;
		private BluetoothLEAdvertisementWatcher watcher;

		public LighthousePowerController(ILogger<LighthousePowerController> logger, IOptions<AppSettings> opt)
		{
			_logger = logger;
			_minLighthouses = opt.Value.MinLighthouses;

			watcher = new BluetoothLEAdvertisementWatcher();
			watcher.Received += AdvertisementWatcher_Received;
		}

		public void Initialise()
		{
			watcher.Start();
		}

		private void ProcessLighthouse(Lighthouse lh)
		{
			//https://docs.microsoft.com/en-us/windows/uwp/devices-sensors/gatt-client
			var potentialLighthouseTask = BluetoothLEDevice.FromBluetoothAddressAsync(lh.Address).AsTask();
			potentialLighthouseTask.Wait();
			if (!potentialLighthouseTask.IsCompletedSuccessfully || potentialLighthouseTask.Result == null)
			{
				_logger.LogError($"Could not connect to lighthouse {lh.Name}");
				return;
			}

			using var btDevice = potentialLighthouseTask.Result;

			var gattServicesTask = btDevice.GetGattServicesAsync(BluetoothCacheMode.Uncached).AsTask();
			gattServicesTask.Wait();
			if (!gattServicesTask.IsCompletedSuccessfully || gattServicesTask.Result.Status != GattCommunicationStatus.Success)
			{
				_logger.LogError("Failed to get services");
				return;
			}

			_logger.LogDebug($"Got services for {lh.Name}");

			using var service = gattServicesTask.Result.Services.SingleOrDefault(s => s.Uuid == _powerGuid);

			if (service == null)
			{
				_logger.LogError("Could not find power service");
				return;
			}

			_logger.LogDebug($"Found power service for {lh.Name}");

			var powerCharacteristicsTask = service.GetCharacteristicsAsync(BluetoothCacheMode.Uncached).AsTask();
			powerCharacteristicsTask.Wait();
			if (!powerCharacteristicsTask.IsCompletedSuccessfully || powerCharacteristicsTask.Result.Status != GattCommunicationStatus.Success)
			{
				_logger.LogError("Could not get power service characteristics");
				return;
			}

			var powerChar = powerCharacteristicsTask.Result.Characteristics.SingleOrDefault(c => c.Uuid == _powerCharacteristic && c.CharacteristicProperties == GattCharacteristicProperties.Write);
			if (powerChar == null)
			{
				_logger.LogError("could not get power char");
				return;
			}

			_logger.LogDebug($"Found power characteristic for {lh.Name}");

			using var w = new DataWriter();
			w.WriteByte(_command);
			var buff = w.DetachBuffer();

			var friendlyCommand = _command == ON ? "ON" : "OFF";
			_logger.LogDebug($"Sending {friendlyCommand} command to {lh.Name}");
			var writeResultTask = powerChar.WriteValueAsync(buff).AsTask();
			writeResultTask.Wait();

			if (!writeResultTask.IsCompletedSuccessfully || writeResultTask.Result != GattCommunicationStatus.Success)
			{
				_logger.LogError("Failed to write to char");
				return;
			}

			_logger.LogDebug($"Success for {lh.Name}");
		}

        public void TurnOn()
        {
			_logger.LogInformation("Turning lighthouses on...");

			doCommand(lh => !lh.PoweredOn, ON);

			_logger.LogInformation("All lighthouses on");
		}

        public void TurnOff()
        {
			_logger.LogInformation("Turning lighthouses off...");

			doCommand(lh => lh.PoweredOn, OFF);

			_logger.LogInformation("All lighthouses off");
		}

		private void doCommand(Func<Lighthouse, bool> lighthousePredicate, byte command)
		{
			_command = command;

			WaitForMinLighthouses();

			while (true)
			{
				var results = _lighthouses.Where(lighthousePredicate);
				if (!results.Any())
					break;

				results.ToList().ForEach(ProcessLighthouse);
				Thread.Sleep(5000);
			}
		}

		private void WaitForMinLighthouses()
		{
			if (_lighthouses.Count < _minLighthouses)
			{
				_logger.LogInformation($"Looking for {_minLighthouses}");

				while (_lighthouses.Count < _minLighthouses)
				{
					Thread.Sleep(100);
				}
			}
		}

		private void AdvertisementWatcher_Received(BluetoothLEAdvertisementWatcher sender, BluetoothLEAdvertisementReceivedEventArgs args)
		{
			if (!args.Advertisement.LocalName.StartsWith("LHB-"))
			{
				return;
			}

			var existing = _lighthouses.SingleOrDefault(lh => lh.Address == args.BluetoothAddress);

			if(existing == null)
			{
				_logger.LogInformation($"Found lighthouse {args.Advertisement.LocalName}");

				existing = new Lighthouse(args.Advertisement.LocalName, args.BluetoothAddress);
				_lighthouses.Add(existing);
			}

			var valveData = args.Advertisement.GetManufacturerDataByCompanyId(0x055D).Single();
			var data = new byte[valveData.Data.Length];

			using(var reader = DataReader.FromBuffer(valveData.Data))
			{
				reader.ReadBytes(data);
			}

			existing.PoweredOn = data[4] == 0x03;
		}

		#region IDisposable Support
		private bool disposedValue = false; // To detect redundant calls

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					watcher.Stop();
				}
				disposedValue = true;
			}
		}

		public void Dispose()
		{
			Dispose(true);
		}
		#endregion
	}
}
