using Castle.DynamicProxy;
using EtherealS.Core;
using EtherealS.Core.Attribute;
using EtherealS.Core.BaseCore;
using EtherealS.Core.Manager.AbstractType;
using EtherealS.Core.Model;
using EtherealS.Request.Interface;
using System.Collections.Generic;
using System.Reflection;

namespace EtherealS.Request.Abstract
{
    [Attribute.Request]
    public abstract class Request : MZCore,IRequest
    {

        #region --事件字段--

        #endregion

        #region --事件属性--

        #endregion

        #region --字段--

        internal string name;
        protected Service.Abstract.Service service;
        protected RequestConfig config;
        private Dictionary<string, MethodInfo> methods = new Dictionary<string, MethodInfo>();
        #endregion

        #region --属性--
        public RequestConfig Config { get => config; set => config = value; }
        public Service.Abstract.Service Service { get => service; set => service = value; }
        public string Name { get => name; set => name = value; }


        #endregion

        #region --方法--
        public Request()
        {

        }
        internal static void Register(Request instance)
        {
            foreach (MethodInfo method in instance.GetType().GetMethods())
            {
                Attribute.RequestMapping attribute = method.GetCustomAttribute<Attribute.RequestMapping>();
                if (attribute != null)
                {
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
                }
            }
        }

        #endregion

        #region -- 生命周期 --

        internal protected abstract void Initialize();
        internal protected abstract void Register();
        internal protected abstract void UnRegister();
        internal protected abstract void UnInitialize();

        #endregion
    }
}
