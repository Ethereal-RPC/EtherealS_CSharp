using EtherealS.Core.Model;
using EtherealS.Net;
using EtherealS.Net.Abstract;
using EtherealS.Request;
using EtherealS.Server;
using EtherealS.Server.Abstract;
using EtherealS.Service;
using EtherealS.Service.Abstract;
using EtherealS_Test.Model;
using EtherealS_Test.RequestDemo;
using EtherealS_Test.ServiceDemo;
using System;
using System.Collections.Generic;
using EtherealS.Net.WebSocket;
using EtherealS.Server.WebSocket;

namespace EtherealS_Test
{
    public class Program
    {

        public static void Main()
        {

            string ip = "127.0.0.1";
            string port;
            Console.WriteLine("请选择端口(0-3)");
            int mode = int.Parse(Console.ReadLine());
            switch (mode)
            {
                case 0:
                    port = "28015";
                    break;
                case 1:
                    port = "28016";
                    break;
                case 2:
                    port = "28017";
                    break;
                case 3:
                    port = "28018";
                    break;
                default:
                    port = mode.ToString();
                    break;
            }
            Console.Title = $"{ip}-{port}";
            //注册数据类型
            AbstractTypes types = new AbstractTypes();
            types.Add<int>("Int");
            types.Add<User>("User");
            types.Add<long>("Long");
            types.Add<string>("String");
            types.Add<bool>("Bool");
            //建立网关
            Net net = NetCore.Register(new WebSocketNet("demo"));
            net.ExceptionEvent += Config_ExceptionEvent;
            net.LogEvent += Config_LogEvent;
            //向网关注册服务
            ServerService service = ServiceCore.Register(net,new ServerService(), "Server", types);
            //向网关注册请求
            IClientRequest request = RequestCore.Register<ClientRequest, IClientRequest>(net, "Client", types);
            //本例中，突出服务类可作为正常类
            service.UserRequest = request;
            //向网关注册连接(提供一个生成User的方法)
            Server server = ServerCore.Register(net,new WebSocketServer(new List<string>(), () => new User()));
            server.Prefixes.Add($"ethereal://{ip}:{port}/NetDemo/");
            List<Tuple<string, EtherealC.Client.Abstract.ClientConfig>> ips = new();
            EtherealC.Client.Abstract.ClientConfig  clientConfig = new EtherealC.Client.WebSocket.WebSocketClientConfig();
            /*
             * 部署分布式集群
             */
            //开启集群
            net.Config.NetNodeMode = true;
            //添加集群地址
            ips.Add(new Tuple<string, EtherealC.Client.Abstract.ClientConfig>($"ethereal://{ip}:{28015}/NetDemo/", clientConfig));
            ips.Add(new Tuple<string, EtherealC.Client.Abstract.ClientConfig>($"ethereal://{ip}:{28016}/NetDemo/", clientConfig));
            ips.Add(new Tuple<string, EtherealC.Client.Abstract.ClientConfig>($"ethereal://{ip}:{28017}/NetDemo/", clientConfig));
            ips.Add(new Tuple<string, EtherealC.Client.Abstract.ClientConfig>($"ethereal://{ip}:{28018}/NetDemo/", clientConfig));
            net.Config.NetNodeIps = ips;
            //发布服务
            net.Publish();
            Console.WriteLine("服务器初始化完成....");
            Console.ReadKey();
        }
        

        private static void Config_LogEvent(TrackLog log)
        {
            Console.WriteLine(log.Message);
        }

        private static void Config_ExceptionEvent(Exception exception)
        {
            Console.WriteLine(exception.Message);
        }
    }
}
