using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using EtherealS.Extension.Authority;
using EtherealS.Model;

namespace EtherealS.RPCService
{
    public class Service
    {
        //原作者的思想是Type调用Invoke，这里是在注册的时候就预存方法，1e6情况下调用速度的话是快了4-5倍左右，比正常调用慢10倍
        //猜测类似C++函数指针可能会更快,C#.NET理念下函数指针只能用委托替代，但委托自由度不高.
        //string连接的时候使用引用要比tuple慢很多
        private Dictionary<string, MethodInfo> methods = new Dictionary<string, MethodInfo>();
        private ServiceConfig config;
        private int paramStart;
        private object instance;
        public Dictionary<string, MethodInfo> Methods { get => methods; set => methods = value; }
        public ServiceConfig Config { get => config; set => config = value; }
        public object Instance { get => instance; set => instance = value; }

        public void Register(object instance,ServiceConfig config)
        {
            this.config = config;
            this.instance = instance;
            if (config.TokenEnable) paramStart = 1;
            else paramStart = 0;

            //检查权限接口是否开启并实现
            if (config.Authoritable && !(instance is IAuthoritable))
            {
                throw new RPCException($"{instance.GetType().FullName} 服务已开启权限系统，但尚未实现权限接口");
            }

            StringBuilder methodid = new StringBuilder();
            foreach (MethodInfo method in instance.GetType().GetMethods())
            {
                Annotation.RPCService rpcAttribute = method.GetCustomAttribute<Annotation.RPCService>();
                if (rpcAttribute != null)
                {
                    if (!method.IsAbstract)
                    {
                        methodid.Append(method.Name);
                        ParameterInfo[] parameters = method.GetParameters();
                        if (rpcAttribute.Paramters == null)
                        {
                            for (int i = paramStart; i < parameters.Length; i++)
                            {
                                if(i == 1 && (parameters[i].ParameterType.IsAssignableFrom(typeof(BaseUserToken))))
                                {
                                    throw new RPCException($"{method.Name}方法中的首参数并非继承于BaseUserToken!");
                                }

                                try
                                {
                                    methodid.Append("-" + config.Type.AbstractName[parameters[i].ParameterType]);
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
                            if(parameters.Length == types_name.Length)
                            {
                                for (int i = paramStart; i < parameters.Length; i++)
                                {
                                    if(config.Type.AbstractType.ContainsKey(types_name[i]))
                                    {
                                        methodid.Append("-").Append(types_name[i]);
                                    }
                                    else throw new RPCException($"C#对应的{types_name[i]}类型参数尚未注册"); 
                                }
                            }
                            else throw new RPCException($"方法体{method.Name}中[RPCMethod]与实际参数数量不符,[RPCMethod]:{types_name.Length}个,Method:{parameters.Length}个");
                        }
                        Methods.TryAdd(methodid.ToString(), method);
                        methodid.Length = 0;
                    }
                }
            }
        }

        public void ConvertParams(string methodId,object[] parameters)
        {
            string[] param_id = methodId.Split('-');
            if (param_id.Length > 1) {
                for (int i = paramStart; i < param_id.Length; i++)
                {
                    if (config.Type.TypeConvert.TryGetValue(param_id[i], out RPCType.ConvertDelegage convert))
                    {
                        parameters[i] = convert((string)parameters[i]);
                    }
                    else throw new RPCException($"RPC中的{param_id[i]}类型转换器在TypeConvert字典中尚未被注册");
                }
            }
        }
    }
}
