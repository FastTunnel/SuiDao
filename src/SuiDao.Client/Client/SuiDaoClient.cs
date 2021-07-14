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

namespace SuiDao.Client.Models
{
    public class SuiDaoClient : FastTunnelClient
    {
        LoginDataGetter _loginDataGetter;

        public SuiDaoClient(ILogger<FastTunnelClient> logger, HttpRequestHandler newCustomerHandler, NewSSHHandler newSSHHandler, LogHandler logHandler, IConfiguration configuration, ClientHeartHandler clientHeartHandler, LoginDataGetter suiDaoLoginData)
            : base(logger, newCustomerHandler, newSSHHandler, logHandler, configuration, clientHeartHandler)
        {
            this._loginDataGetter = suiDaoLoginData;
        }

        /// <summary>
        /// 重写自己的登录逻辑
        /// </summary>
        /// <returns>成功登录后的Sokcet对象</returns>
        protected override Socket login()
        {
            LoginParam loginParam = _loginDataGetter.GetLoginData();
            DnsSocket _client = null;

            Server = new SuiDaoServer { ServerAddr = loginParam.server.ip, ServerPort = loginParam.server.bind_port };

            try
            {
                _client = new DnsSocket(loginParam.server.ip, loginParam.server.bind_port);
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
