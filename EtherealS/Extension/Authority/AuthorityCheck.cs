using System.Reflection;
using EtherealS.Annotation;
using EtherealS.Model;
using EtherealS.RPCService;

namespace EtherealS.Extension.Authority
{
    public class AuthorityCheck
    {
        public static bool ServiceCheck(Service service, MethodInfo method, BaseUserToken token)
        {
            Annotation.RPCService annotation = method.GetCustomAttribute<Annotation.RPCService>();
            if (annotation.Authority != null)
            {
                if ((token as IAuthorityCheck).Check(annotation))
                {
                    return true;
                }
                else return false;
            }
            else if (service.Config.Authoritable)
            {
                if ((token as IAuthorityCheck).Check((IAuthoritable)service.Instance))
                {
                    return true;
                }
                else return false;
            }
            else return true;
        }
    }
}
