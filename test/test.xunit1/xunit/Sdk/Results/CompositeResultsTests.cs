using System.Xml;
using Xunit;
using Xunit.Sdk;

namespace Xunit1
{
    public class CompositeResultsTests
    {
        [Fact]
        public void AddResult()
        {
            StubResult stubResult = new StubResult();

            StubCompositeResult compositeResult = new StubCompositeResult();
            compositeResult.Add(stubResult);

            Assert.Single(compositeResult.Results);
            Assert.True(compositeResult.Results.Contains(stubResult));
        }

        [Fact]
        public void AddResultsMultiple()
        {
            StubResult stubResult1 = new StubResult();
            StubResult stubResult2 = new StubResult();
            StubResult stubResult3 = new StubResult();

            StubCompositeResult compositeResult = new StubCompositeResult();
            compositeResult.Add(stubResult1);
            compositeResult.Add(stubResult2);
            compositeResult.Add(stubResult3);

            Assert.Equal(3, compositeResult.Results.Count);
            Assert.True(compositeResult.Results.Contains(stubResult1));
            Assert.True(compositeResult.Results.Contains(stubResult2));
            Assert.True(compositeResult.Results.Contains(stubResult3));
        }

        [Fact]
        public void ResultsShouldBeReadOnly()
        {
            StubCompositeResult result = new StubCompositeResult();

            Assert.True(result.Results.IsReadOnly);
        }

        class StubCompositeResult : CompositeResult
        {
            public override XmlNode ToXml(XmlNode parentNode)
            {
                throw new System.NotImplementedException();
            }
        }

        class StubResult : ITestResult
        {
            public XmlNode ToXml(XmlNode parentNode)
            {
                throw new System.NotImplementedException();
            }

            public double ExecutionTime
            {
                get { throw new System.NotImplementedException(); }
            }
        }
    }
}
