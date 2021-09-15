using EtherealS.Core.Interface;
using EtherealS.Model;
using EtherealS.RPCNet;
using System;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Threading;

namespace EtherealS.NativeServer.Interface
{
    public interface IBaseToken:ILogEvent,IExceptionEvent
    {
        public void DisConnect(string reason);


    }
}
