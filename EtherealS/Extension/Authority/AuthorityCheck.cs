using System.Reflection;
using EtherealS.Server.Abstract;
using EtherealS.Service.Attribute;

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
        public static bool ServiceCheck(Net.Abstract.Net net,Service.Abstract.Service service, MethodInfo method, BaseToken token)
        {
            Service.Attribute.Service annotation = method.GetCustomAttribute<Service.Attribute.Service>();
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
                if ((token as IAuthorityCheck).Check((IAuthoritable)service))
                {
                    return true;
                }
                else return false;
            }
        }
    }
}
