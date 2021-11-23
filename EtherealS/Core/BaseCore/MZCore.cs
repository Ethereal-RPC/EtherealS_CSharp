using EtherealS.Core.Manager.AbstractType;
using EtherealS.Core.Manager.Ioc;

namespace EtherealS.Core.BaseCore
{
    public class MZCore : BaseCore
    {
        public IocManager IOCManager { get; set; } = new();
        public AbstractTypeManager Types { get; set; } = new();

    }
}
