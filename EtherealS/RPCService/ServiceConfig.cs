using System.Reflection;
using EtherealS.Model;

namespace EtherealS.RPCService
{
    public class ServiceConfig
    {
        #region --委托--
        public delegate bool InterceptorDelegate(Service service, MethodInfo method, BaseUserToken token);
        #endregion

        #region --事件--
        public event InterceptorDelegate InterceptorEvent;
        #endregion

        #region --字段--
        private RPCTypeConfig types;
        private bool authoritable = false;
        #endregion

        #region --属性--
        public RPCTypeConfig Types { get => types; set => types = value; }
        public bool Authoritable { get => authoritable; set => authoritable = value; }
        #endregion

        #region --方法--
        public ServiceConfig(RPCTypeConfig type)
        {
            this.types = type;
        }

        public ServiceConfig(RPCTypeConfig type, bool tokenEnable)
        {
            this.types = type;
        }
        public bool OnInterceptor(Service service, MethodInfo method, BaseUserToken token)
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
