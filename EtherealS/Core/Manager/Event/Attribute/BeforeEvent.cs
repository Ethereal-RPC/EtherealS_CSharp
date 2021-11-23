using System.Collections.Generic;
using System.Reflection;

namespace EtherealS.Core.Manager.Event.Attribute
{
    public class BeforeEventContext : EventContext
    {
        public BeforeEventContext(Dictionary<string, object> parameters, MethodInfo method) : base(parameters, method)
        {

        }
    }
    public class BeforeEvent : EventSender
    {
        public BeforeEvent(string function) : base(function)
        {
        }
    }
}
