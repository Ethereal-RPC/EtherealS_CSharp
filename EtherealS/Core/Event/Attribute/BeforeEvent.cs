using EtherealS.Core.Event.Model;
using System.Collections.Generic;
using System.Reflection;

namespace EtherealS.Core.Event.Attribute
{
    public class BeforeEventContext : EventContext
    {
        public BeforeEventContext(Dictionary<string, object> parameters, MethodInfo method) : base(parameters, method)
        {

        }
    }
    public class BeforeEvent : EventSender
    {
        public BeforeEvent(string instance, string mapping, string params_mapping = "") : base(instance, mapping, params_mapping)
        {

        }
    }
}
