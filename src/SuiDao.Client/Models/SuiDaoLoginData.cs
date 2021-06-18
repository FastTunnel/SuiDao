using FastTunnel.Core.Models;
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
    public class SuiDaoLoginData
    {
        const string KeyLogName = ".key";

        public object GetCustomLoginData()
        {
            //if (args.Length == 0)
            //{
            //    defaultLogic(logger);
            //    return;
            //}

            return defaultLogic();

            //switch (args[0])
            //{
            //    case "login":
            //        loginByKey(logger, args);
            //        break;
            //    default:
            //        Console.WriteLine($"{args[0]} 指令不存在");
            //        break;
            //}
        }


        public LoginParam defaultLogic()
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
                Console.WriteLine("请选择要启动的客户端：" + Environment.NewLine);

                Console.WriteLine($" 0：其他密钥登录");
                for (int i = 0; i < keys.Count; i++)
                {
                    Console.WriteLine($" {i + 1}：{keys[i]}");
                }

                Console.WriteLine(Environment.NewLine + "输入编号回车键继续：");

                return HandleNum(keys);
            }

            return NewKey();
        }

        private void loginByKey(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine($"参数不全");
                return;
            }

            var key = args[1];
            LogByKey(key, false);
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

        private LoginParam Run(string key, bool log)
        {
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
                        Console.WriteLine("请选择其中一个服务器进行连接（输入序号，回车键确认）：");
                        for (int i = 0; i < res.servers.Length; i++)
                        {
                            Console.WriteLine($"{i}:{res.servers[i].server_name}");
                        }

                        while (true)
                        {
                            var input = Console.ReadLine();
                            int index;
                            if (int.TryParse(input, out index) && index <= res.servers.Length - 1 && index >= 0)
                            {
                                // 输入有效，退出循环
                                server = res.servers[index];
                                Console.WriteLine($"您选择的服务器为：{server.server_name}");
                                break;
                            }
                            else
                            {
                                Console.WriteLine("输入有误，请重新输入");
                            }
                        }
                    }
                }
                else
                {
                    Console.WriteLine("您无可用的服务器");
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

        private LoginParam HandleNum(List<string> keys)
        {
            while (true)
            {
                var str = Console.ReadLine();
                if (string.IsNullOrEmpty(str))
                {
                    continue;
                }

                int index;
                if (!int.TryParse(str, out index))
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
            return Run(key, log);
        }
    }
}
