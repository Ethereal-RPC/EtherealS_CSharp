using EtherealS.Core.Interface;

namespace EtherealS.RPCService
{
    public interface IService : ILogEvent, IExceptionEvent
    {
        public void Register(string netName, string service_name, object instance, ServiceConfig config);
    }
}
