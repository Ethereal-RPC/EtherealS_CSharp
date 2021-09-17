using System.Reflection;
using EtherealS.Core.Model;
using EtherealS.Server.Abstract;

namespace EtherealS.Service.Abstract
{
    /// <summary>
    /// 服务配置项
    /// </summary>
    public class ServiceConfig
    {
        #region --委托--
        public delegate bool InterceptorDelegate(Service service, MethodInfo method, Token token);
        #endregion

        #region --事件属性--
        /// <summary>
        /// 拦截器事件
        /// </summary>
        public event InterceptorDelegate InterceptorEvent;
        #endregion

        #region --字段--
        /// <summary>
        /// 中间层抽象数据类配置项
        /// </summary>
        private AbstractTypes types;
        #endregion

        #region --属性--
        public AbstractTypes Types { get => types; set => types = value; }
        #endregion

        #region --方法--
        public ServiceConfig(AbstractTypes type)
        {
            this.types = type;
        }
        internal bool OnInterceptor(Service service, MethodInfo method, Token token)
        {
            if (InterceptorEvent != null)
            {
                foreach (InterceptorDelegate item in InterceptorEvent.GetInvocationList())
                {
                    if (!item.Invoke(service, method, token)) return false;
                }
                return true;
            }
            else return true;
        }
        #endregion
    }
}
