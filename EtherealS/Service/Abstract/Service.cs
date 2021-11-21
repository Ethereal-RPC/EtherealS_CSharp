using EtherealS.Core;
using EtherealS.Core.EventManage;
using EtherealS.Core.EventManage.Attribute;
using EtherealS.Core.EventManage.Model;
using EtherealS.Core.Interface;
using EtherealS.Core.Model;
using EtherealS.Net.Extension.Plugins;
using EtherealS.Server.Abstract;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;

namespace EtherealS.Service.Abstract
{
    [Attribute.Service]
    public abstract class Service : Interface.IService, IBaseIoc
    {
        #region --委托字段--
        private OnLogDelegate logEvent;
        private OnExceptionDelegate exceptionEvent;

        public delegate bool InterceptorDelegate(Net.Abstract.Net net, Service service, MethodInfo method, Server.Abstract.Token token);
        /// <summary>
        /// BaseUserToken实例化方法委托
        /// </summary>
        /// <returns>BaseUserToken实例</returns>
        public delegate Token TokenCreateInstanceDelegate();
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
        protected Net.Abstract.Net net;
        protected string name;
        protected Dictionary<string, MethodInfo> methods = new Dictionary<string, MethodInfo>();
        protected ServiceConfig config;
        protected AbstractTypes types = new AbstractTypes();
        protected PluginDomain pluginDomain;
        /// <summary>
        /// 创建实例化方法委托实现
        /// </summary>
        protected TokenCreateInstanceDelegate tokenCreateInstance;
        /// <summary>
        /// Token映射表
        /// </summary>
        protected ConcurrentDictionary<object, Token> tokens = new ConcurrentDictionary<object, Token>();
        /// <summary>
        /// Reqeust映射表
        /// </summary>
        protected ConcurrentDictionary<string, Request.Abstract.Request> requests = new ConcurrentDictionary<string, Request.Abstract.Request>();
        #endregion

        #region --属性--
        public Dictionary<string, MethodInfo> Methods { get => methods; set => methods = value; }
        public ServiceConfig Config { get => config; set => config = value; }
        public Net.Abstract.Net Net { get => net; set => net = value; }
        public string Name { get => name; set => name = value; }
        public AbstractTypes Types { get => types; set => types = value; }
        public PluginDomain PluginDomain { get => pluginDomain; set => pluginDomain = value; }
        public ConcurrentDictionary<object, Token> Tokens { get => tokens; set => tokens = value; }
        public TokenCreateInstanceDelegate TokenCreateInstance { get => tokenCreateInstance; set => tokenCreateInstance = value; }
        public ConcurrentDictionary<string, Request.Abstract.Request> Requests { get => requests; set => requests = value; }
        internal protected Dictionary<string, object> IocContainer { get; set; } = new();
        public EventManager EventManager { get; set; } = new EventManager();

        #endregion

