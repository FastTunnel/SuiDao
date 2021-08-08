using FastTunnel.Core;
using FastTunnel.Core.Client;
using FastTunnel.Core.Handlers.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Sockets;
using FastTunnel.Core.Models;
using FastTunnel.Core.Sockets;
using FastTunnel.Core.Config;
using Microsoft.Extensions.Options;
using FastTunnel.Core.Extensions;

namespace SuiDao.Client.Models
{
    public class SuiDaoClient : FastTunnelClient
    {
        SuiDaoLoginData suiDaoLoginData;

        public SuiDaoClient(
          ILogger<FastTunnelClient> logger,
          SwapHandler newCustomerHandler,
          LogHandler logHandler,
          IConfiguration _configuration,
          IOptionsMonitor<DefaultClientConfig> configuration)
            : base(logger, newCustomerHandler, logHandler, configuration)
        {
            this.suiDaoLoginData = new SuiDaoLoginData(_configuration);
        }

        public override string GetLoginMsg()
        {
            LoginParam loginParam = suiDaoLoginData.GetLoginData();
            Server = new SuiDaoServer { ServerAddr = loginParam.server.ip, ServerPort = loginParam.server.bind_port };
            return new LogInByKeyMassage { key = loginParam.key, server_id = loginParam.server.server_id }.ToJson(); ;
        }
    }
}
