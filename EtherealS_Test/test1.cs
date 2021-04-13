using EtherealS.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace EtherealS_Test
{
    public class test1
    {
        public bool test()
        {
            RPCType types = new RPCType();
            types.Add<string>("String");
            string ip = "127.0.0.1";
            string port = "28015";
            EtherealS.RPCNet.NetCore.Register(ip,port);
            EtherealS.RPCService.ServiceCore.Register(new Service(),"ServerService",ip,port,types);
            EtherealS.NativeServer.ServerCore.Register(ip, port,() => new Token()).Start();
            return true;
        }
    }
}
