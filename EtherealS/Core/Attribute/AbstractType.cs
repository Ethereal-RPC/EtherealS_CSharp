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
        private string abstractName;

        public string AbstractName { get => abstractName; set => abstractName = value; }
    }
}
