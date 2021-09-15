using EtherealS.Core.Interface;
using EtherealS.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtherealS.NativeServer.Interface
{
    interface IServer : ILogEvent, IExceptionEvent
    {
        public void Start();
        public void Close();
    }
}
