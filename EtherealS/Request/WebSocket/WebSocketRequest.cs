using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using EtherealS.Core.Model;
using EtherealS.Request.Abstract;
using EtherealS.Request.Attribute;
using EtherealS.Server.Abstract;
using EtherealS.Server.Attribute;

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
                if (args == null) throw new TrackException(TrackException.ErrorCode.Runtime, $"{name}-{methodid}首参并非BaseToken实现类！");
                Server.Abstract.Token token = null;
                ParameterInfo[] parameterInfos = targetMethod.GetParameters();
                //理想状态下为抛出Token的参数数量，但后期可能会存在不只是一个特殊类的问题，所以改为了动态数组。
                List<string> @params = new List<string>(parameterInfos.Length - 1);
                for (int i = 0;i < parameterInfos.Length;i++)
                {
                    if (parameterInfos[i].GetCustomAttribute<Server.Attribute.Token>(true) != null)
                    {
                        token = args[i] as Server.Abstract.Token;
                    }
                    else if (Types.TypesByType.TryGetValue(parameterInfos[i].ParameterType, out AbstractType type)
                    || Types.TypesByName.TryGetValue(parameterInfos[i].GetCustomAttribute<Core.Attribute.AbstractType>(true)?.AbstractName, out type))
                    {
                        methodid.Append("-" + type.Name);
                        @params.Add(type.Serialize(args[i]));
                    }
                    else throw new TrackException($"{targetMethod.Name}方法中的{parameterInfos[i].ParameterType}类型参数尚未注册");
                }
                ServerRequestModel request = new ServerRequestModel(Name, methodid.ToString(), @params.ToArray());
                if (token != null)
                {
                    if (token.CanRequest)
                    {
                        throw new TrackException(TrackException.ErrorCode.Runtime, $"{name}-{methodid}传递了非WebSocket协议的Token！");
                    }
                    token.SendServerRequest(request);
                    if ((rpcAttribute.InvokeType & Attribute.Request.InvokeTypeFlags.All) != 0)
                    {
                        localResult = targetMethod.Invoke(this, args);
                    }
                }
                else throw new TrackException(TrackException.ErrorCode.Runtime, $"{name}-{methodid}并未提供Token！");
            }
            else
            {
                localResult = targetMethod.Invoke(this, args);
            }
            return localResult;
        }
    }
}
