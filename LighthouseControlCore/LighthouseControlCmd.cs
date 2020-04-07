using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

namespace LighthouseControlCore
{
	public class LighthouseControlCmd
	{
		static void Main(string[] args)
		{
			var serviceCollection = new ServiceCollection();
			serviceCollection.AddSingleton<LighthousePowerController>();
			serviceCollection.AddLogging(logging => logging.AddConsole());
			var serviceProvider = serviceCollection.BuildServiceProvider();

			var powerController = serviceProvider.GetService<LighthousePowerController>();

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
