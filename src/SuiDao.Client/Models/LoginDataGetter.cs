using FastTunnel.Core.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SuiDao.Client.Extensions;
using SuiDao.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SuiDao.Client.Models
{
    public class LoginDataGetter
    {
        ILogger<LoginDataGetter> _logger;

        public LoginDataGetter(ILogger<LoginDataGetter> logger, IConfiguration configuration)
        {
            this.configuration = configuration;
            _logger = logger;
        }

        const string KeyLogName = ".key";
        private IConfiguration configuration;

        /// <summary>
        /// 上次选择的key，默认第一个key
        /// </summary>
        string lastKeyInput = "1";

        /// <summary>
        /// 上次选择的服务器序号
        /// </summary>
        string lastIndexInput = "0";
        int sec = 10;

        public LoginParam GetLoginData(CancellationToken cancellationToken)
        {
            // 控制台传参直接登录
            if (configuration["key"] != null)
            {
                _logger.LogDebug($"传参快速登录 key={configuration["key"]}");
                return LogByKeyAsync(configuration["key"], true, cancellationToken);
            }

            return defaultLogic(cancellationToken);
        }

        private LoginParam defaultLogic(CancellationToken cancellationToken)
        {
            _logger.LogDebug($"默认方式登录开始");
            var keyFile = Path.Combine(AppContext.BaseDirectory, KeyLogName);
            if (!File.Exists(keyFile))
            {
                return NewKey(cancellationToken);
            }

            List<string> keys = new List<string>();
            using (var reader = new StreamReader(keyFile))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    if (!string.IsNullOrEmpty(line))
                    {
                        keys.Add(line);
                    }
                }
            }

            keys = keys.Distinct().ToList();
            if (keys.Count > 0)
            {
                _logger.LogInformation("请选择要启动的客户端使用的Key：");
                _logger.LogInformation($"0：其他密钥登录");
                for (int i = 0; i < keys.Count; i++)
                {
                    _logger.LogInformation($"{i + 1}：{keys[i]}");
                }

                return HandleInputForServers(keys, cancellationToken);
            }

            return NewKey(cancellationToken);
        }

        private LoginParam NewKey(CancellationToken cancellationToken)
        {
            string key;
            while (true)
            {
                _logger.LogInformation("请输入登录密钥：");
                key = Console.ReadLine();
                // 当前控制台不可用
                if (key == null)
                    throw new Exception("登录参数异常，请在.key文件中填入登录key，或添加启动参数，如：SuiDao.Client key xxxxxxxxxxxxxxxxx");

                if (key.Equals(string.Empty))
                    continue;

                break;
            }

            return LogByKeyAsync(key, true, cancellationToken);
        }

        public static void AppendTextToFile(string filename, string inputStr)
        {
            var dir = Path.GetDirectoryName(filename);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            using (FileStream fsw = new FileStream(filename, FileMode.Append))
            {
                byte[] writeBytes = Encoding.UTF8.GetBytes(inputStr);
                fsw.Write(writeBytes, 0, writeBytes.Length);
                fsw.Close();
            }
        }

        private LoginParam HandleInputForServers(List<string> keys, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"输入编号回车键继续：{sec}秒后将自动选择序号{lastKeyInput}");
            while (true)
            {
                string input = null;
                Task.Factory.StartNew(() =>
                {
                    input = Console.ReadLine();
                }).Wait(sec * 1000, cancellationToken);

                if (string.IsNullOrEmpty(input))
                    input = lastKeyInput;

                if (string.IsNullOrEmpty(input))
                    continue;

                int index;
                if (!int.TryParse(input, out index))
                {
                    _logger.LogInformation("输入错误 请重新选择");
                    continue;
                }

                if (index < 0 || index > keys.Count)
                {
                    _logger.LogInformation("输入错误 请重新选择");
                    continue;
                }

                if (index == 0)
                {
                    return NewKey(cancellationToken);
                }
                else
                {
                    lastKeyInput = input;
                    return LogByKeyAsync(keys[index - 1], false, cancellationToken);
                }
            }
        }

        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="key"></param>
        /// <param name="logger"></param>
        /// <param name="log">是否记录登录记录</param>
        private LoginParam LogByKeyAsync(string key, bool log, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"正在使用Key={key}登录");
            var version = typeof(FastTunnelClient).Assembly.GetName().Version.ToString();
            var res_str = HttpHelper.PostAsJsonAsync(SuiDaoApi.GetServerListByKey, new { key = key, version }.ToJson()).
                GetAwaiter().GetResult();

            var jobj = System.Text.Json.JsonSerializer.Deserialize<ApiResponse<SuiDaoServerConfig>>(res_str);
            if (jobj.success)
            {
                // 记录登录记录
                if (log)
                {
                    AppendTextToFile(Path.Combine(AppContext.BaseDirectory, KeyLogName), Environment.NewLine + key);
                }

                SuiDaoServerInfo server;
                var res = jobj.data;
                if (res.servers != null && res.servers.Count() > 0)
                {
                    // 选择其中一个服务器继续
                    if (res.servers.Count() == 1)
                    {
                        server = res.servers.First();
                    }
                    else
                    {
                        string input = null;
                        _logger.LogInformation($"请选择其中一个服务器进行连接（输入序号，回车键确认）：{sec}秒后将自动选择 {res.servers[int.Parse(lastIndexInput)].server_name}");

                        for (int i = 0; i < res.servers.Length; i++)
                            _logger.LogInformation($"{i}:{res.servers[i].server_name}");

                        while (true)
                        {
                            Task.Factory.StartNew(() =>
                            {
                                input = Console.ReadLine();
                            }).Wait(sec * 1000, cancellationToken);

                            if (string.IsNullOrEmpty(input))
                                input = lastIndexInput;

                            int index;
                            if (int.TryParse(input, out index) && index <= res.servers.Length - 1 && index >= 0)
                            {
                                // 输入有效，退出循环
                                server = res.servers[index];
                                lastIndexInput = input;
                                _logger.LogInformation($"您选择的服务器为：{server.server_name}");
                                break;
                            }
                            else
                            {
                                _logger.LogInformation("输入有误，请重新输入");
                                continue;
                            }
                        }
                    }
                }
                else
                {
                    _logger.LogInformation("当前服务器无可用隧道，请添加新的隧道或服务器。");
                    return NewKey(cancellationToken);
                }

                return new LoginParam { key = key, server = server };
            }
            else
            {
                _logger.LogError(jobj.errorMsg);
                throw new Exception(jobj.errorMsg);
            }
        }
    }
}
