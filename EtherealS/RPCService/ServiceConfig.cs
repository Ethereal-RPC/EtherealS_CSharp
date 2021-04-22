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
        private RPCType type;
        private bool authoritable = false;
        #endregion

        #region --属性--
        public RPCType Type { get => type; set => type = value; }
        public bool Authoritable { get => authoritable; set => authoritable = value; }
        #endregion

        #region --方法--
        public ServiceConfig(RPCType type)
        {
            this.type = type;
        }

        public ServiceConfig(RPCType type, bool tokenEnable)
        {
            this.type = type;
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
