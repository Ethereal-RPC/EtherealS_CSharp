using EtherealS.Core.Interface;

namespace EtherealS.Request.Interface
{
    public interface IRequest : ILogEvent, IExceptionEvent
    {
        public void Initialize();
        public void UnInitialize();
    }
}
