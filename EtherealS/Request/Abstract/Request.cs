using Castle.DynamicProxy;
using EtherealS.Core;
using EtherealS.Core.Attribute;
using EtherealS.Core.Event;
using EtherealS.Core.Event.Attribute;
using EtherealS.Core.Interface;
using EtherealS.Core.Model;
using EtherealS.Request.Attribute;
using EtherealS.Request.Interface;
using EtherealS.Utils;
using System;
using System.Collections.Generic;
using System.Reflection;

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
        public static T Register<T>() where T : Request
        {
            ProxyGenerator generator = new ProxyGenerator();
            RequestInterceptor interceptor = new RequestInterceptor();
            T request = generator.CreateClassProxy<T>(interceptor);
            foreach (MethodInfo method in typeof(T).GetMethods())
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
