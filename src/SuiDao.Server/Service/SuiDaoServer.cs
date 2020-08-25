using FastTunnel.Core.Core;
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

        public SuiDaoServer(ILogger<SuiDaoServer> logger, IConfiguration configuration, FastTunnelServer fastTunnelServer)
        {
            _logger = logger;
            _configuration = configuration;
            _fastTunnelServer = fastTunnelServer;
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
            _fastTunnelServer.Run(_configuration.Get<Appsettings>().ServerSettings);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("===== SuiDao Server Stoping =====");
            return Task.CompletedTask;
        }
    }
}
