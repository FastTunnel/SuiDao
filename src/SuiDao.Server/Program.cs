using FastTunnel.Core.Core;
using FastTunnel.Core.Handlers;
using FastTunnel.Core.Handlers.Server;
using FastTunnel.Core.Logger;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NLog.Extensions.Logging;

namespace SuiDao.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureLogging((context) =>
                {
                    context.AddNLog(NlogConfig.getNewConfig());
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<SuiDaoServer>();
                });
    }
}
