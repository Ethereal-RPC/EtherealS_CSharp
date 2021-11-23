using EtherealS.Core.Manager.Event.Attribute;
using System;

namespace EtherealS_Test.RequestDemo
{
    internal class EventClass
    {
        [Event("after")]
        public void After(int ddd, [EventContextParam] EventContext context, string s)
        {
            Console.WriteLine(ddd);
            Console.WriteLine(s);
            Console.WriteLine("结果值是" + ((AfterEventContext)context).Result);
        }
    }
}
