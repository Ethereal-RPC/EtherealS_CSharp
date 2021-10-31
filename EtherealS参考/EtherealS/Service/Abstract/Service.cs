using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using EtherealC.Service.Interface;
using EtherealS.Core.Delegates;
using EtherealS.Core.Model;
using EtherealS.Net.Extension.Plugins;
using EtherealS.Server.Abstract;
using EtherealS.Server.Attribute;
using EtherealS.Service.Attribute;

namespace EtherealS.Service.Abstract
{
    public abstract class Service:Interface.IService
    {
        #region --委托字段--
        private OnLogDelegate logEvent;
        private OnExceptionDelegate exceptionEvent;
        public delegate bool InterceptorDelegate(Net.Abstract.Net net,Service service, MethodInfo method, Server.Abstract.Token token);
        #endregion

        #region --委托属性--
        /// <summary>
        /// 网络级拦截器事件
        /// </summary>
        public event InterceptorDelegate InterceptorEvent;
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
        protected string netName;
        protected string name;
        protected AbstractTypes types;
        private ServicePluginManager pluginManager;
        #endregion

        #region --属性--
        public Dictionary<string, MethodInfo> Methods { get => methods; set => methods = value; }
        public ServiceConfig Config { get => config; set => config = value; }
        public string NetName { get => netName; set => netName = value; }
        public string Name { get => name; set => name = value; }
        public AbstractTypes Types { get => types; set => types = value; }
        protected ServicePluginManager PluginManager { get => pluginManager; set => pluginManager = value; }

        #endregion

        public void OnException(TrackException.ErrorCode code, string message)
        {
            OnException(new TrackException(code, message));
        }
        public void OnException(TrackException e)
        {
            if (exceptionEvent != null)
            {
                e.Service = this;
                exceptionEvent?.Invoke(e);
            }
        }

        public static void Register(Service instance)
        {
            StringBuilder methodid = new StringBuilder();
            foreach (MethodInfo method in instance.GetType().GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
            {
                Attribute.Service rpcAttribute = method.GetCustomAttribute<Attribute.Service>();
                if (rpcAttribute != null)
                {
                    if (!method.IsAbstract)
                    {
                        methodid.Append(method.Name);
                        ParameterInfo[] parameterInfos = method.GetParameters();
                        foreach (ParameterInfo parameterInfo in parameterInfos)
                        {
                            if (parameterInfo.GetCustomAttribute<Server.Attribute.Token>(true) != null)
                            {
                                continue;
                            }
                            else if (instance.Types.TypesByType.TryGetValue(parameterInfo.ParameterType, out AbstractType type)
                                || instance.Types.TypesByName.TryGetValue(parameterInfo.GetCustomAttribute<Core.Attribute.AbstractType>(true)?.AbstractName, out type))
                            {
                                methodid.Append("-" + type.Name);
                            }
                            else throw new TrackException($"{method.Name}方法中的{parameterInfo.ParameterType}类型参数尚未注册");
                        }
                        string name = methodid.ToString();
                        if (instance.methods.TryGetValue(name, out MethodInfo item))
                        {
                            throw new TrackException($"服务方法{name}已存在，无法重复注册！");
                        }
                        instance.methods.TryAdd(name, method);
                        methodid.Length = 0;
                    }
                }
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
                log.Service = this;
                logEvent?.Invoke(log);
            }
        }
        internal bool OnInterceptor(Net.Abstract.Net net,MethodInfo method, Server.Abstract.Token token)
        {
            if (InterceptorEvent != null)
            {
                foreach (InterceptorDelegate item in InterceptorEvent?.GetInvocationList())
                {
                    if (!item.Invoke(net,this, method, token)) return false;
                }
                return true;
            }
            else return true;
        }
        public abstract void Initialization();
        public abstract void UnInitialization();
    }
}
