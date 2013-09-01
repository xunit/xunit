using System.Diagnostics;
using System.Reflection;
using Xunit;

namespace TestUtility
{
    public class TestTimingAttribute : BeforeAfterTestAttribute
    {
        Stopwatch sw = new Stopwatch();

        public override void Before(MethodInfo methodUnderTest)
        {
            sw.Start();
        }

        public override void After(MethodInfo methodUnderTest)
        {
            sw.Stop();
            Trace.WriteLine(string.Format("Time spent in execution: {0}ms", sw.ElapsedMilliseconds));
        }
    }
}
