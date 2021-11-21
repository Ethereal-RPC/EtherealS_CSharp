using System;

namespace EtherealS.Core.Attribute
{
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Method)]
    public class Param : System.Attribute
    {
        public Param(string Name)
        {
            name = Name;
        }
        private string name;

        public string Name { get => name; set => name = value; }
    }
}
