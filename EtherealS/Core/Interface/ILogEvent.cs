using EtherealS.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtherealS.Core.Interface
{
    public interface ILogEvent
    {
        public void OnLog(RPCLog.LogCode code, string message);
        public void OnLog(RPCLog log);
    }
}
