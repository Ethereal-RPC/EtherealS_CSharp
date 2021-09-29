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

        public static R Register<R,T>(Net.Abstract.Net net, string servicename, AbstractTypes types, RequestConfig config=null) where R:Abstract.Request 
        {
            net.Requests.TryGetValue(servicename, out Request.Abstract.Request request);
            if (request == null)
            {
                request = Abstract.Request.Register<R, T>(net.Name, servicename,types, config);
                net.Requests[servicename] = request;
                request.LogEvent += net.OnLog;
                request.ExceptionEvent += net.OnException;
            }
            else throw new TrackException(TrackException.ErrorCode.Core, $"{net.Name}-{servicename}已注册，无法重复注册！");
            return (R)request;
        }
        public static bool UnRegister(string netName,string serviceName)
        {
            if (NetCore.Get(netName, out Net.Abstract.Net net))
            {
                return UnRegister(net, serviceName);
            }
            return true;
        }
        public static bool UnRegister(Net.Abstract.Net net, string serviceName)
        {
            if(net != null)
            {
                net.Requests.Remove(serviceName, out Request.Abstract.Request request);
                if(request != null)
                {
                    request.LogEvent -= net.OnLog;
                    request.ExceptionEvent -= net.OnException;
                }
            }
            return true;
        }
    }
}
