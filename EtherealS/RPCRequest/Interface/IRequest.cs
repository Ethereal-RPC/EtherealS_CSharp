using EtherealS.Core.Interface;
using EtherealS.Core.Model;
using EtherealS.NativeServer;
using EtherealS.RPCNet;
using Newtonsoft.Json;
using System;
using System.Reflection;
using System.Text;

namespace EtherealS.RPCRequest
{
    public interface IRequest : ILogEvent, IExceptionEvent
    {

    }
}
