﻿using EtherealS.Core.EventManage.Attribute;
using System.Collections.Generic;
using System.Reflection;

namespace EtherealS.Service.Event
{
    public class ServiceContext : EventContext
    {
        public ServiceContext(Dictionary<string, object> parameters, MethodInfo method) : base(parameters, method)
        {

        }
    }
}
