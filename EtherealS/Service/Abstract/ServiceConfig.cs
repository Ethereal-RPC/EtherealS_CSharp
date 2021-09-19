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

        #endregion
    }
}
