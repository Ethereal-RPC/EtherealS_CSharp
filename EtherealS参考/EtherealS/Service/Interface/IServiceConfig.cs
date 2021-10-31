namespace EtherealS.Service.Interface
{
    /// <summary>
    /// 服务配置项
    /// </summary>
    public interface IServiceConfig
    {
        /// <summary>
        /// 读Config配置项
        /// </summary>
        /// <returns>成功返回true，失败返回false</returns>
        public bool Load();
        /// <summary>
        /// 写Config配置项
        /// </summary>
        /// <returns>成功返回true，失败返回false</returns>
        public bool Save();
    }
}
