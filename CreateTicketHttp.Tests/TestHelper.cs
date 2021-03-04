using Microsoft.Azure.WebJobs.Host;

namespace TestHelpers
{
    public abstract class FunctionTest
    {

        protected TraceWriter log = new FunctionTestHelper.VerboseDiagnosticsTraceWriter();

    }
}
