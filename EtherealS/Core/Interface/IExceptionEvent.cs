using EtherealS.Core.Model;
using System;

namespace EtherealS.Core.Interface
{
    public interface IExceptionEvent
    {
        public void OnException(RPCException.ErrorCode code, string message);
        public void OnException(RPCException e);
    }
}
