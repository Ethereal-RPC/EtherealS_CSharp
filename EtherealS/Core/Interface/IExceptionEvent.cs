using EtherealS.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtherealS.Core.Interface
{
    public interface IExceptionEvent
    {
        public void OnException(RPCException.ErrorCode code, string message);
        public void OnException(Exception e);
    }
}
