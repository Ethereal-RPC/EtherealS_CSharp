using EtherealS.Core.EventManage.Attribute;
using EtherealS.Request.Attribute;
using EtherealS.Request.WebSocket;
using EtherealS_Test.Model;
using System;

namespace EtherealS_Test.RequestDemo
{
    public class ClientRequest : WebSocketRequest
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
            object instance = new EventClass();
            RegisterIoc("instance", instance);
            EventManager.RegisterEventMethod("instance", instance);
        }
        [RequestMapping(Mapping: "Say", InvokeType = RequestMapping.InvokeTypeFlags.Local)]
        public virtual void Say(User user, User sender, string message)
        {
            Console.WriteLine("asd");
        }
        [AfterEvent("instance.after(ddd:d,s:s))")]
        [RequestMapping(Mapping: "test", InvokeType = RequestMapping.InvokeTypeFlags.Local)]
        public virtual bool test(int d, string s)
        {
            Console.WriteLine("Add");
            return true;
        }

        public override void UnInitialize()
        {

        }
    }
}
