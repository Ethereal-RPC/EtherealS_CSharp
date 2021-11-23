using EtherealS.Core.Manager.Event.Attribute;
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
            Types.Add<int>("Int");
            Types.Add<User>("User");
            Types.Add<long>("Long");
            Types.Add<string>("String");
            Types.Add<bool>("Bool");
        }

        public override void Register()
        {
            object instance = new EventClass();
            IOCManager.Register("instance", instance);
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