        #region --方法--
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
            foreach (MethodInfo method in instance.GetType().GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
            {
                Attribute.ServiceMethod attribute = method.GetCustomAttribute<Attribute.ServiceMethod>();
                if (attribute == null) continue;
                else if (attribute.Mapping == null) throw new TrackException(TrackException.ErrorCode.Runtime, $"{instance.GetType().FullName}-{method.Name}的Mapping未赋值！");
                //参数检查
                ParameterInfo[] parameterInfos = method.GetParameters();
                foreach (ParameterInfo parameterInfo in parameterInfos)
                {
                    if (parameterInfo.GetCustomAttribute<Server.Attribute.Token>(true) != null)
                    {
                        continue;
                    }
                    Core.Attribute.Param abstractTypeAttribute = parameterInfo.GetCustomAttribute<Core.Attribute.Param>(true);
                    if ((abstractTypeAttribute != null && instance.Types.TypesByName.ContainsKey(abstractTypeAttribute.Name))
                        || instance.Types.TypesByType.ContainsKey(parameterInfo.ParameterType))
                    {
                        continue;
                    }
                    else throw new TrackException($"{method.Name}方法中的{parameterInfo.ParameterType}类型参数尚未注册");
                }
                if (instance.methods.TryGetValue(attribute.Mapping, out MethodInfo item))
                {
                    throw new TrackException($"服务方法{attribute}已存在，无法重复注册！");
                }
                instance.methods.TryAdd(attribute.Mapping, method);
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
        internal bool OnInterceptor(Net.Abstract.Net net, MethodInfo method, Server.Abstract.Token token)
        {
            if (InterceptorEvent != null)
            {
                foreach (InterceptorDelegate item in InterceptorEvent?.GetInvocationList())
                {
                    if (!item.Invoke(net, this, method, token)) return false;
                }
                return true;
            }
            else return true;
        }
        public abstract void Initialize();
        public abstract void UnInitialize();
        public ClientResponseModel ClientRequestReceiveProcess(Token token, ClientRequestModel request)
        {
            EventContext eventContext;
            EventSender eventSender;
            Dictionary<string, object> @params = null;
            if (!Methods.TryGetValue(request.Mapping, out MethodInfo method))
            {
                return new ClientResponseModel(null, request.Id, new Error(Error.ErrorCode.NotFoundService, $"{Name}服务中{request.Mapping}未找到!", null));
            }
            try
            {
                if (Net.OnInterceptor(this, method, token) &&
                    OnInterceptor(Net, method, token))
                {
                    ParameterInfo[] parameterInfos = method.GetParameters();
                    List<object> parameters = new List<object>(parameterInfos.Length);
                    int i = 0;
                    @params = new(parameterInfos.Length);
                    foreach (ParameterInfo parameterInfo in parameterInfos)
                    {
                        if (parameterInfo.GetCustomAttribute<Server.Attribute.Token>(true) != null)
                        {
                            parameters.Add(token);
                            continue;
                        }
                        Core.Attribute.Param abstractTypeAttribute = parameterInfo.GetCustomAttribute<Core.Attribute.Param>(true);
                        if ((abstractTypeAttribute != null && Types.TypesByName.TryGetValue(abstractTypeAttribute.Name, out AbstractType type))
                            || Types.TypesByType.TryGetValue(parameterInfo.ParameterType, out type))
                        {
                            object param = type.Deserialize(request.Params[i++]);
                            parameters.Add(param);
                            @params.Add(parameterInfo.Name, param);
                        }
                        else return new ClientResponseModel(null, request.Id, new Error(Error.ErrorCode.Intercepted, $"RPC中的{request.Params[i]}类型中尚未被注册", null));
                    }
                    eventSender = method.GetCustomAttribute<BeforeEvent>();
                    if (eventSender != null)
                    {
                        eventContext = new BeforeEventContext(@params, method);
                        EventManager.InvokeEvent(IocContainer[eventSender.InstanceName], eventSender, @params, eventContext);
                    }
                    object result = null;
                    try
                    {
                        result = method.Invoke(this, parameters.ToArray());
                    }
                    catch (Exception e)
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
                    eventSender = method.GetCustomAttribute<AfterEvent>();
                    if (eventSender != null)
                    {
                        eventContext = new AfterEventContext(@params, method, result);
                        EventManager.InvokeEvent(IocContainer[eventSender.InstanceName], eventSender, @params, eventContext);
                    }
                    Type return_type = method.ReturnType;
                    if (return_type != typeof(void))
                    {
                        if (Types.TypesByType.TryGetValue(return_type, out AbstractType type)
                        || Types.TypesByName.TryGetValue(method.GetCustomAttribute<Core.Attribute.Param>(true)?.Name, out type))
                        {
                            return new ClientResponseModel(type.Serialize(result), request.Id, null);
                        }
                        else return new ClientResponseModel(null, request.Id, new Error(Error.ErrorCode.Intercepted, $"RPC中的{return_type}类型中尚未被注册", null));
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return new ClientResponseModel(null, request.Id, new Error(Error.ErrorCode.Intercepted, $"请求已被拦截", null));
                }
            }
            catch (TargetInvocationException e)
            {
                return new ClientResponseModel(null, request.Id, new Error(Error.ErrorCode.Common, $"{e.InnerException.Message}\n {e.InnerException.StackTrace}", null));
            }
            catch (Exception e)
            {
                return new ClientResponseModel(null, request.Id, new Error(Error.ErrorCode.Common, $"{e.Message}\n {e.StackTrace}", null));
            }
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
