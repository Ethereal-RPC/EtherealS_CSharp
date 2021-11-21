using EtherealS.Core.Model;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;

namespace EtherealS.Core.Event.Attribute
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
    [AttributeUsage(AttributeTargets.Method)]
    public class EventSender : System.Attribute
    {
        private static Regex regex = new Regex(@"\w+");
        public string InstanceName { get; set; }
        public string Mapping { get; set; }
        public Dictionary<string, string> paramsMapping { get; set; }
        public EventSender(string function)
        {
            MatchCollection matches = regex.Matches(function);
            if (matches.Count % 2 != 0 || matches.Count < 2) throw new TrackException(TrackException.ErrorCode.Core, $"{function}不合法");
            InstanceName = matches[0].Value;
            Mapping = matches[1].Value;
            paramsMapping = new(matches.Count - 2);
            for (int i = 2; i < matches.Count;)
            {
                paramsMapping.Add(matches[i++].Value, matches[i++].Value);
            }
        }
    }
}
