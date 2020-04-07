using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using LighthouseControlCore;

namespace LighthouseControlService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .ConfigureLogging((hostContext, logger) =>
            { 
                logger.AddConsole();
                logger.AddEventLog();
                
            })
            .ConfigureServices((hostContext, services) =>
            {
                services.AddSingleton<LighthousePowerController>();
                services.AddHostedService<Worker>();
            }).UseWindowsService();
    }
}
