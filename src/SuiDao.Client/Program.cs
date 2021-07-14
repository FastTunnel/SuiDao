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
                .UseWindowsService()
                .ConfigureServices((hostContext, services) =>
                {
                    // -------------------FastTunnel START------------------
                    services.AddFastTunnelClient();
                    // -------------------FastTunnel EDN--------------------

                    services.AddTransient<LoginDataGetter, LoginDataGetter>();

                    // -------------------自定义实现覆盖----------------------
                    services.AddSingleton<FastTunnelClient, SuiDaoClient>();
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