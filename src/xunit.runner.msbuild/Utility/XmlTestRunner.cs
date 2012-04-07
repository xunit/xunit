
namespace Xunit.Runner.MSBuild
{
    public class XmlTestRunner
    {
        TestRunner testRunner;

        public XmlTestRunner(IExecutorWrapper executorWrapper, IRunnerLogger logger)
        {
            testRunner = new TestRunner(executorWrapper, logger);
        }

        public string Xml { get; private set; }

        public TestRunnerResult RunAssembly()
        {
            InMemoryTransform transform = new InMemoryTransform();
            TestRunnerResult result = testRunner.RunAssembly(new[] { transform });
            Xml = transform.Xml;
            return result;
        }

        class InMemoryTransform : IResultXmlTransform
        {
            public string Xml { get; private set; }

            public string OutputFilename
            {
                get { return null; }
            }

            public void Transform(string xml)
            {
                Xml = xml;
            }
        }
    }
}
