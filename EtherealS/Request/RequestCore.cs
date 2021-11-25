using Castle.DynamicProxy;
using EtherealS.Core.Model;
using EtherealS.Request.Abstract;

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

        public static T Register<T>(Service.Abstract.Service service, string serviceName = null) where T : Abstract.Request
        {
            ProxyGenerator generator = new ProxyGenerator();
            RequestInterceptor interceptor = new RequestInterceptor();
            T request = generator.CreateClassProxy<T>(interceptor);
            request.Initialize();
            if (serviceName != null) request.name = serviceName;
            if (!request.IsRegister)
            {
                request.isRegister = true;
                Abstract.Request.Register(request);
                request.Service = service;
                request.LogEvent += service.OnLog;
                request.ExceptionEvent += service.OnException;
                service.Requests[request.Name] = request;
                request.Register();
                return request;
            }
            else throw new TrackException(TrackException.ErrorCode.Core, $"{service.Net.Name}-{service.Name}-{serviceName}已注册，无法重复注册！");
        }
        public static bool UnRegister(Abstract.Request request)
        {
            if (request.IsRegister)
            {
                request.UnRegister();
                if(request.Service != null)
                {
                    request.Service.Requests.TryRemove(request.Name, out request);
                    request.LogEvent -= request.Service.OnLog;
                    request.ExceptionEvent -= request.Service.OnException;
                    request.Service = null;
                }
                request.UnInitialize();
                request.isRegister = false;
                return true;
            }
            else throw new TrackException(TrackException.ErrorCode.Core, $"{request.Name}并未注册，无需UnRegister");
        }
    }
}
