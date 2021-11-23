using EtherealS.Core;
using EtherealS.Core.Attribute;
using EtherealS.Core.BaseCore;
using EtherealS.Core.Manager.AbstractType;
using EtherealS.Core.Manager.Event.Attribute;
using EtherealS.Core.Model;
using EtherealS.Net.Extension.Plugins;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;

namespace EtherealS.Service.Abstract
{
    [Attribute.Service]
    public abstract class Service : MZCore,Interface.IService
    {
        #region --委托字段--
        public delegate bool InterceptorDelegate(Net.Abstract.Net net, Service service, MethodInfo method, Token token);
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
        #endregion

        #region --字段--
        internal string name;
        protected Net.Abstract.Net net;
        protected Dictionary<string, MethodInfo> methods = new Dictionary<string, MethodInfo>();
        protected ServiceConfig config;
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
        public PluginDomain PluginDomain { get => pluginDomain; set => pluginDomain = value; }
        public ConcurrentDictionary<object, Token> Tokens { get => tokens; set => tokens = value; }
        public TokenCreateInstanceDelegate TokenCreateInstance { get => tokenCreateInstance; set => tokenCreateInstance = value; }
        public ConcurrentDictionary<string, Request.Abstract.Request> Requests { get => requests; set => requests = value; }
        public bool Enable { get; set; } = true;
        public string Name { get => name; set => name = value; }
        #endregion

        #region --方法--

        internal static void Register(Service instance)
        {
            foreach (MethodInfo method in instance.GetType().GetMethods())
            {
                Attribute.ServiceMapping attribute = method.GetCustomAttribute<Attribute.ServiceMapping>();
                if (attribute != null)
                {
                    if (method.ReturnType != typeof(void))
                    {
                        Param paramAttribute = method.GetCustomAttribute<Param>();
                        if (paramAttribute != null && !instance.Types.Get(paramAttribute.Type, out AbstractType type))
                        {
                            throw new TrackException(TrackException.ErrorCode.Core, $"{instance.Name}-{method.Name}-{paramAttribute.Type}抽象类型未找到");
                        }
                        else if (!instance.Types.Get(method.ReturnType, out type))
                        {
                            throw new TrackException(TrackException.ErrorCode.Core, $"{instance.Name}-{method.Name}-{method.ReturnType}类型映射抽象类型");
                        }
                    }
                    ParameterInfo[] parameterInfos = method.GetParameters();
                    foreach (ParameterInfo parameterInfo in parameterInfos)
                    {
                        BaseParam paramsAttribute = parameterInfo.GetCustomAttribute<BaseParam>(true);
                        if (paramsAttribute != null)
                        {
                            continue;
                        }
                        Param paramAttribute = parameterInfo.GetCustomAttribute<Param>();
                        if (paramAttribute != null && !instance.Types.Get(paramAttribute.Type, out AbstractType type))
                        {   
                            throw new TrackException(TrackException.ErrorCode.Core, $"{instance.Name}-{method.Name}-{paramAttribute.Type}抽象类型未找到");
                        }
                        else if (!instance.Types.Get(parameterInfo.ParameterType, out type))
                        {
                            throw new TrackException(TrackException.ErrorCode.Core, $"{instance.Name}-{method.Name}-{parameterInfo.ParameterType}类型映射抽象类型");
                        }
                    }
                    instance.methods.TryAdd(attribute.Mapping, method);
                }
            }
        }

        internal bool OnInterceptor(Net.Abstract.Net net, MethodInfo method, Token token)
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
        public ClientResponseModel ClientRequestReceiveProcess(Token token, ClientRequestModel request)
        {
            ClientResponseModel response = new();
            response.Id = request.Id;
            if (!Methods.TryGetValue(request.Mapping, out MethodInfo method))
            {
                response.Error = new Error(Error.ErrorCode.NotFoundService, $"{Name}服务中{request.Mapping}未找到!");
                return response;
            }
            try
            {
                if (Net.OnInterceptor(this, method, token) &&
                    OnInterceptor(Net, method, token))
                {
                    EventContext eventContext;
                    EventSender eventSender;
                    ParameterInfo[] parameterInfos = method.GetParameters();
                    Dictionary<string, object> @params = new Dictionary<string, object>(parameterInfos.Length);
                    object[] args = new object[parameterInfos.Length];
                    int idx = 0;
                    foreach (ParameterInfo parameterInfo in parameterInfos)
                    {
                        if (parameterInfo.GetCustomAttribute<Attribute.Token>(true) != null)
                        {
                            args[idx] = token;
                        }
                        else if (request.Params.TryGetValue(parameterInfo.Name, out string value))
                        {
                            Types.Get(parameterInfo, out AbstractType type);
                            args[idx] = type.Deserialize(value);
                        }
                        else throw new TrackException(TrackException.ErrorCode.Runtime, $"来自服务器的{Name}服务请求中未提供{method.Name}方法的{parameterInfo.Name}参数");
                        @params.Add(parameterInfo.Name, args[idx++]);
                    }
                    eventSender = method.GetCustomAttribute<BeforeEvent>();
                    if (eventSender != null)
                    {
                        eventContext = new BeforeEventContext(@params, method);
                        IOCManager.EventManager.InvokeEvent(IOCManager.Get(eventSender.InstanceName), eventSender, @params, eventContext);
                    }
                    object result = null;
                    try
                    {
                        result = method.Invoke(this, args);
                    }
                    catch (Exception e)
                    {
                        eventSender = method.GetCustomAttribute<ExceptionEvent>();
                        if (eventSender != null)
                        {
                            (eventSender as ExceptionEvent).Exception = e;
                            eventContext = new ExceptionEventContext(@params, method, e);
                            IOCManager.EventManager.InvokeEvent(IOCManager.Get(eventSender.InstanceName), eventSender, @params, eventContext);
                            if ((eventSender as ExceptionEvent).IsThrow) throw;
                        }
                        else throw;
                    }
                    eventSender = method.GetCustomAttribute<AfterEvent>();
                    if (eventSender != null)
                    {
                        eventContext = new AfterEventContext(@params, method, result);
                        IOCManager.EventManager.InvokeEvent(IOCManager.Get(eventSender.InstanceName), eventSender, @params, eventContext);
                    }
                    Type return_type = method.ReturnType;
                    if (return_type != typeof(void))
                    {
                        Types.Get(method.GetCustomAttribute<Param>()?.Type, return_type, out AbstractType type);
                        response.Result = type.Serialize(result);
                        return response;
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    response.Error = new Error(Error.ErrorCode.Intercepted, $"请求已被拦截");
                    return response;
                }
            }
            catch (TargetInvocationException e)
            {
                response.Error = new Error(Error.ErrorCode.Common, $"{e.InnerException.Message}\n {e.InnerException.StackTrace}");
                return response;
            }
            catch (Exception e)
            {
                response.Error = new Error(Error.ErrorCode.Intercepted, $"{e.Message}\n {e.StackTrace}");
                return response;
            }
        }
        public abstract void Register();
        public abstract void UnRegister();
        public abstract void Initialize();
        public abstract void UnInitialize();
        #endregion

    }
}
