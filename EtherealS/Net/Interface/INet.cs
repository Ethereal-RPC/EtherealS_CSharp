using EtherealS.Core.Interface;

namespace EtherealS.Net.Interface
{
    public interface INet
    {
        #region --方法--
        /// <summary>
        /// 部署节点
        /// </summary>
        /// <returns></returns>
        public bool Publish();

        #endregion
    }
}
