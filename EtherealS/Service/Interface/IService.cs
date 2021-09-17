using EtherealS.Core.Interface;
using EtherealS.Service.Abstract;

namespace EtherealS.Service.Interface
{
    public interface IService : ILogEvent, IExceptionEvent
    {
        public void Register(string netName, string service_name, object instance, ServiceConfig config);
    }
}
