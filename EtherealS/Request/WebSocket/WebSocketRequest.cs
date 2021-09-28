using System;
using System.Reflection;
using System.Text;
using EtherealS.Core.Model;
using EtherealS.Request.Abstract;
using EtherealS.Server.Abstract;

namespace EtherealS.Request.WebSocket
{
    public class WebSocketRequest : Abstract.Request
    {
        #region --属性--

        public new WebSocketRequestConfig Config { get => (WebSocketRequestConfig)config; set => config = value; }

        #endregion

        protected override object Invoke(MethodInfo targetMethod, object[] args)    
        {
            Attribute.Request rpcAttribute = targetMethod.GetCustomAttribute<Attribute.Request>();
            if (rpcAttribute == null)
            {
                return targetMethod.Invoke(this, args);
            }
            object localResult = null;
            if ((rpcAttribute.InvokeType & Attribute.Request.InvokeTypeFlags.Local)!=0)
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
                            if(Config.Types.TypesByType.TryGetValue(parameters[i].ParameterType,out AbstractType type))
                            {
                                
                                methodid.Append("-" + type.Name);
                                obj[i - 1] = type.Serialize(args[i]);
                            }
                            else throw new TrackException($"C#对应的{args[i].GetType()}类型参数尚未注册");
                        }
                    }
                    else
                    {
                        string[] types_name = rpcAttribute.Paramters;
                        if(param_count == types_name.Length)
                        {
                            for (int i = 1; i < param_count; i++)
                            {
                                if (Config.Types.TypesByName.TryGetValue(types_name[i], out AbstractType type))
                                {
                                    methodid.Append("-" + type.Name);
                                    obj[i - 1] = type.Serialize(args[i]);
                                }
                                else throw new TrackException($"C#对应的{args[i].GetType()}类型参数尚未注册") ;
                            }
                        }
                        else throw new TrackException($"方法体{targetMethod.Name}中[RPCMethod]与实际参数数量不符,[RPCMethod]:{types_name.Length}个,Method:{param_count}个") ;
                    }

                }
                ServerRequestModel request = new ServerRequestModel(Name, methodid.ToString(), obj);
                if (args[0] != null && (args[0] as BaseToken != null))
                {
                    if (!((args[0] as BaseToken).IsWebSocket))
                    {
                        throw new TrackException(TrackException.ErrorCode.Runtime, $"{name}-{methodid}传递了非WebSocket协议的Token！");
                    }
                    (args[0] as BaseToken).SendServerRequest(request);
                    if ((rpcAttribute.InvokeType & Attribute.Request.InvokeTypeFlags.All) != 0)
                    {
                        localResult = targetMethod.Invoke(this, args);
                    }
                }
                throw new TrackException(TrackException.ErrorCode.Runtime, $"{name}-{methodid}！");
            }
            else
            {
                localResult = targetMethod.Invoke(this, args);
            }

            return localResult;
        }
    }
}
