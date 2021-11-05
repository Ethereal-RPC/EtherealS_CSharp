using EtherealS.Core.Model;
using EtherealS.Net;
using EtherealS.Request.Abstract;
using EtherealS.Request.Interface;
using EtherealS.Request.WebSocket;

namespace EtherealS.Request
{
    public class RequestCore
    {

        public static bool Get<R>(string netName, string servicename, out R reqeust) where R:Abstract.Request
        {
            if (NetCore.Get(netName, out Net.Abstract.Net net))
            {
                return Get(net, servicename, out reqeust);
            }
            else
            {
                reqeust = null;
                return false;
            }
        }

        public static bool Get<R>(Net.Abstract.Net net, string servicename, out R reqeust) where R: Abstract.Request
        {
            bool result = net.Requests.TryGetValue(servicename, out Abstract.Request value);
            reqeust = (R)value;
            return result;
        }

        public static R Register<R,T>(Net.Abstract.Net net) where R:Abstract.Request
        {
            return Register<R, T>(net,null,null);
        }
        public static R Register<R,T>(Net.Abstract.Net net, string serviceName, AbstractTypes types) where R:Abstract.Request 
        {
            Abstract.Request request = Abstract.Request.Register<R, T>();
            if (serviceName != null) request.Name = serviceName;
            if (types != null) request.Types = types;
            if (!net.Requests.ContainsKey(request.Name))
            {
                request.Net = net;
                request.LogEvent += net.OnLog;
                request.ExceptionEvent += net.OnException;
                net.Requests[request.Name] = request;
                request.Initialize();
                return (R)request;
            }
            else throw new TrackException(TrackException.ErrorCode.Core, $"{net.Name}-{serviceName}已注册，无法重复注册！");
        }   
        public static bool UnRegister(Abstract.Request request)
        {
            request.Net.Requests.TryRemove(request.Name, out request);
            request.LogEvent -= request.Net.OnLog;
            request.ExceptionEvent -= request.Net.OnException;
            request.Net = null;
            request.UnInitialize();
            return true;
        }
    }
}
