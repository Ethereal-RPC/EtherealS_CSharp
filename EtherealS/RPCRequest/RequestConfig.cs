using EtherealS.Model;

namespace EtherealS.RPCRequest
{
    public class RequestConfig
    {
        #region --字段--
        private bool tokenEnable;
        private RPCType type;
        #endregion

        #region --属性--
        public bool TokenEnable { get => tokenEnable; set => tokenEnable = value; }
        public RPCType Type { get => type; set => type = value; }
        #endregion

        #region --方法--
        public RequestConfig(RPCType type)
        {
            this.type = type;
        }
        #endregion
    }
}
