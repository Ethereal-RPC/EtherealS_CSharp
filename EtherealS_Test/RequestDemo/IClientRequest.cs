using EtherealS_Test.Model;
using EtherealS.Request.Attribute;
using EtherealS.Server.Attribute;

namespace EtherealS_Test.RequestDemo
{
    public interface IClientRequest
    {
        [Request(InvokeType = Request.InvokeTypeFlags.All)]
         public string Say([Token]User user,User sender, string message);
    }
}
