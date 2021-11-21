using EtherealS.Core.Event.Attribute;
using System.Collections.Generic;
using System.Reflection;

namespace EtherealS.Request.Event
{
    public class TimeoutEventContext : RequestContext
    {
        public TimeoutEventContext(Dictionary<string, object> parameters, MethodInfo method) : base(parameters, method)
        {

        }
    }
    public class TimeoutEvent : EventSender
    {
        public TimeoutEvent(string instance, string mapping, string params_mapping = "") : base(instance, mapping, params_mapping)
        {

        }
    }
}
