using System.Reflection;
using Xunit;
using Xunit.Sdk;

namespace Xunit1
{
    public class SkipCommandTests
    {
        [Fact]
        public void SkipReturnSkipResult()
        {
            MethodInfo method = typeof(SpyStub).GetMethod("Skip");
            SkipCommand command = new SkipCommand(Reflector.Wrap(method), null, "reason");

            MethodResult result = command.Execute(new SpyStub());

            SkipResult skipResult = Assert.IsType<SkipResult>(result);
            Assert.Equal("reason", skipResult.Reason);
        }

        internal class SpyStub : FixtureSpy
        {
            public void Skip() { }
        }
    }
}
