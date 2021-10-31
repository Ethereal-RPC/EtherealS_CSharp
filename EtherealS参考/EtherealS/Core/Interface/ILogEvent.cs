﻿using EtherealS.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtherealS.Core.Interface
{
    public interface ILogEvent
    {
        public void OnLog(TrackLog.LogCode code, string message);
        public void OnLog(TrackLog log);
    }
}
