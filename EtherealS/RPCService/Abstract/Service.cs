using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using EtherealS.Extension.Authority;
using EtherealS.Model;
using EtherealS.NativeServer;
using static EtherealS.Core.Delegate.Delegates;

namespace EtherealS.RPCService
{
    public abstract class Service
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
        protected Dictionary<string, MethodInfo> methods = new Dictionary<string, MethodInfo>();
        protected ServiceConfig config;
        protected object instance;
        protected string netName;
        protected string name;
        #endregion

        #region --属性--
        public Dictionary<string, MethodInfo> Methods { get => methods; set => methods = value; }
        public ServiceConfig Config { get => config; set => config = value; }
        public object Instance { get => instance; set => instance = value; }
        public string NetName { get => netName; set => netName = value; }
        public string Name { get => name; set => name = value; }
        #endregion
        public abstract void Register(string netName, string service_name, object instance, ServiceConfig config);
        public void OnException(RPCException.ErrorCode code, string message)
        {
            OnException(new RPCException(code, message));
        }
        public void OnException(Exception e)
        {
            if (exceptionEvent != null)
            {
                if (e is not RPCException)
                {
                    e = new RPCException(e, e.Message);
                }
                (e as RPCException).Service = this;
                exceptionEvent.Invoke(e);
            }
        }

        public void OnLog(RPCLog.LogCode code, string message)
        {
            OnLog(new RPCLog(code, message));
        }
        public void OnLog(RPCLog log)
        {
            if (logEvent != null)
            {
                log.Service = this;
                logEvent.Invoke(log);
            }
        }
    }
}
