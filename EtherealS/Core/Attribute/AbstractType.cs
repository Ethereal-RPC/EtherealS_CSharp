using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtherealS.Core.Attribute
{
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Method)]
    public class AbstractType:System.Attribute
    {
        public AbstractType(string Name)
        {
            name = Name;
        }
        private string name;

        public string Name { get => name; set => name = value; }
    }
}
