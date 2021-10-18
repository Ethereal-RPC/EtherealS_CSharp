namespace EtherealS.Service.Extension.Authority
{
    /// <summary>
    /// 可权限化接口
    /// </summary>
    public interface IAuthoritable
    {
        /// <summary>
        /// 权限信息
        /// </summary>
        public object Authority { get; set; }
    }
}
