using EtherealS.Core.Interface;

namespace EtherealS.Server.Interface
{
    public interface IToken : ILogEvent, IExceptionEvent
    {
        public void DisConnect(string reason);
    }
}
