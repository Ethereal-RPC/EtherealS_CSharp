using EtherealS_Test.Model;
using EtherealS.Request.Attribute;

namespace EtherealS_Test.RequestDemo
{
    public interface IClientRequest
    {
        [Request]
         public string Say(User user,User sender, string message);
    }
}
