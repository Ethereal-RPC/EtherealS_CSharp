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
        private string requestname;
        private Tuple<string, string> serverkey;
        private RequestConfig config;
        public static Request Register<T>(string requestname, Tuple<string, string> clientkey, RequestConfig config)
        {
            Request proxy = Create<T, Request>() as Request;
            proxy.requestname = requestname;
            proxy.serverkey = clientkey ?? throw new ArgumentNullException(nameof(clientkey));
            proxy.config = config;
            return proxy;
        }
        protected override object Invoke(MethodInfo targetMethod, object[] args)    
        {
            Annotation.RPCRequest rpcAttribute = targetMethod.GetCustomAttribute<Annotation.RPCRequest>();
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
                            try
                            {
                                methodid.Append("-" + config.Type.AbstractName[parameters[i].ParameterType]);
                                obj[i - 1] = JsonConvert.SerializeObject(args[i]);
                            }
                            catch (Exception)
                            {
                                throw new RPCException($"C#对应的{args[i].GetType()}类型参数尚未注册");
                            }
                        }
                    }
                    else
                    {
                        string[] types_name = rpcAttribute.Paramters;
                        if(param_count == types_name.Length)
                        {
                            for (int i = 1; i < param_count; i++)
                            {
                                if (config.Type.AbstractType.ContainsKey(types_name[i]))
                                {
                                    methodid.Append("-" + types_name[i]);
                                    obj[i - 1] = JsonConvert.SerializeObject(args[i]);
                                }
                                else throw new RPCException($"C#对应的{types_name[i]}-{args[i].GetType()}类型参数尚未注册");
                            }
                        }
                        else throw new RPCException($"方法体{targetMethod.Name}中[RPCMethod]与实际参数数量不符,[RPCMethod]:{types_name.Length}个,Method:{param_count}个");
                    }

                }
                ServerRequestModel request = new ServerRequestModel("2.0", requestname, methodid.ToString(), obj);
                if (args[0] != null && (args[0] as BaseUserToken).Net != null)
                {
                    if (NetCore.Get(serverkey, out NetConfig netConfig))
                    {
                        throw new RPCException(RPCException.ErrorCode.RuntimeError,
                            $"{requestname}服务在发送请求时，NetConfig为空！");
                    }
                    netConfig.ServerRequestSend((args[0] as BaseUserToken), request);
                    return null;
                }
                return null;
            }
            return targetMethod.Invoke(this, args);
        }
    }
}
