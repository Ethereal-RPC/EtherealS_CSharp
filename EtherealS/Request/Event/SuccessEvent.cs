using EtherealS.Core.Event.Attribute;
using System.Collections.Generic;
using System.Reflection;

namespace EtherealS.Request.Event
{
    public class SuccessEventContext : RequestContext
    {
        public object RemoteResult { get; set; }
        public SuccessEventContext(Dictionary<string, object> parameters, MethodInfo method, object remoteResult) : base(parameters, method)
        {
            RemoteResult = remoteResult;
        }
    }
    public class SuccessEvent : EventSender
    {
        public SuccessEvent(string instance, string mapping, string params_mapping = "") : base(instance, mapping, params_mapping)
        {
        }
    }
}
