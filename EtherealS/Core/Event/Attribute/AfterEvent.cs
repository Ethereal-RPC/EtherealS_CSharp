using EtherealS.Core.Event.Model;
using System.Collections.Generic;
using System.Reflection;

namespace EtherealS.Core.Event.Attribute
{
    public class AfterEventContext : EventContext
    {
        public object Result { get; set; }
        public AfterEventContext(Dictionary<string, object> parameters, MethodInfo method, object result) : base(parameters, method)
        {
            Result = result;
        }
    }
    public class AfterEvent : EventSender
    {
        public AfterEvent(string instance, string mapping, string params_mapping = "") : base(instance, mapping, params_mapping)
        {
        }
    }
}
