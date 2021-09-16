using EtherealS.Core.Interface;

namespace EtherealS.NativeServer.Interface
{
    interface IServer : ILogEvent, IExceptionEvent
    {
        public void Start();
        public void Close();
    }
}
