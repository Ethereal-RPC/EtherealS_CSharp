using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using EtherealC.Service.Interface;
using EtherealS.Core.Delegates;
using EtherealS.Core.Model;
using EtherealS.Server.Abstract;

namespace EtherealS.Service.Abstract
{
    public abstract class Service:Interface.IService
    {

        #region --委托字段--
        private OnLogDelegate logEvent;
        private OnExceptionDelegate exceptionEvent;
        public delegate bool InterceptorDelegate(Net.Abstract.Net net,Service service, MethodInfo method, BaseToken token);
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
        #endregion

        #region --属性--
        public Dictionary<string, MethodInfo> Methods { get => methods; set => methods = value; }
        public ServiceConfig Config { get => config; set => config = value; }
        public string NetName { get => netName; set => netName = value; }
        public string Name { get => name; set => name = value; }
        public AbstractTypes Types { get => types; set => types = value; }

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
            //遍历所有字段
            foreach (FieldInfo field in instance.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
            {
                Attribute.ServiceConfig rpcAttribute = field.GetCustomAttribute<Attribute.ServiceConfig>();

            }
            StringBuilder methodid = new StringBuilder();
            foreach (MethodInfo method in instance.GetType().GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
            {
                Attribute.Service rpcAttribute = method.GetCustomAttribute<Attribute.Service>();
                if (rpcAttribute != null)
                {
                    if (!method.IsAbstract)
                    {
                        methodid.Append(method.Name);
                        ParameterInfo[] parameters = method.GetParameters();
                        int start_idx = 1;
                        if (parameters.Length > 0 && (parameters[0].ParameterType.BaseType != typeof(BaseToken) && parameters[0].ParameterType != typeof(BaseToken))) start_idx = 0;
                        if (rpcAttribute.Paramters == null)
                        {
                            for (int i = start_idx; i < parameters.Length; i++)
                            {
                                try
                                {
                                    methodid.Append("-" + instance.types.TypesByType[parameters[i].ParameterType].Name);
                                }
                                catch (Exception)
                                {
                                    throw new TrackException($"{method.Name}方法中的{parameters[i].ParameterType}类型参数尚未注册");
                                }
                            }
                        }
                        else
                        {
                            string[] types_name = rpcAttribute.Paramters;
                            if (parameters.Length == types_name.Length + 1)
                            {
                                for (int i = 0; i < types_name.Length; i++)
                                {
                                    if (instance.Types.TypesByName.ContainsKey(types_name[i]))
                                    {
                                        methodid.Append("-").Append(types_name[i]);
                                    }
                                    else throw new TrackException($"C#对应的{types_name[i]}类型参数尚未注册");
                                }
                            }
                            else throw new TrackException($"方法体{method.Name}中[RPCMethod]与实际参数数量不符,[RPCMethod]:{types_name.Length + 1}个,Method:{parameters.Length}个");
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
        internal bool OnInterceptor(Net.Abstract.Net net,MethodInfo method, BaseToken token)
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
    }
}
