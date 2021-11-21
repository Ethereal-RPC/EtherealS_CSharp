using EtherealS.Core;
using EtherealS.Core.Attribute;
using EtherealS.Core.Event;
using EtherealS.Core.Event.Attribute;
using EtherealS.Core.Event.Model;
using EtherealS.Core.Interface;
using EtherealS.Core.Model;
using EtherealS.Request.Attribute;
using EtherealS.Request.Interface;
using EtherealS.Utils;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace EtherealS.Request.Abstract
{
    [Attribute.Request]
    public abstract class Request : IRequest, IBaseIoc
    {

        #region --事件字段--
        private OnLogDelegate logEvent;
        private OnExceptionDelegate exceptionEvent;
        #endregion

        #region --事件属性--
        /// <summary>
        /// 日志输出事件
        /// </summary>
        public event OnLogDelegate LogEvent
        {
            add
            {
                logEvent -= value;
                logEvent += value;
            }
            remove
            {
                logEvent -= value;
            }
        }
        /// <summary>
        /// 抛出异常事件
        /// </summary>
        public event OnExceptionDelegate ExceptionEvent
        {
            add
            {
                exceptionEvent -= value;
                exceptionEvent += value;
            }
            remove
            {
                exceptionEvent -= value;
            }

        }
        #endregion

        #region --字段--
        protected string name;
        protected Service.Abstract.Service service;
        protected RequestConfig config;
        private Dictionary<string, MethodInfo> methods = new Dictionary<string, MethodInfo>();
        protected AbstractTypes types = new AbstractTypes();
        #endregion

        #region --属性--
        public string Name { get => name; set => name = value; }
        public RequestConfig Config { get => config; set => config = value; }
        public AbstractTypes Types { get => types; set => types = value; }
        public Service.Abstract.Service Service { get => service; set => service = value; }
        protected Dictionary<string, MethodInfo> Methods { get => methods; set => methods = value; }

        internal protected Dictionary<string, object> IocContainer { get; set; } = new();
        public EventManager EventManager { get; set; } = new EventManager();

        #endregion

        #region --方法--
        public Request()
        {

        }
        public static R Register<R>() where R : Request
        {
            R request = DynamicProxy.CreateRequestProxy<R>();
            foreach (MethodInfo method in typeof(R).GetMethods())
            {
                RequestMapping attribute = method.GetCustomAttribute<RequestMapping>();
                if (attribute != null)
                {
                    request.Methods.Add(attribute.Mapping, method);
                }
            }
            return request;
        }

        public void OnException(TrackException.ErrorCode code, string message)
        {
            OnException(new TrackException(code, message));
        }
        public void OnException(TrackException e)
        {
            if (exceptionEvent != null)
            {
                e.Request = this;
                exceptionEvent?.Invoke(e);
            }
        }

        public void OnLog(TrackLog.LogCode code, string message)
        {
            OnLog(new TrackLog(code, message));
        }
        public void OnLog(TrackLog log)
        {
            if (logEvent != null)
            {
                log.Request = this;
                logEvent?.Invoke(log);
            }
        }
        public abstract void Initialize();
        public abstract void UnInitialize();

        public virtual object Invoke(string mapping, object[] args,object localResult)
        {
            MethodInfo method = null;
            Dictionary<string, object> @params = null;
            EventSender eventSender;
            EventContext eventContext;
            try
            {
                //方法信息获取
                Methods.TryGetValue(mapping, out method);
                RequestMapping attribute = method.GetCustomAttribute<RequestMapping>();
                //注入参数
                ParameterInfo[] parameterInfos = method.GetParameters();
                ServerRequestModel request = new ServerRequestModel();
                request.Mapping = attribute.Mapping;
                request.Params = new string[parameterInfos.Length];
                @params = new(parameterInfos.Length -1);
                Server.Abstract.Token token = null;
                for (int i = 0, j = 0; i < parameterInfos.Length; i++)
                {
                    if (parameterInfos[i].GetCustomAttribute<Server.Attribute.Token>(true) != null)
                    {
                        token = args[i] as Server.Abstract.Token;
                        continue;
                    }
                    Param paramAttribute = parameterInfos[i].GetCustomAttribute<Param>(true);
                    if (paramAttribute != null && Types.TypesByName.TryGetValue(paramAttribute.Name, out AbstractType type) || Types.TypesByType.TryGetValue(parameterInfos[i].ParameterType, out type))
                    {
                        request.Params[j++] = type.Serialize(args[i]);
                        @params.Add(parameterInfos[i].Name, args[i]);
                    }
                    else throw new TrackException($"{request.Mapping}方法中的{parameterInfos[i].ParameterType}类型参数尚未注册");
                }

                eventSender = method.GetCustomAttribute<BeforeEvent>();
                if (eventSender != null)
                {
                    eventContext = new BeforeEventContext(@params, method);
                    EventManager.InvokeEvent(IocContainer[eventSender.InstanceName], eventSender, @params, eventContext);
                }
                if ((attribute.InvokeType & RequestMapping.InvokeTypeFlags.Remote) != 0)
                {
                    if (token != null)
                    {
                        if (!token.CanRequest)
                        {
                            throw new TrackException(TrackException.ErrorCode.Runtime, $"{name}-{request.Mapping}传递了非WebSocket协议的Token！");
                        }
                        token.SendServerRequest(request);
                    }
                    else throw new TrackException(TrackException.ErrorCode.Runtime, $"{name}-{request.Mapping}并未提供Token！");
                }
                eventSender = method.GetCustomAttribute<AfterEvent>();
                if (eventSender != null)
                {
                    eventContext = new AfterEventContext(@params, method, localResult);
                    EventManager.InvokeEvent(IocContainer[eventSender.InstanceName], eventSender, @params, eventContext);
                }
            }
            catch(Exception e)
            {
                eventSender = method.GetCustomAttribute<ExceptionEvent>();
                if (eventSender != null)
                {
                    (eventSender as ExceptionEvent).Exception = e;
                    eventContext = new ExceptionEventContext(@params, method, e);
                    EventManager.InvokeEvent(IocContainer[eventSender.InstanceName], eventSender, @params, eventContext);
                    if ((eventSender as ExceptionEvent).IsThrow) throw;
                }
                else throw;
            }
            return localResult;
        }


        public void RegisterIoc(string name, object instance)
        {
            if (IocContainer.ContainsKey(name))
            {
                throw new TrackException(TrackException.ErrorCode.Runtime, $"{Name}请求中的{name}实例已注册");
            }
            IocContainer.Add(name, instance);
        }
        public void UnRegisterIoc(string name)
        {
            if (IocContainer.TryGetValue(name, out object instance))
            {
                IocContainer.Remove(name);
                EventManager.UnRegisterEventMethod(name, instance);
            }
        }
        public bool GetIocObject(string name, out object instance)
        {
            return IocContainer.TryGetValue(name, out instance);
        }
        #endregion
    }
}
