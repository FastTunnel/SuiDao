using FastTunnel.Core;
using FastTunnel.Core.Client;
using FastTunnel.Core.Handlers.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Sockets;
using FastTunnel.Core.Models;

namespace SuiDao.Client.Models
{
    public class SuiDaoTunnel : FastTunnelClient
    {
        public SuiDaoTunnel(ILogger<FastTunnelClient> logger, HttpRequestHandler newCustomerHandler, NewSSHHandler newSSHHandler, LogHandler logHandler, IConfiguration configuration, ClientHeartHandler clientHeartHandler)
            : base(logger, newCustomerHandler, newSSHHandler, logHandler, configuration, clientHeartHandler)
        {
        }

        protected override Socket login()
        {
            LoginParam loginParam = new SuiDaoLoginData().defaultLogic();

            Connecter _client = null;

            Server = new SuiDaoServer { ServerAddr = loginParam.server.ip, ServerPort = loginParam.server.bind_port };

            try
            {
                _client = new Connecter(Server.ServerAddr, Server.ServerPort);
                _client.Connect();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "登录异常");
                _client.Socket.Close();
                throw;
            }

            // 登录
            _client.Send(new Message<LogInByKeyMassage>
            {
                MessageType = MessageType.C_LogIn,
                Content = new LogInByKeyMassage { key = loginParam.key, server_id = loginParam.server.server_id }
            });

            return _client.Socket;
        }
    }
}
