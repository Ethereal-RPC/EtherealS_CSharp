namespace EtherealS.Extension.Authority
{
    /// <summary>
    /// 权限检查接口,实现此接口默认视为实现了可权限化接口
    /// </summary>
    public interface IAuthorityCheck:IAuthoritable
    {
        /// <summary>
        /// 权限接口检查函数
        /// </summary>
        /// <param name="authoritable"></param>
        /// <returns></returns>
        public bool Check(IAuthoritable authoritable);
    }
}
