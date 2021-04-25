using EtherealS.Model;

namespace EtherealS.RPCRequest
{
    public class RequestConfig
    {
        #region --字段--
        private RPCTypeConfig types;
        #endregion

        #region --属性--
        public RPCTypeConfig Types { get => types; set => types = value; }
        #endregion

        #region --方法--
        public RequestConfig(RPCTypeConfig type)
        {
            this.types = type;
        }
        #endregion
    }
}
