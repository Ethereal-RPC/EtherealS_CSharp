using System.Collections.Generic;
using System.Reflection;

namespace EtherealS.Core.Event.Model
{
    public class EventContext
    {
        public EventContext(Dictionary<string, object> parameters, MethodInfo method)
        {
            Method = method;
            Parameters = parameters;
        }

        public MethodInfo Method { get; set; }
        public Dictionary<string, object> Parameters { get; set; }
    }
}
