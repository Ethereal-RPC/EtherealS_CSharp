using EtherealS.Core.Model;
using System;

namespace EtherealS.RPCRequest
{
    /// <summary>
    /// 服务请求配置项
    /// </summary>
    public class RequestConfig:IRequestConfig
    {
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
        public RequestConfig(RPCTypeConfig type)
        {
            this.types = type;
        }
        #endregion
    }
}
