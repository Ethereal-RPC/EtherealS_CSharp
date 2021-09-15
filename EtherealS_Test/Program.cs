using EtherealS.Model;
using EtherealS.NativeServer;
using EtherealS.NativeServer.Abstract;
using EtherealS.RPCNet;
using EtherealS.RPCRequest;
using EtherealS.RPCService;
using EtherealS_Test.Model;
using EtherealS_Test.RequestDemo;
using EtherealS_Test.ServiceDemo;
using System;
using System.Collections.Generic;

namespace EtherealS_Test
{
    public class Program
    {
        public static void Main()
        {
            string ip = "127.0.0.1";
            string port = "28015";
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
            RPCTypeConfig types = new RPCTypeConfig();
            types.Add<int>("Int");
            types.Add<User>("User");
            types.Add<long>("Long");
            types.Add<string>("String");
            types.Add<bool>("Bool");
            //建立网关
            Net net = NetCore.Register("demo");
            net.ExceptionEvent += Config_ExceptionEvent;
            net.LogEvent += Config_LogEvent;
            //向网关注册服务
            Service service = ServiceCore.Register<ServerService>(net, "Server", types);
            //向网关注册请求
            ClientRequest request = RequestCore.Register<ClientRequest>(net, "Client", types);
            //本例中，突出服务类可作为正常类
            (service.Instance as ServerService).UserRequest = request;
            //向网关注册连接(提供一个生成User的方法)
            Server server = ServerCore.Register(net,new string[]{ $"{ip}:{port}/NetDemo/"} ,()=>new User());
            List<Tuple<string, EtherealC.NativeClient.ClientConfig>> ips = new();
            EtherealC.NativeClient.ClientConfig  clientConfig = new EtherealC.NativeClient.ClientConfig();
            /*
             * 部署分布式集群
             */
            //开启集群
            net.Config.NetNodeMode = true;
            //添加集群地址
            ips.Add(new Tuple<string,EtherealC.NativeClient.ClientConfig>($"{ip}:{28015}/NetDemo/", clientConfig));
            ips.Add(new Tuple<string,EtherealC.NativeClient.ClientConfig>($"{ip}:{28016}/NetDemo/", clientConfig));
            ips.Add(new Tuple<string,EtherealC.NativeClient.ClientConfig>($"{ip}:{28017}/NetDemo/", clientConfig));
            ips.Add(new Tuple<string,EtherealC.NativeClient.ClientConfig>($"{ip}:{28018}/NetDemo/", clientConfig));
            net.Config.NetNodeIps = ips;
            //发布服务
            net.Publish();
            Console.WriteLine("服务器初始化完成....");
            Console.ReadKey();
        }

        private static void Config_LogEvent(RPCLog log)
        {
            Console.WriteLine(log.Message);
        }

        private static void Config_ExceptionEvent(Exception exception)
        {
            Console.WriteLine(exception.Message);
        }
    }
}
