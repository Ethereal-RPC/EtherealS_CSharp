using System;
using System.Reflection;
using System.Text;
using EtherealS.Core.Model;
using EtherealS.Server.Abstract;
using EtherealS.Service.Abstract;

namespace EtherealS.Service.WebSocket
{
    public class WebSocketService:Abstract.Service
    {
        #region --属性--
        public new WebSocketServiceConfig Config { get => (WebSocketServiceConfig)config; set => config = value; }
        #endregion

        public override void Register(string netName, string service_name,object instance,ServiceConfig config)
        {
            this.config = config;
            this.instance = instance;   
            this.netName = netName;
            this.name = service_name;
            //遍历所有字段
            foreach (FieldInfo field in instance.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
            {
                Attribute.ServiceConfig rpcAttribute = field.GetCustomAttribute<Attribute.ServiceConfig>();
                if (rpcAttribute != null)
                {
                    //将config赋值入该Service
                    field.SetValue(instance, config);
                }
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
                        if (parameters.Length > 0 && (parameters[0].ParameterType.BaseType != typeof(Token) && parameters[0].ParameterType != typeof(Token))) start_idx = 0;
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
                                    throw new RPCException($"{method.Name}方法中的{parameters[i].ParameterType}类型参数尚未注册");
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
                                    else throw new RPCException($"C#对应的{types_name[i]}类型参数尚未注册"); 
                                }
                            }
                            else throw new RPCException($"方法体{method.Name}中[RPCMethod]与实际参数数量不符,[RPCMethod]:{types_name.Length + 1}个,Method:{parameters.Length}个");
                        }
                        string name =  methodid.ToString();
                        if (methods.TryGetValue(name,out MethodInfo item))
                        {
                            throw new RPCException($"服务方法{name}已存在，无法重复注册！");
                        }
                        Methods.TryAdd(name, method);
                        methodid.Length = 0;
                    }
                }
            }
        }
    }
}
