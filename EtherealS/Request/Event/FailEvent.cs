﻿using EtherealS.Core.EventManage.Attribute;
using EtherealS.Core.Model;
using System.Collections.Generic;
using System.Reflection;

namespace EtherealS.Request.Event
{
    public class FailEventContext : RequestContext
    {
        public Error Error { get; set; }
        public FailEventContext(Dictionary<string, object> parameters, MethodInfo method, Error error) : base(parameters, method)
        {
            Error = error;
        }
    }
    public class FailEvent : EventSender
    {
        public FailEvent(string function) : base(function)
        {
        }
    }
}
