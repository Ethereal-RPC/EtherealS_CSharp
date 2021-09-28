using EtherealS_Test.Model;
using EtherealS.Request.Attribute;

namespace EtherealS_Test.RequestDemo
{
    public interface IClientRequest
    {
        [Request(InvokeType = Request.InvokeTypeFlags.All)]
         public string Say(User user,User sender, string message);
    }
}
