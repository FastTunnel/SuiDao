using System;
using System.Collections.Generic;
using System.Text;

namespace SuiDao.Client
{
    public static class SuiDaoApi
    {
        /// <summary>
        /// 根据Key获取可用的服务器列表
        /// </summary>
        internal static readonly string GetServerListByKey = "https://api1.suidao.io/api/Client/GetServerByKey";

        /// <summary>
        /// 根据Key和ServerId获取所有可用隧道列表
        /// </summary>
        internal static readonly string GetTunnelListByKeyAndServerId = "https://api1.suidao.io/api/Client/GetTunnelByKey";
    }
}
