using FastTunnel.Core.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
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

        public LoginDataGetter(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public LoginParam GetLoginData()
        {
            // 控制台传参直接登录
            if (configuration["key"] != null)
            {
                return LogByKey(configuration["key"], true);
            }

            return defaultLogic();
        }


        private LoginParam defaultLogic()
        {
            var keyFile = Path.Combine(AppContext.BaseDirectory, KeyLogName);
            if (!File.Exists(keyFile))
            {
                return NewKey();
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
                Console.WriteLine("请选择要启动的客户端使用的Key：" + Environment.NewLine);
                Console.WriteLine($" 0：其他密钥登录");
                for (int i = 0; i < keys.Count; i++)
                {
                    Console.WriteLine($" {i + 1}：{keys[i]}");
                }

                return HandleNum(keys);
            }

            return NewKey();
        }

        private LoginParam NewKey()
        {
            string key;
            while (true)
            {
                Console.Write("请输入登录密钥：");
                key = Console.ReadLine();

                if (string.IsNullOrEmpty(key))
                {
                    continue;
                }

                break;
            }

            return LogByKey(key, true);
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

        private LoginParam HandleNum(List<string> keys)
        {
            Console.WriteLine($"{Environment.NewLine}输入编号回车键继续：5秒后将自动选择序号{lastKeyInput}");
            while (true)
            {
                string input = lastKeyInput;
                Task.Factory.StartNew(() =>
                {
                    input = Console.ReadLine();
                }).Wait(5 * 1000);

                if (string.IsNullOrEmpty(input))
                {
                    continue;
                }

                int index;
                if (!int.TryParse(input, out index))
                {
                    Console.WriteLine("输入错误 请重新选择");
                    continue;
                }

                if (index < 0 || index > keys.Count)
                {
                    Console.WriteLine("输入错误 请重新选择");
                    continue;
                }

                if (index == 0)
                {
                    return NewKey();
                }
                else
                {
                    return LogByKey(keys[index - 1], false);
                }
            }
        }

        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="key"></param>
        /// <param name="logger"></param>
        /// <param name="log">是否记录登录记录</param>
        private LoginParam LogByKey(string key, bool log)
        {
            Console.WriteLine($"AccessKey={key} \n登录中...");
            var res_str = HttpHelper.PostAsJson(SuiDaoApi.GetServerListByKey, $"{{ \"key\":\"{key}\"}}").Result;
            var jobj = JObject.Parse(res_str);
            if ((bool)jobj["success"] == true)
            {
                // 记录登录记录
                if (log)
                {
                    AppendTextToFile(Path.Combine(AppContext.BaseDirectory, KeyLogName), Environment.NewLine + key);
                }

                SuiDaoServerInfo server;
                var res = jobj["data"].ToObject<SuiDaoServerConfig>();
                if (res.servers != null && res.servers.Count() > 0)
                {
                    // 选择其中一个服务器继续
                    if (res.servers.Count() == 1)
                    {
                        server = res.servers.First();
                    }
                    else
                    {
                        string input = lastIndexInput;
                        Console.WriteLine($"请选择其中一个服务器进行连接（输入序号，回车键确认）：5秒后将自动选择 {res.servers[int.Parse(lastIndexInput)].server_name}");

                        for (int i = 0; i < res.servers.Length; i++)
                            Console.WriteLine($"{i}:{res.servers[i].server_name}");

                        while (true)
                        {
                            Task.Factory.StartNew(() =>
                            {
                                input = Console.ReadLine();
                            }).Wait(5 * 1000);

                            if (string.IsNullOrEmpty(input))
                                input = "0";

                            int index;
                            if (int.TryParse(input, out index) && index <= res.servers.Length - 1 && index >= 0)
                            {
                                // 输入有效，退出循环
                                server = res.servers[index];
                                lastIndexInput = input;
                                Console.WriteLine($"您选择的服务器为：{server.server_name}");
                                break;
                            }
                            else
                            {
                                Console.WriteLine("输入有误，请重新输入");
                                continue;
                            }
                        }
                    }
                }
                else
                {
                    Console.WriteLine("当前服务器无可用隧道，请添加新的隧道或服务器。");
                    return NewKey();
                }

                return new LoginParam { key = key, server = server };
            }
            else
            {
                Console.WriteLine(jobj["errorMsg"].ToString());
                return NewKey();
            }
        }
    }
}
