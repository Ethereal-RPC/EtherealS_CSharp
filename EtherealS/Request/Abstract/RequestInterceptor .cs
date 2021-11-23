using Castle.DynamicProxy;
using EtherealS.Core.Manager.AbstractType;
using EtherealS.Core.Manager.Event.Attribute;
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
            EventSender eventSender = null;
            EventContext eventContext = null;
            Server.Abstract.Token token = null;
            object localResult = null;
            request.Params = new Dictionary<string, string>(parameterInfos.Length);
            Dictionary<string, object> @params = new Dictionary<string, object>(parameterInfos.Length);
            int idx = 0;
            foreach (ParameterInfo parameterInfo in parameterInfos)
            {
                if (parameterInfo.GetCustomAttribute<Server.Attribute.Token>(true) != null)
                {
                    @params.Add(parameterInfo.Name, invocation.Arguments[idx++]);
                    continue;
                }
                instance.Types.Get(parameterInfo, out AbstractType type);
                request.Params.Add(parameterInfo.Name, type.Serialize(invocation.Arguments[idx]));
                @params.Add(parameterInfo.Name, invocation.Arguments[idx++]);
            }

            eventSender = method.GetCustomAttribute<BeforeEvent>();
            if (eventSender != null)
            {
                eventContext = new BeforeEventContext(@params, method);
                instance.IOCManager.EventManager.InvokeEvent(instance.IOCManager.Get(eventSender.InstanceName), eventSender, @params, eventContext);
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
                        instance.IOCManager.EventManager.InvokeEvent(instance.IOCManager.Get(eventSender.InstanceName), eventSender, @params, eventContext);
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
                instance.IOCManager.EventManager.InvokeEvent(instance.IOCManager.Get(eventSender.InstanceName), eventSender, @params, eventContext);
            }
        }
    }
}
