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
        private object instance;
        private Tuple<string, string> clientkey;
        private string service_name;
        public Dictionary<string, MethodInfo> Methods { get => methods; set => methods = value; }
        public ServiceConfig Config { get => config; set => config = value; }
        public object Instance { get => instance; set => instance = value; }
        public Tuple<string, string> Clientkey { get => clientkey; set => clientkey = value; }

        public void Register(Tuple<string, string> clientkey, string service_name,object instance,ServiceConfig config)
        {
            this.config = config;
            this.instance = instance;
            this.clientkey = clientkey;
            this.service_name = service_name;

            StringBuilder methodid = new StringBuilder();
            foreach (MethodInfo method in instance.GetType().GetMethods())
            {
                Attribute.RPCService rpcAttribute = method.GetCustomAttribute<Attribute.RPCService>();
                if (rpcAttribute != null)
                {
                    if (!method.IsAbstract)
                    {
                        methodid.Append(method.Name);
                        ParameterInfo[] parameters = method.GetParameters();
                        if (rpcAttribute.Paramters == null)
                        {
                            for (int i = 1; i < parameters.Length; i++)
                            {   
                                try
                                {
                                    methodid.Append("-" + config.Types.TypesByType[parameters[i].ParameterType].Name);
                                }
                                catch (Exception)
                                {
                                    config.OnException(new RPCException($"{method.Name}方法中的{parameters[i].ParameterType}类型参数尚未注册"));
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
                                    else config.OnException(new RPCException($"C#对应的{types_name[i]}类型参数尚未注册")); 
                                }
                            }
                            else config.OnException(new RPCException($"方法体{method.Name}中[RPCMethod]与实际参数数量不符,[RPCMethod]:{types_name.Length + 1}个,Method:{parameters.Length}个"));
                        }
                        string name =  methodid.ToString();
                        if (methods.TryGetValue(name,out MethodInfo methodInfo))
                        {
                            config.OnException(new RPCException($"服务方法{name}已存在，无法重复注册！"));
                        }
                        Methods.TryAdd(name, method);
                        methodid.Length = 0;
                    }
                }
            }
        }
    }
}
