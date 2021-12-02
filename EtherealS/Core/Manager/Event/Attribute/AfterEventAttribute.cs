using System.Collections.Generic;
using System.Reflection;

namespace EtherealS.Core.Manager.Event.Attribute
{
    public class AfterEventContext : EventContext
    {
        public object Result { get; set; }
        public AfterEventContext(Dictionary<string, object> parameters, MethodInfo method, object result) : base(parameters, method)
        {
            Result = result;
        }
    }
    public class AfterEventAttribute : EventSenderAttribute
    {
        public AfterEventAttribute(string function) : base(function)
        {
        }
    }
}
