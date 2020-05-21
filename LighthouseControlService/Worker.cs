using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LighthouseControlCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LighthouseControlService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private bool steamVrRunning = true;
        private LighthousePowerController controller;

        public Worker(ILogger<Worker> logger, LighthousePowerController lpc)
        {
            _logger = logger;
            controller = lpc;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            controller.Initialise();

            while (!stoppingToken.IsCancellationRequested)
            {

                if (steamVrRunning)
                {
                    // Is it still running?
                    if(!SteamVRRunning())
                    {
                        steamVrRunning = false;
                        controller.TurnOff();
                    }
                }
                else
                {
                    // Has it started running?
                    if(SteamVRRunning())
                    {
                        steamVrRunning = true;
                        controller.TurnOn();
                    }
                }
             
                await Task.Delay(1000, stoppingToken);
            }

            _logger.LogInformation("Worker exit at: {time}", DateTimeOffset.Now);
        }

        private bool SteamVRRunning()
        {
            var allProcesses = Process.GetProcesses();
            return allProcesses.Any(p => p.ProcessName == "vrserver");
        }
    }
}
