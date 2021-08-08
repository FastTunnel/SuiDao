using FastTunnel.Core;
using FastTunnel.Core.Client;
using FastTunnel.Core.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SuiDao.Client.Models;
using System;

namespace SuiDao.Client
{
    class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    // -------------------FastTunnel START------------------
                    services.AddFastTunnelClient();
                    // -------------------FastTunnel EDN--------------------

                    // -------------------自定义实现覆盖----------------------
                    services.AddSingleton<IFastTunnelClient, SuiDaoClient>();
                    // -----------------------------------------------------
                })
                .ConfigureLogging((HostBuilderContext context, ILoggingBuilder logging) =>
                {
                    logging.ClearProviders();
                    logging.AddLog4Net();
                    logging.SetMinimumLevel(LogLevel.Debug);
                });
    }
}
