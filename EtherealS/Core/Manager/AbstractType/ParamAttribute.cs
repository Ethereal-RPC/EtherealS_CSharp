using System;

namespace EtherealS.Core.Manager.AbstractType
{
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Method)]
    public class ParamAttribute : System.Attribute
    {
        public ParamAttribute(string Type)
        {
            this.Type = Type;
        }
        public string Type { get; set; }

    }
}
