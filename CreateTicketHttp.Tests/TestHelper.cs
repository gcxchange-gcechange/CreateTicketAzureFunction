using Microsoft.Azure.WebJobs.Host;

namespace TestHelpers
{
    public abstract class FunctionTest
    {
        [System.Obsolete]
        protected TraceWriter log = new FunctionTestHelper.VerboseDiagnosticsTraceWriter();
    }
}
