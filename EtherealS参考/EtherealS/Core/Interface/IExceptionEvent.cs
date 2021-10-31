using EtherealS.Core.Model;
using System;

namespace EtherealS.Core.Interface
{
    public interface IExceptionEvent
    {
        public void OnException(TrackException.ErrorCode code, string message);
        public void OnException(TrackException e);
    }
}
