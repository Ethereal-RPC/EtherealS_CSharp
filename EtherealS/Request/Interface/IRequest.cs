using EtherealS.Core.Interface;

namespace EtherealS.Request.Interface
{
    public interface IRequest : ILogEvent, IExceptionEvent
    {
        void Initialize();
        void Register();
        void UnRegister();
        void UnInitialize();
    }
}
