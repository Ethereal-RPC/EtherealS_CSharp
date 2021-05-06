using System.Reflection;
using EtherealS.Model;
using EtherealS.RPCService;

namespace EtherealS.Extension.Authority
{
    /// <summary>
    /// 权限检查默认实现类
    /// </summary>
    public class AuthorityCheck
    {
        /// <summary>
        /// 权限检查默认实现函数
        /// </summary>
        /// <param name="service">服务信息</param>  
        /// <param name="method">方法信息</param>
        /// <param name="token">Token信息</param>
        /// <returns></returns>
        public static bool ServiceCheck(Service service, MethodInfo method, BaseUserToken token)
        {
            Attribute.RPCService annotation = method.GetCustomAttribute<Attribute.RPCService>();
            if (annotation.Authority != null)
            {
                if ((token as IAuthorityCheck).Check(annotation))
                {
                    return true;
                }
                else return false;
            }
            else
            {
                if ((token as IAuthorityCheck).Check((IAuthoritable)service.Instance))
                {
                    return true;
                }
                else return false;
            }
        }
    }
}
