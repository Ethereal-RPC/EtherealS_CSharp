using EtherealS.Core.Interface;

namespace EtherealS.NativeServer.Interface
{
    public interface IBaseToken:ILogEvent,IExceptionEvent
    {
        public void DisConnect(string reason);


    }
}
