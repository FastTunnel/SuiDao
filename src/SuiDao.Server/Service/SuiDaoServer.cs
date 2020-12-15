using FastTunnel.Core.Client;
using FastTunnel.Core.Global;
using FastTunnel.Core.Handlers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SuiDao.Server
{
    public class SuiDaoServer : IHostedService
    {
        ILogger<SuiDaoServer> _logger;
        IConfiguration _configuration;
        FastTunnelServer _fastTunnelServer;

        public SuiDaoServer(ILogger<SuiDaoServer> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            _fastTunnelServer = new FastTunnelServer(_logger, _configuration.Get<Appsettings>().ServerSettings);
            FastTunnelGlobal.AddCustomHandler<IConfigHandler, SuiDaoConfigHandler>(new SuiDaoConfigHandler());
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("===== SuiDao Server Starting =====");

            try
            {
                Run();
            }
            catch (Exception ex)
            {
                // NLog: catch any exception and log it.
                _logger.LogError(ex, "Server Error");
                Console.WriteLine(ex);
            }

            return Task.CompletedTask;
        }

        private void Run()
        {
            _fastTunnelServer.Run();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("===== SuiDao Server Stoping =====");
            return Task.CompletedTask;
        }
    }
}
