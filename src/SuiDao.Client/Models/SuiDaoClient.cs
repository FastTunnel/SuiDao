using FastTunnel.Core;
using FastTunnel.Core.Client;
using FastTunnel.Core.Handlers.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using FastTunnel.Core.Config;
using Microsoft.Extensions.Options;
using System.Threading;
using SuiDao.Client.Extensions;

namespace SuiDao.Client.Models
{
    public class SuiDaoClient : FastTunnelClient
    {
        LoginDataGetter suiDaoLoginData;

        public SuiDaoClient(
          LoginDataGetter loginDataGetter,
          ILogger<FastTunnelClient> logger,
          SwapHandler newCustomerHandler,
          LogHandler logHandler,
          IConfiguration _configuration,
          IOptionsMonitor<DefaultClientConfig> configuration)
            : base(logger, newCustomerHandler, logHandler, configuration)
        {
            this.suiDaoLoginData = loginDataGetter;
        }

        protected override string GetLoginMsg(CancellationToken cancellationToken)
        {
            LoginParam loginParam = suiDaoLoginData.GetLoginData(cancellationToken);
            Server = new SuiDaoServer { ServerAddr = loginParam.server.ip, ServerPort = loginParam.server.bind_port };
            var version = typeof(FastTunnelClient).Assembly.GetName().Version.ToString();
            return new LogInByKeyMassage { key = loginParam.key, client_version = version, server_id = loginParam.server.server_id }.ToJson(); ;
        }
    }
}
