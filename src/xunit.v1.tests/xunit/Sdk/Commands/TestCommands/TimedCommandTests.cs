using System.Reflection;
using System.Threading;
using System.Xml;
using Xunit;
using Xunit.Sdk;

namespace Xunit1
{
    public class TimedCommandTests
    {
        [Fact]
        public void MeasuresTime()
        {
            TimedCommand command = new TimedCommand(new DummyCommand());

            MethodResult result = command.Execute(null);

            Assert.InRange(result.ExecutionTime, 0.01, 100.0);
        }

        class DummyCommand : ITestCommand
        {
            public string DisplayName
            {
                get { return null; }
            }

            public bool ShouldCreateInstance
            {
                get { return true; }
            }

            public int Timeout
            {
                get { return 0; }
            }

            public MethodResult Execute(object testClass)
            {
                Thread.Sleep(50);
                MethodInfo method = GetType().GetMethod("Execute");
                return new PassedResult(Reflector.Wrap(method), null);
            }

            public XmlNode ToStartXml()
            {
                return null;
            }
        }
    }
}
