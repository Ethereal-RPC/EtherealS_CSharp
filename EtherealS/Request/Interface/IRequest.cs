using EtherealS.Core.Interface;

namespace EtherealS.Request.Interface
{
    public interface IRequest
    {
        void Initialize();
        void Register();
        void UnRegister();
        void UnInitialize();
    }
}
