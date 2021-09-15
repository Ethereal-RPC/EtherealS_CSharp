using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using EtherealS.Core.Interface;
using EtherealS.Extension.Authority;
using EtherealS.Model;
using EtherealS.NativeServer;
using static EtherealS.Core.Delegate.Delegates;

namespace EtherealS.RPCService
{
    public interface IService : ILogEvent, IExceptionEvent
    {

    }
}
