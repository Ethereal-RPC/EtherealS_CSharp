using EtherealS.Core.Interface;

namespace EtherealS.Service.Interface
{
    public interface IToken : ILogEvent, IExceptionEvent
    {
        public void DisConnect(string reason);
    }
}
