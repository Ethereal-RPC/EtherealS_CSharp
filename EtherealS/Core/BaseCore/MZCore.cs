using EtherealS.Core.Manager.AbstractType;
using EtherealS.Core.Manager.IOC;

namespace EtherealS.Core.BaseCore
{
    public class MZCore : BaseCore
    {
        public IOCManager IOCManager { get; set; } = new();
        public AbstractTypeManager Types { get; set; } = new();

    }
}
