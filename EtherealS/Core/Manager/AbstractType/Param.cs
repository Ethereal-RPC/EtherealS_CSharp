using System;

namespace EtherealS.Core.Manager.AbstractType
{
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Method)]
    public class Param : System.Attribute
    {
        public Param(string Type)
        {
            this.Type = Type;
        }
        public string Type { get; set; }

    }
}
