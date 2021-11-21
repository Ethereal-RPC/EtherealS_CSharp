using Castle.DynamicProxy;
using EtherealS.Core.Attribute;
using EtherealS.Core.Event.Attribute;
using EtherealS.Core.Model;
using EtherealS.Request.Attribute;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace EtherealS.Request.Abstract
{
    internal class RequestInterceptor : IInterceptor
    {
        private Random random = new Random();
        public void Intercept(IInvocation invocation)
        {
            MethodInfo method = invocation.Method;
            RequestMapping attribute = method.GetCustomAttribute<RequestMapping>();
            if (attribute == null)
            {
                invocation.Proceed();
                return;
            }
            Request instance = invocation.InvocationTarget as Request;
            //注入参数
            ParameterInfo[] parameterInfos = method.GetParameters();
            ServerRequestModel request = new ServerRequestModel();
            request.Mapping = attribute.Mapping;
            request.Params = new string[parameterInfos.Length];
            Dictionary<string,object> @params = new (parameterInfos.Length);
            EventSender eventSender = null;
            EventContext eventContext = null;
            Server.Abstract.Token token = null;
            object localResult = null;
            for (int i = 0, j = 0; i < parameterInfos.Length; i++)
            {
                if (parameterInfos[i].GetCustomAttribute<Server.Attribute.Token>(true) != null)
                {
                    token = invocation.Arguments[i] as Server.Abstract.Token;
                    continue;
                }
                Param paramAttribute = parameterInfos[i].GetCustomAttribute<Param>(true);
                if (paramAttribute != null && instance.Types.TypesByName.TryGetValue(paramAttribute.Name, out AbstractType type) || instance.Types.TypesByType.TryGetValue(parameterInfos[i].ParameterType, out type))
                {
                    request.Params[j++] = type.Serialize(invocation.Arguments[i]);
                    @params.Add(parameterInfos[i].Name, invocation.Arguments[i]);
                }
                else throw new TrackException($"{request.Mapping}方法中的{parameterInfos[i].ParameterType}类型参数尚未注册");
            }

            eventSender = method.GetCustomAttribute<BeforeEvent>();
            if (eventSender != null)
            {
                eventContext = new BeforeEventContext(@params, method);
                instance.EventManager.InvokeEvent(instance.IocContainer[eventSender.InstanceName], eventSender, @params, eventContext);
            }
            if (attribute.InvokeType.HasFlag(RequestMapping.InvokeTypeFlags.Local))
            {
                try
                {
                    invocation.Proceed();
                    localResult = invocation.ReturnValue;
                }
                catch(Exception e)
                {
                    eventSender = method.GetCustomAttribute<ExceptionEvent>();
                    if (eventSender != null)
                    {
                        (eventSender as ExceptionEvent).Exception = e;
                        eventContext = new ExceptionEventContext(@params, method, e);
                        instance.EventManager.InvokeEvent(instance.IocContainer[eventSender.InstanceName], eventSender, @params, eventContext);
                        if ((eventSender as ExceptionEvent).IsThrow) throw;
                    }
                    else throw;
                }
                localResult = invocation.ReturnValue;
            }
            if ((attribute.InvokeType & RequestMapping.InvokeTypeFlags.Remote) != 0)
            {
                if (token != null)
                {
                    if (!token.CanRequest)
                    {
                        throw new TrackException(TrackException.ErrorCode.Runtime, $"{instance.Name}-{request.Mapping}传递了非WebSocket协议的Token！");
                    }
                    token.SendServerRequest(request);
                }
                else throw new TrackException(TrackException.ErrorCode.Runtime, $"{instance.Name}-{request.Mapping}并未提供Token！");
            }
            eventSender = method.GetCustomAttribute<AfterEvent>();
            if (eventSender != null)
            {
                eventContext = new AfterEventContext(@params, method, localResult);
                instance.EventManager.InvokeEvent(instance.IocContainer[eventSender.InstanceName], eventSender, @params, eventContext);
            }
        }
    }
}
