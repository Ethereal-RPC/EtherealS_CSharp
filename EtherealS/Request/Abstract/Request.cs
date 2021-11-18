using System;
using System.Collections.Generic;
using System.Reflection;
using EtherealS.Core.Delegates;
using EtherealS.Core.Model;
using EtherealS.Request.Attribute;
using EtherealS.Request.Interface;
using EtherealS.Utils;

namespace EtherealS.Request.Abstract
{
    [Attribute.Request]
    public abstract class Request : IRequest
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
                RequestMethod attribute = method.GetCustomAttribute<RequestMethod>();
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
        public abstract object Invoke(string mapping, object[] args);
        public abstract void Initialize();
        public abstract void UnInitialize();
        #endregion
    }
}
