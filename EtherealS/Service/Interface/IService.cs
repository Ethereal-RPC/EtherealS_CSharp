using EtherealS.Core.Interface;
using EtherealS.Service.Abstract;

namespace EtherealS.Service.Interface
{
    public interface IService : ILogEvent, IExceptionEvent
    {
        public void Initialize();
        public void UnInitialize();
    }
}
