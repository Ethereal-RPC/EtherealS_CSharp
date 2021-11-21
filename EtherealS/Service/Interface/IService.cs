using EtherealS.Core.Interface;
using EtherealS.Core.Model;
using EtherealS.Server.Abstract;

namespace EtherealS.Service.Interface
{
    public interface IService : ILogEvent, IExceptionEvent
    {
        public void Initialize();
        public void UnInitialize();
        public ClientResponseModel ClientRequestReceiveProcess(Token token, ClientRequestModel request);
    }
}
