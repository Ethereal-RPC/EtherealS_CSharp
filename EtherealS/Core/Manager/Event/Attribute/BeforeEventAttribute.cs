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
    public class BeforeEventAttribute : EventSenderAttribute
    {
        public BeforeEventAttribute(string function) : base(function)
        {

        }
    }
}
