using EtherealS.Model;
using EtherealS.NativeServer;
using EtherealS.RPCNet;
using EtherealS.RPCRequest;
using EtherealS.RPCService;
using EtherealS_Test.Model;
using EtherealS_Test.RequestDemo;
using EtherealS_Test.ServiceDemo;
using System;
using System.Collections.Generic;
using System.Text;

namespace EtherealS_Test
{
    public class Program
    {
        public static void Main()
        {
            //注册数据类型
            RPCTypeConfig types = new RPCTypeConfig();
            types.Add<int>("int");
            types.Add<User>("user");
            types.Add<long>("long");
            types.Add<string>("string");
            types.Add<bool>("bool");
            
            //建立网关
            Net net = NetCore.Register("demo");
            //向网关注册服务
            Service service = ServiceCore.Register<ServerService>(net, "Server", types);
            //向网关注册请求
            ClientRequest request = RequestCore.Register<ClientRequest>(net, "Client", types);
            //本例中，突出服务类可作为正常类
            (service.Instance as ServerService).UserRequest = request;
            //向网关注册连接(提供一个生成User的方法)
            ServerListener server = ServerCore.Register(net, "127.0.0.1", "28015",()=>new User());
            //启动连接
            server.Start();
            Console.WriteLine("服务器初始化完成....");
            
        }

    }
}
