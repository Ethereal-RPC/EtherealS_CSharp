using EtherealS.Core.Manager.Event.Attribute;
using System.Collections.Generic;
using System.Reflection;

namespace EtherealS.Request.Event
{
    public class RequestContext : EventContext
    {
        public RequestContext(Dictionary<string, object> parameters, MethodInfo method) : base(parameters, method)
        {

        }
    }
}
