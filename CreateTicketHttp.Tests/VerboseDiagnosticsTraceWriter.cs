﻿using Microsoft.Azure.WebJobs.Host;
using System.Diagnostics;

namespace FunctionTestHelper
{
    [System.Obsolete]
    public class VerboseDiagnosticsTraceWriter : TraceWriter
    {

        public VerboseDiagnosticsTraceWriter() : base(TraceLevel.Verbose)
        {

        }
        public override void Trace(TraceEvent traceEvent)
        {
            Debug.WriteLine(traceEvent.Message);
        }
    }
}
