using EtherealS.Core.Model;

namespace EtherealS.Request
{
    public class RequestCore
    {
        public static bool Get<R>(Service.Abstract.Service service, string request_name, out R reqeust) where R : Abstract.Request
        {
            bool result = service.Requests.TryGetValue(request_name, out Abstract.Request value);
            reqeust = (R)value;
            return result;
        }

        public static R Register<R>(Service.Abstract.Service service, string serviceName = null) where R : Abstract.Request
        {
            R request = Abstract.Request.Register<R>();
            if (serviceName != null) request.Name = serviceName;
            if (!service.Requests.ContainsKey(request.Name))
            {
                request.Service = service;
                request.LogEvent += service.OnLog;
                request.ExceptionEvent += service.OnException;
                service.Requests[request.Name] = request;
                request.Initialize();
                return request;
            }
            else throw new TrackException(TrackException.ErrorCode.Core, $"{service.Net.Name}-{service.Name}-{serviceName}已注册，无法重复注册！");
        }
        public static bool UnRegister(Abstract.Request request)
        {
            request.UnInitialize();
            request.Service.Requests.TryRemove(request.Name, out request);
            request.LogEvent -= request.Service.OnLog;
            request.ExceptionEvent -= request.Service.OnException;
            request.Service = null; 
            return true;
        }
    }
}
