using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using EtherealS.Extension.Authority;
using EtherealS.Model;
using EtherealS.NativeServer;

namespace EtherealS.RPCService
{
    public class Service
    {
        private Dictionary<string, MethodInfo> methods = new Dictionary<string, MethodInfo>();
        private ServiceConfig config;
        private object instance;
        private string netName;
        private string name;

        public Dictionary<string, MethodInfo> Methods { get => methods; set => methods = value; }
        public ServiceConfig Config { get => config; set => config = value; }
        public object Instance { get => instance; set => instance = value; }
        public string NetName { get => netName; set => netName = value; }

        public string Name { get => name; set => name = value; }

        #region --委托--
        public delegate void OnExceptionDelegate(Exception exception, Service service);
        public delegate void OnLogDelegate(RPCLog log, Service service);
        #endregion

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

        public void Register(string netName, string service_name,object instance,ServiceConfig config)
        {
            this.config = config;
            this.instance = instance;
            this.netName = netName;
            this.name = service_name;
            //遍历所有字段
            foreach (FieldInfo field in instance.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
            {
                Attribute.Service.ServiceConfig rpcAttribute = field.GetCustomAttribute<Attribute.Service.ServiceConfig>();
                if (rpcAttribute != null)
                {
                    //将config赋值入该Service
                    field.SetValue(instance, config);
                }
            }
            StringBuilder methodid = new StringBuilder();
            foreach (MethodInfo method in instance.GetType().GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
            {
                Attribute.RPCService rpcAttribute = method.GetCustomAttribute<Attribute.RPCService>();
                if (rpcAttribute != null)
                {
                    if (!method.IsAbstract)
                    {
                        methodid.Append(method.Name);
                        ParameterInfo[] parameters = method.GetParameters();
                        int start_idx = 1;
                        if (parameters.Length > 0 && parameters[0].ParameterType.BaseType != typeof(BaseToken)) start_idx = 0;
                        if (rpcAttribute.Paramters == null)
                        {
                            for (int i = start_idx; i < parameters.Length; i++)
                            {   
                                try
                                {
                                    methodid.Append("-" + config.Types.TypesByType[parameters[i].ParameterType].Name);
                                }
                                catch (Exception)
                                {
                                    OnException(new RPCException($"{method.Name}方法中的{parameters[i].ParameterType}类型参数尚未注册"));
                                }
                            }
                        }
                        else
                        {
                            string[] types_name = rpcAttribute.Paramters;
                            if(parameters.Length == types_name.Length + 1)
                            {
                                for (int i = 0; i < types_name.Length; i++)
                                {
                                    if(config.Types.TypesByName.ContainsKey(types_name[i]))
                                    {
                                        methodid.Append("-").Append(types_name[i]);
                                    }
                                    else OnException(new RPCException($"C#对应的{types_name[i]}类型参数尚未注册")); 
                                }
                            }
                            else OnException(new RPCException($"方法体{method.Name}中[RPCMethod]与实际参数数量不符,[RPCMethod]:{types_name.Length + 1}个,Method:{parameters.Length}个"));
                        }
                        string name =  methodid.ToString();
                        if (methods.TryGetValue(name,out MethodInfo item))
                        {
                            OnException(new RPCException($"服务方法{name}已存在，无法重复注册！"));
                        }
                        Methods.TryAdd(name, method);
                        methodid.Length = 0;
                    }
                }
            }
        }
        public void OnException(RPCException.ErrorCode code, string message)
        {
            OnException(new RPCException(code, message));
        }
        public void OnException(Exception e)
        {
            if (exceptionEvent != null)
            {
                exceptionEvent.Invoke(e, this);
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
                logEvent.Invoke(log, this);
            }
        }
    }
}
