using EtherealS.Model;
using EtherealS.NativeServer.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtherealS.Core.Delegate
{
    public class Delegates
    {
        public delegate void OnExceptionDelegate(Exception exception);
        public delegate void OnLogDelegate(RPCLog log);
    }
}
