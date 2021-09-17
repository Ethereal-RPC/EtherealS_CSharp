using EtherealS.Core.Model;
using EtherealS.Request.Interface;

namespace EtherealS.Request.Abstract
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
        private AbstractTypes types;


        #endregion

        #region --属性--
        public AbstractTypes Types { get => types; set => types = value; }


        #endregion

        #region --方法--
        public RequestConfig(AbstractTypes type)
        {
            this.types = type;
        }
        #endregion
    }
}
