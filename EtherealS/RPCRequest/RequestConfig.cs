using EtherealS.Model;

namespace EtherealS.RPCRequest
{
    public class RequestConfig
    {
        #region --字段--
        private bool tokenEnable;
        private RPCTypeConfig types;
        #endregion

        #region --属性--
        public bool TokenEnable { get => tokenEnable; set => tokenEnable = value; }
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
