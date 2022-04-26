using FastTunnel.Core.Client;
using FastTunnel.Core.Exceptions;
using FastTunnel.Core.Handlers.Server;
using FastTunnel.Core.Models;
using Microsoft.Extensions.Logging;
using SuiDao.Client.Models;
using SuiDao.Client;
using SuiDao.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading.Tasks;
using System.Xml.Linq;
using Yarp.ReverseProxy.Configuration;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using FastTunnel.Core.Extensions;

namespace SuiDao.Server.Handlers
{
    public class SuiDaoLoginHandler : LoginHandler
    {
        public SuiDaoLoginHandler(ILogger<SuiDaoLoginHandler> logger, IProxyConfigProvider proxyConfig)
            : base(logger, proxyConfig)
        {
        }

        public override async Task<bool> HandlerMsg(FastTunnelServer server, TunnelClient client, string content)
        {
            var version = typeof(LoginHandler).Assembly.GetName().Version;
            var versionLow = $"当前客户端版本低于服务端版本{version}，请下载最新的客户端：https://github.com/FastTunnel/SuiDao/releases";
            var logMsg = System.Text.Json.JsonSerializer.Deserialize<LogInByKeyMassage>(content);

            if (string.IsNullOrEmpty(logMsg.client_version) || Version.Parse(logMsg.client_version).Major < version.Major)
            {
                throw new Exception(versionLow);
            }

            var res = HttpHelper.PostAsJsonAsync(SuiDaoApi.GetTunnelListByKeyAndServerId, logMsg.ToJson()).Result;

            var jobj = System.Text.Json.JsonSerializer.Deserialize<ApiResponse<Tunnel[]>>(res);
            if (jobj.success)
            {
                var tunnels = jobj.data;
                var Webs = new List<WebConfig>();
                var SSH = new List<ForwardConfig>();

                foreach (var tunnel in tunnels)
                {
                    if (tunnel.app_type == 1) // web
                    {
                        Webs.Add(new WebConfig
                        {
                            LocalIp = tunnel.local_ip,
                            LocalPort = tunnel.local_port,
                            SubDomain = tunnel.sub_domain,
                            WWW = string.IsNullOrEmpty(tunnel.custom_domain) ? null : new string[] { tunnel.custom_domain }
                        });
                    }
                    else if (tunnel.app_type == 2)
                    {
                        SSH.Add(new ForwardConfig
                        {
                            LocalIp = tunnel.local_ip,
                            LocalPort = tunnel.local_port,
                            RemotePort = tunnel.remote_port,
                        });
                    }
                }

                await HandleLoginAsync(server, client, new LogInMassage
                {
                    Forwards = SSH,
                    Webs = Webs,
                });

                return NeedRecive;
            }
            else
            {
                throw new APIErrorException(jobj.errorMsg);
            }
        }
    }
}
