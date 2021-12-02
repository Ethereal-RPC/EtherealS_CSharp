using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtherealS.Net.Extension.Plugins
{
    [Plugin]
    public abstract class Plugin
    {
        public PluginConfig Config { get; set; }
        public PluginDomain Domain { get; set; }
        public abstract void Initialize(Abstract.Net net);
        public abstract void UnInitialize(Abstract.Net net);
    }
}
