using EtherealS.Core.Interface;
using EtherealS.RPCService.Abstract;

namespace EtherealS.RPCService.Interface
{
    public interface IService : ILogEvent, IExceptionEvent
    {
        public void Register(string netName, string service_name, object instance, ServiceConfig config);
    }
}
