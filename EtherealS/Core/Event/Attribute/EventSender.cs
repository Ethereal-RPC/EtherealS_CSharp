using EtherealS.Core.Model;
using System;
using System.Collections.Generic;

namespace EtherealS.Core.Event.Attribute
{
    [AttributeUsage(AttributeTargets.Method)]
    public class EventSender : System.Attribute
    {
        public string InstanceName { get; set; }
        public string Mapping { get; set; }
        public Dictionary<string, string> paramsMapping { get; set; }
        public EventSender(string instance, string mapping, string params_mapping = "")
        {
            InstanceName = instance;
            Mapping = mapping;
            params_mapping = params_mapping.Replace("[", "").Replace("]", "");
            string[] paramsSplit = params_mapping.Split(",");
            paramsMapping = new(params_mapping.Length);
            foreach (string param in paramsSplit)
            {
                string[] param_mapping = param.Split(":");
                if (param_mapping.Length != 2) throw new TrackException(TrackException.ErrorCode.Core, $"{params_mapping}中{param}不合法");
                paramsMapping.Add(param_mapping[1], param_mapping[0]);
            }
        }
    }
}
