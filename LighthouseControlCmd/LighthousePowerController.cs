using LighthouseControlCmd;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;
using Windows.Storage.Streams;

namespace LighthouseControlCore
{
	public class LighthousePowerController
    {
		private const byte ON = 0x01;
		private const byte OFF = 0x00;
		private readonly Guid _powerGuid = Guid.Parse("00001523-1212-efde-1523-785feabcd124");
		private readonly Guid _powerCharacteristic = Guid.Parse("00001525-1212-efde-1523-785feabcd124");

		private bool enumerationComplete = false;
		private byte _command;
		private List<(string id, string name)> _lighthouseIds;
		private readonly ILogger _logger;
		private readonly int _minLighthouses = 2;

		public LighthousePowerController(ILogger<LighthousePowerController> logger, IOptions<AppSettings> opt)
		{
			_logger = logger;
			_minLighthouses = opt.Value.MinLighthouses;
		}

		private void ProcessLighthouseId(string id, string name)
		{
			//https://docs.microsoft.com/en-us/windows/uwp/devices-sensors/gatt-client
			var potentialLighthouseTask = BluetoothLEDevice.FromIdAsync(id).AsTask();
			potentialLighthouseTask.Wait();
			if (!potentialLighthouseTask.IsCompletedSuccessfully && potentialLighthouseTask.Result != null)
			{
				_logger.LogError($"Could not connect to lighthouse {name}");
				return;
			}

			using var btDevice = potentialLighthouseTask.Result;

			var gattServicesTask = btDevice.GetGattServicesAsync().AsTask();
			gattServicesTask.Wait();
			if (!gattServicesTask.IsCompletedSuccessfully || gattServicesTask.Result.Status != GattCommunicationStatus.Success)
			{
				_logger.LogError("Failed to get services");
				return;
			}

			_logger.LogInformation($"Got services for {name}");

			using var service = gattServicesTask.Result.Services.SingleOrDefault(s => s.Uuid == _powerGuid);

			if (service == null)
			{
				_logger.LogError("Could not find power service");
				return;
			}

			_logger.LogInformation($"Found power service for {name}");

			var powerCharacteristicsTask = service.GetCharacteristicsAsync().AsTask();
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

			_logger.LogInformation($"Found power characteristic for {name}");

			using var w = new DataWriter();
			w.WriteByte(_command);
			var buff = w.DetachBuffer();

			var friendlyCommand = _command == ON ? "ON" : "OFF";
			_logger.LogInformation($"Sending {friendlyCommand} command to {name}");
			var writeResultTask = powerChar.WriteValueAsync(buff).AsTask();
			writeResultTask.Wait();

			if (!writeResultTask.IsCompletedSuccessfully || writeResultTask.Result != GattCommunicationStatus.Success)
			{
				_logger.LogError("Failed to write to char");
				return;
			}

			_logger.LogInformation($"Success for {name}");
		}

        public void TurnOn()
        {
			_logger.LogInformation("Turning lighthouses on...");
			_command = ON;
			StartWatch();
        }

        public void TurnOff()
        {
			_logger.LogInformation("Turning lighthouses off...");
			_command = OFF;
			StartWatch();
        }

        private void StartWatch()
        {
			_lighthouseIds = new List<(string, string)>();
			var deviceWatcher = DeviceInformation.CreateWatcher(BluetoothLEDevice.GetDeviceSelectorFromPairingState(false));

			// Register event handlers before starting the watcher.
			// Added, Updated and Removed are required to get all nearby devices
			deviceWatcher.Added += DeviceWatcher_Added;
			deviceWatcher.Updated += DeviceWatcher_Updated;
			deviceWatcher.Removed += DeviceWatcher_Removed;

			// EnumerationCompleted and Stopped are optional to implement.
			deviceWatcher.EnumerationCompleted += DeviceWatcher_EnumerationCompleted;
			deviceWatcher.Stopped += DeviceWatcher_Stopped;

			// Start the watcher.
			deviceWatcher.Start();

			_logger.LogInformation($"Looking for {_minLighthouses}, waiting for complete enumeration...");

			while (_lighthouseIds.Count < _minLighthouses && !enumerationComplete)
			{
				Thread.Sleep(10);
			}

			enumerationComplete = false;

			deviceWatcher.Stop();

			_lighthouseIds.ForEach(lid => ProcessLighthouseId(lid.id, lid.name));
		}

		private void DeviceWatcher_Stopped(DeviceWatcher sender, object args)
		{
			_logger.LogDebug("Stopped");
		}

		private void DeviceWatcher_EnumerationCompleted(DeviceWatcher sender, object args)
		{
			_logger.LogInformation("Enumeration complete");
			enumerationComplete = true;
		}

		private void DeviceWatcher_Removed(DeviceWatcher sender, DeviceInformationUpdate args)
		{
			_logger.LogDebug($"Device removed {args.Id}");
		}

		private void DeviceWatcher_Updated(DeviceWatcher sender, DeviceInformationUpdate args)
		{
			_logger.LogDebug($"Device updated {args.Id}");
		}

		private void DeviceWatcher_Added(DeviceWatcher sender, DeviceInformation args)
		{
			if (!args.Name.StartsWith("LHB-"))
			{
				return;
			}

			_logger.LogInformation($"Found lighthouse {args.Name}");
			_lighthouseIds.Add((args.Id, args.Name));
		}
	}
}
