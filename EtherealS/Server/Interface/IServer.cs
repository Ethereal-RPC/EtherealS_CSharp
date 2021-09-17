using EtherealS.Core.Interface;

namespace EtherealS.Server.Interface
{
    interface IServer : ILogEvent, IExceptionEvent
    {
        public void Start();
        public void Close();
    }
}
