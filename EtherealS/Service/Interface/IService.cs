using EtherealS.Core.Interface;
using EtherealS.Core.Model;
using EtherealS.Server.Abstract;

namespace EtherealS.Service.Interface
{
    public interface IService
    {
        void Initialize();
        void Register();
        void UnRegister();
        void UnInitialize();
    }
}
