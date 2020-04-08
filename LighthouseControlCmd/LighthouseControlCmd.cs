using LighthouseControlCmd;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Configuration;
using System.IO;

namespace LighthouseControlCore
{
	public class LighthouseControlCmd
	{
		static void Main(string[] args)
		{
			var host = Host.CreateDefaultBuilder()
				.ConfigureServices((ctx, services) =>
				{
					services.Configure<AppSettings>(ctx.Configuration.GetSection("AppSettings"));
					services.AddSingleton<LighthousePowerController>();
				}).Build();

			var powerController = host.Services.GetService<LighthousePowerController>();

			if (args.Length != 1)
			{
				Console.WriteLine("Supply either 'on' or 'off'");
				return;
			}

			if (args[0] == "on")
			{
				powerController.TurnOn();
			}
			else
			{
				powerController.TurnOff();
			}
		}
	}
}
