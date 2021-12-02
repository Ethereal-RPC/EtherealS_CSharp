using EtherealS.Core.Manager.Event.Attribute;
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
    public class TimeoutEvent : EventSenderAttribute
    {
        public TimeoutEvent(string function) : base(function)
        {
        }
    }
}
