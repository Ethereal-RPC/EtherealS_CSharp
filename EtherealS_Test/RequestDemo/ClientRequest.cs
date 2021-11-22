using EtherealS.Core.Event.Attribute;
using EtherealS.Request.Attribute;
using EtherealS.Request.WebSocket;
using EtherealS_Test.Model;
using System;

namespace EtherealS_Test.RequestDemo
{
    public class ClientRequest : WebSocketRequest
    {
        public override void Initialize()
        {
            types.Add<int>("Int");
            types.Add<User>("User");
            types.Add<long>("Long");
            types.Add<string>("String");
            types.Add<bool>("Bool");
        }

        public override void Register()
        {
            object instance = new EventClass();
            RegisterIoc("instance", instance);
        }

        public override void UnInitialize()
        {

        }

        public override void UnRegister()
        {

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

    }
}
