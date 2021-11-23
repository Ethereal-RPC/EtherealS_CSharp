using System;
using System.Collections.Generic;
using System.Reflection;

namespace EtherealS.Core.Manager.Event.Attribute
{
    public class ExceptionEventContext : EventContext
    {
        public Exception Exception { get; set; }
        public ExceptionEventContext(Dictionary<string, object> parameters, MethodInfo method, Exception exception) : base(parameters, method)
        {
            Exception = exception;
        }
    }
    public class ExceptionEvent : EventSender
    {
        public ExceptionEvent(string function) : base(function)
        {
        }

        public Exception Exception { get; set; }
        public bool IsThrow { get; set; }

    }
}
