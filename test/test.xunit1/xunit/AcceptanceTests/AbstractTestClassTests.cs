using System.Xml;
using TestUtility;
using Xunit;

namespace Xunit1
{
    public class AbstractTestClassTests : AcceptanceTestInNewAppDomain
    {
        [Fact]
        public void TestsFromAbstractBaseClassesShouldBeExecuted()
        {
            string code = @"
                using Xunit;

                public abstract class TestBase
                {
                    [Fact]
                    public void BaseTestMethod() {}
                }

                public class DerivedTestClass1 : TestBase
                {
                    [Fact]
                    public void TestMethod1() {}
                }

                public class DerivedTestClass2 : TestBase
                {
                    [Fact]
                    public void TestMethod2() {}
                }";

            XmlNode assemblyNode = Execute(code);

            XmlNode result00 = ResultXmlUtility.GetResult(assemblyNode, 0, 0);
            ResultXmlUtility.AssertAttribute(result00, "result", "Pass");
            XmlNode result01 = ResultXmlUtility.GetResult(assemblyNode, 0, 1);
            ResultXmlUtility.AssertAttribute(result01, "result", "Pass");
            XmlNode result10 = ResultXmlUtility.GetResult(assemblyNode, 1, 0);
            ResultXmlUtility.AssertAttribute(result10, "result", "Pass");
            XmlNode result11 = ResultXmlUtility.GetResult(assemblyNode, 1, 1);
            ResultXmlUtility.AssertAttribute(result11, "result", "Pass");
        }
    }
}
