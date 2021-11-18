using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EtherealS.Request.Attribute;
using EtherealS.Request.WebSocket;
using EtherealS_Test.Model;

namespace EtherealS_Test.RequestDemo
{
    public class ClientRequest:WebSocketRequest
    {
        public ClientRequest()
        {
            types.Add<int>("Int");
            types.Add<User>("User");
            types.Add<long>("Long");
            types.Add<string>("String");
            types.Add<bool>("Bool");
        }
        public override void Initialize()
        {

        }
        [RequestMethod(Mapping:"Say",InvokeType = RequestMethod.InvokeTypeFlags.Local)]
        public void Say(User user, User sender, string message)
        {
            Console.WriteLine("asd");
        }
        [RequestMethod(Mapping:"asd", InvokeType = RequestMethod.InvokeTypeFlags.Remote)]
        public string test()
        {
            return "asd";
        }

        public override void UnInitialize()
        {

        }
    }
}
