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

        public WebSocketRequest()
        {
            config = new WebSocketRequestConfig();
        }
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
                if (args == null) throw new TrackException(TrackException.ErrorCode.Runtime, $"{name}-{methodid}首参并非BaseToken实现类！");
                obj = new string[args.Length - 1];
                if (rpcAttribute.Paramters == null)
                {
                    ParameterInfo[] parameters = targetMethod.GetParameters();
                    for (int i = 1; i < args.Length; i++)
                    {
                        if (Types.TypesByType.TryGetValue(parameters[i].ParameterType, out AbstractType type))
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
                    if (args.Length == types_name.Length)
                    {
                        for (int i = 1; i < args.Length; i++)
                        {
                            if (Types.TypesByName.TryGetValue(types_name[i], out AbstractType type))
                            {
                                methodid.Append("-" + type.Name);
                                obj[i - 1] = type.Serialize(args[i]);
                            }
                            else throw new TrackException($"C#对应的{args[i].GetType()}类型参数尚未注册");
                        }
                    }
                    else throw new TrackException($"方法体{targetMethod.Name}中[RPCMethod]与实际参数数量不符,[RPCMethod]:{types_name.Length}个,Method:{args.Length}个");
                }
                ServerRequestModel request = new ServerRequestModel(Name, methodid.ToString(), obj);
                if (args?[0] != null && (args[0] is BaseToken))
                {
                    if (!((args[0] as BaseToken).CanRequest))
                    {
                        throw new TrackException(TrackException.ErrorCode.Runtime, $"{name}-{methodid}传递了非WebSocket协议的Token！");
                    }
                    (args[0] as BaseToken).SendServerRequest(request);
                    if ((rpcAttribute.InvokeType & Attribute.Request.InvokeTypeFlags.All) != 0)
                    {
                        localResult = targetMethod.Invoke(this, args);
                    }
                }
                else throw new TrackException(TrackException.ErrorCode.Runtime, $"{name}-{methodid}首参并非BaseToken实现类！");
            }
            else
            {
                localResult = targetMethod.Invoke(this, args);
            }
            return localResult;
        }
    }
}
