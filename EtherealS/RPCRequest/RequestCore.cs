using System;
using System.Collections.Generic;
using EtherealS.Model;
using EtherealS.RPCNet;

namespace EtherealS.RPCRequest
{
    public class RequestCore
    {

        public static bool Get(string netName, string servicename, out Request reqeust)
        {
            if (NetCore.Get(netName, out Net net))
            {
                return Get(net, servicename, out reqeust);
            }
            else
            {
                reqeust = null;
                return false;
            }
        }
        public static bool Get(Net net, string servicename, out Request reqeust)
        {
            return net.Requests.TryGetValue(servicename, out reqeust);
        }
        public static R Register<R>(Net net, string servicename, RPCTypeConfig type)
        {
            RequestConfig config = new RequestConfig(type);
            return Register<R>(net, servicename, config);
        }

        public static R Register<R>(Net net, string servicename, RequestConfig config)
        {
            net.Requests.TryGetValue(servicename, out Request request);
            if (request == null)
            {
                request = Request.Register<R>(net.Name, servicename, config);
                net.Requests[servicename] = request;
                request.LogEvent += net.OnRequestLog;
                request.ExceptionEvent += net.OnRequestException;
            }
            else throw new RPCException(RPCException.ErrorCode.Core, $"{net.Name}-{servicename}已注册，无法重复注册！");
            return (R)(request as object);
        }
        public static bool UnRegister(string netName,string serviceName)
        {
            if (NetCore.Get(netName, out Net net))
            {
                return UnRegister(net, serviceName);
            }
            return true;
        }
        public static bool UnRegister(Net net, string serviceName)
        {
            if(net != null)
            {
                net.Requests.Remove(serviceName, out Request request);
                if (request != null)
                {
                    request.LogEvent -= net.OnRequestLog;
                    request.ExceptionEvent -= net.OnRequestException;
                }
            }
            return true;
        }
    }
}
