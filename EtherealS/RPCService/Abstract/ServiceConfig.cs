using EtherealS.Core.Model;
using EtherealS.NativeServer;
using System.Reflection;

namespace EtherealS.RPCService
{
    /// <summary>
    /// 服务配置项
    /// </summary>
    public class ServiceConfig
    {
        #region --委托--
        public delegate bool InterceptorDelegate(Service service, MethodInfo method, NativeServer.Token token);
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
        private RPCTypeConfig types;
        #endregion

        #region --属性--
        public RPCTypeConfig Types { get => types; set => types = value; }
        #endregion

        #region --方法--
        public ServiceConfig(RPCTypeConfig type)
        {
            this.types = type;
        }
        internal bool OnInterceptor(Service service, MethodInfo method, NativeServer.Token token)
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
