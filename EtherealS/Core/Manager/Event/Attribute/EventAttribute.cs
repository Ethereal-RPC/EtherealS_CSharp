using System;

namespace EtherealS.Core.Manager.Event.Attribute
{
    [AttributeUsage(AttributeTargets.Method)]
    public class EventAttribute : System.Attribute
    {
        public EventAttribute(string Mapping)
        {
            this.Mapping = Mapping;
        }
        public string Mapping { get; set; }
    }
}
