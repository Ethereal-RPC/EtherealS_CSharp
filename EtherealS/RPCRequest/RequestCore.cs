using System;
using System.Collections.Generic;
using EtherealS.Core.Model;
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
                if (net.NetType == Core.Enums.NetType.WebSocket)
                {
                    request = WebSocketRequest.Register<R>(net.Name, servicename, config);
                }
                else throw new RPCException(RPCException.ErrorCode.Core, $"未有针对{net.NetType}的Request-Register处理");
                net.Requests[servicename] = request;
                request.LogEvent += net.OnLog;
                request.ExceptionEvent += net.OnException;
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
