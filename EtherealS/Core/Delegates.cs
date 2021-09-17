using EtherealS.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtherealS.Core.Delegates
{
    public delegate void OnExceptionDelegate(RPCException exception);
    public delegate void OnLogDelegate(RPCLog log);
}
 