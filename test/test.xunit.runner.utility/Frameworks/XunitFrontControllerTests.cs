#if NETFRAMEWORK

using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

public class XunitFrontControllerTests
{
    public class DescriptorsAndBulkDeserialization
    {
        [Fact]
        public void RoundTrip()
        {
            string code = @"
using System;
using Xunit;

namespace Namespace1
{
    public class Class1
    {
        [Fact]
        public void FactMethod() { }

        [Theory]
        [InlineData(42)]
        public void TheoryMethod(int x) { }
    }
}";

            using (var assembly = CSharpAcceptanceTestV2Assembly.Create(code))
            {
                var serializations = default(List<string>);
                var testCollectionId = default(Guid);

                using (var serializationController = new TestableXunitFrontController(assembly.FileName))
                {
                    var sink = new SpyMessageSink<IDiscoveryCompleteMessage>();
                    serializationController.Find(includeSourceInformation: false, messageSink: sink, discoveryOptions: TestFrameworkOptions.ForDiscovery());
                    sink.Finished.WaitOne();

                    var testCases = sink.Messages.OfType<ITestCaseDiscoveryMessage>().OrderBy(tcdm => tcdm.TestCase.TestMethod.Method.Name).Select(tcdm => tcdm.TestCase).ToList();
                    testCollectionId = testCases[0].TestMethod.TestClass.TestCollection.UniqueID;
                    var descriptors = serializationController.GetTestCaseDescriptors(testCases, true);
                    serializations = descriptors.Select(d => d.Serialization).ToList();
                }

                Assert.Collection(serializations,
                    s => Assert.Equal($":F:Namespace1.Class1:FactMethod:1:0:{testCollectionId.ToString("N")}", s),
                    s => Assert.StartsWith("Xunit.Sdk.XunitTestCase, xunit.execution.{Platform}:", s)
                );

                using (var deserializationController = new TestableXunitFrontController(assembly.FileName))
                {
                    var deserializations = deserializationController.BulkDeserialize(serializations);

                    Assert.Collection(deserializations.Select(kvp => kvp.Value),
                        testCase => Assert.Equal("Namespace1.Class1.FactMethod", testCase.DisplayName),
                        testCase => Assert.Equal("Namespace1.Class1.TheoryMethod(x: 42)", testCase.DisplayName)
                    );
                }
            }
        }
    }

    class TestableXunitFrontController : XunitFrontController
    {
        public TestableXunitFrontController(string assemblyFileName, string configFileName = null, bool shadowCopy = true, AppDomainSupport appDomainSupport = AppDomainSupport.Required)
            : base(appDomainSupport, assemblyFileName, configFileName, shadowCopy)
        {
        }
    }
}

#endif
