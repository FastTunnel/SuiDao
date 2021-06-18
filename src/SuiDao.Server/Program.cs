using FastTunnel.Core.Extensions;
using FastTunnel.Core.Global;
using FastTunnel.Core.Handlers;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;

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
                .ConfigureServices((hostContext, services) =>
                {
                    // -------------------FastTunnel START------------------
                    services.AddFastTunnelServer();
                    // -------------------FastTunnel EDN--------------------
                    FastTunnelGlobal.AddCustomHandler<IConfigHandler, SuiDaoConfigHandler>(new SuiDaoConfigHandler());
                })
                .ConfigureLogging((HostBuilderContext context, ILoggingBuilder logging) =>
                {
                    //logging.ClearProviders();
                    //logging.SetMinimumLevel(LogLevel.Trace);

                    logging.AddLog4Net();
                    logging.SetMinimumLevel(LogLevel.Debug);
                });
    }
}
