using EtherealS.Core.Interface;

namespace EtherealS.Server.Interface
{
    public interface IBaseToken:ILogEvent,IExceptionEvent
    {
        public void DisConnect(string reason);
    }
}
