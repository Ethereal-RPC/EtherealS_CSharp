using EtherealS.Model;
using EtherealS.RPCNet;
using Newtonsoft.Json;
using System;
using System.Reflection;
using System.Text;

namespace EtherealS.RPCRequest
{
    public class Request : DispatchProxy
    {
        private string servicename;
        private string netName;
        private RequestConfig config;
        public static Request Register<T>(string netName, string servicename, RequestConfig config)
        {
            Request proxy = Create<T, Request>() as Request;
            proxy.servicename = servicename;
            proxy.netName = netName ?? throw new ArgumentNullException(nameof(netName));
            proxy.config = config;
            return proxy;
        }
        protected override object Invoke(MethodInfo targetMethod, object[] args)    
        {
            Attribute.RPCRequest rpcAttribute = targetMethod.GetCustomAttribute<Attribute.RPCRequest>();
            if (rpcAttribute != null)
            {
                //这里要连接字符串，发现StringBuilder效率高一些.
                StringBuilder methodid = new StringBuilder(targetMethod.Name);
                string[] obj = null;
                int param_count;
                if (args != null) param_count = args.Length;
                else param_count = 0;
                if (param_count > 1)
                {
                    obj = new string[param_count - 1];
                    if (rpcAttribute.Paramters == null)
                    {
                        ParameterInfo[] parameters = targetMethod.GetParameters();
                        for (int i = 1; i < param_count; i++)
                        {
                            if(config.Types.TypesByType.TryGetValue(parameters[i].ParameterType,out RPCType type))
                            {
                                
                                methodid.Append("-" + type.Name);
                                obj[i - 1] = type.Serialize(args[i]);
                            }
                            else config.OnException(new RPCException($"C#对应的{args[i].GetType()}类型参数尚未注册"),this);
                        }
                    }
                    else
                    {
                        string[] types_name = rpcAttribute.Paramters;
                        if(param_count == types_name.Length)
                        {
                            for (int i = 1; i < param_count; i++)
                            {
                                if (config.Types.TypesByName.TryGetValue(types_name[i], out RPCType type))
                                {
                                    methodid.Append("-" + type.Name);
                                    obj[i - 1] = type.Serialize(args[i]);
                                }
                                else config.OnException(new RPCException($"C#对应的{args[i].GetType()}类型参数尚未注册"), this);
                            }
                        }
                        else config.OnException(new RPCException($"方法体{targetMethod.Name}中[RPCMethod]与实际参数数量不符,[RPCMethod]:{types_name.Length}个,Method:{param_count}个"), this);
                    }

                }
                ServerRequestModel request = new ServerRequestModel("2.0", servicename, methodid.ToString(), obj);
                if (args[0] != null && (args[0] as BaseUserToken).Net != null)
                {
                    if (!NetCore.Get(netName, out Net net))
                    {
                        config.OnException(new RPCException(RPCException.ErrorCode.Runtime,
                            $"{servicename}服务在发送请求时，NetConfig为空！"), this);
                    }

                    string log = "";
                    log += "---------------------------------------------------------\n";
                    log += $"{ DateTime.Now}::{netName}::[服 - 指令]\n{ request}\n";
                    log += "---------------------------------------------------------\n";
                    config.OnLog(RPCLog.LogCode.Runtime, log, this);

                    net.ServerRequestSend((args[0] as BaseUserToken), request);
                    return null;
                }
                return null;
            }
            return targetMethod.Invoke(this, args);
        }
    }
}
