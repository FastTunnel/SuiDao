using FastTunnel.Core.Exceptions;
using FastTunnel.Core.Handlers;
using FastTunnel.Core.Models;
using SuiDao.Client;
using SuiDao.Client.Models;
using SuiDao.Core;
using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace SuiDao.Server
{
    public class SuiDaoConfigHandler
    {
        public LogInMassage GetConfig(string content)
        {
            var logMsg = System.Text.Json.JsonSerializer.Deserialize< LogInByKeyMassage>(content);
            var res = HttpHelper.PostAsJsonAsync(SuiDaoApi.GetTunnelListByKeyAndServerId, $"{{ \"key\":\"{logMsg.key}\",\"server_id\":{logMsg.server_id}}}").Result;

            var jobj = System.Text.Json.JsonSerializer.Deserialize<ApiResponse<Tunnel[]>>(res);
            if (jobj.success)
            {
                var tunnels = jobj.data;
                var Webs = new List<WebConfig>();
                var forward = new List<ForwardConfig>();

                foreach (var tunnel in tunnels)
                {
                    if (tunnel.app_type == 1) // web
                    {
                        Webs.Add(new WebConfig
                        {
                            LocalIp = tunnel.local_ip,
                            LocalPort = tunnel.local_port,
                            SubDomain = tunnel.sub_domain
                        });
                    }
                    else if (tunnel.app_type == 2)
                    {
                        forward.Add(new ForwardConfig
                        {
                            LocalIp = tunnel.local_ip,
                            LocalPort = tunnel.local_port,
                            RemotePort = tunnel.remote_port,
                        });
                    }
                }

                return new LogInMassage
                {
                    Forwards = forward,
                    Webs = Webs,
                };
            }
            else
            {
                throw new APIErrorException(jobj.errorMsg);
            }
        }
    }
}
