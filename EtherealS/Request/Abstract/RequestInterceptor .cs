using Castle.DynamicProxy;
using EtherealS.Core.Manager.AbstractType;
using EtherealS.Core.Manager.Event.Attribute;
using EtherealS.Core.Model;
using EtherealS.Request.Attribute;
using EtherealS.Service.Abstract;
using EtherealS.Service.Attribute;
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
            RequestMappingAttribute attribute = method.GetCustomAttribute<RequestMappingAttribute>();
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
            EventSenderAttribute eventSender = null;
            EventContext eventContext = null;
            Service.Abstract.Token token = null;
            object localResult = null;
            request.Params = new Dictionary<string, string>(parameterInfos.Length - 1);
            Dictionary<string, object> @params = new Dictionary<string, object>(parameterInfos.Length);
            object[] args = invocation.Arguments;
            int idx = 0;
            foreach (ParameterInfo parameterInfo in parameterInfos)
            {
                if (parameterInfo.GetCustomAttribute<Service.Attribute.TokenParamAttribute>(true) != null)
                {
                    token = args[idx] as Service.Abstract.Token;
                }
                else
                {
                    instance.Types.Get(parameterInfo, out AbstractType type);
                    request.Params.Add(parameterInfo.Name, type.Serialize(args[idx]));
                }
                @params.Add(parameterInfo.Name, args[idx++]);
            }

            eventSender = method.GetCustomAttribute<BeforeEventAttribute>();
            if (eventSender != null)
            {
                eventContext = new BeforeEventContext(@params, method);
                instance.IOCManager.EventManager.InvokeEvent(instance.IOCManager.Get(eventSender.InstanceName), eventSender, @params, eventContext);
            }
            if (attribute.InvokeType.HasFlag(RequestMappingAttribute.InvokeTypeFlags.Local))
            {
                try
                {
                    invocation.Proceed();
                    localResult = invocation.ReturnValue;
                }
                catch(Exception e)
                {
                    eventSender = method.GetCustomAttribute<ExceptionEventAttribute>();
                    if (eventSender != null)
                    {
                        (eventSender as ExceptionEventAttribute).Exception = e;
                        eventContext = new ExceptionEventContext(@params, method, e);
                        instance.IOCManager.EventManager.InvokeEvent(instance.IOCManager.Get(eventSender.InstanceName), eventSender, @params, eventContext);
                        if ((eventSender as ExceptionEventAttribute).IsThrow) throw;
                    }
                    else throw;
                }
                localResult = invocation.ReturnValue;
            }
            eventSender = method.GetCustomAttribute<AfterEventAttribute>();
            if (eventSender != null)
            {
                eventContext = new AfterEventContext(@params, method, localResult);
                instance.IOCManager.EventManager.InvokeEvent(instance.IOCManager.Get(eventSender.InstanceName), eventSender, @params, eventContext);
            }
            if ((attribute.InvokeType & RequestMappingAttribute.InvokeTypeFlags.Remote) != 0)
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
        }
    }
}
