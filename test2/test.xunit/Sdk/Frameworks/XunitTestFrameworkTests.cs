using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;
using ITypeInfo = Xunit.Abstractions.ITypeInfo;

public class XunitTestFrameworkTests
{
    public class FindByAssembly
    {
        [Fact]
        public void GuardClause()
        {
            var framework = new XunitTestFramework();

            // REVIEW: Should we add Assert.ThrowsArgumentNull and friends?
            var ex = Record.Exception(() => framework.Find(assembly: null));

            var aex = Assert.IsType<ArgumentNullException>(ex);
            Assert.Equal("assembly", aex.ParamName);
        }

        [Fact]
        public void AssemblyWithNoTypes_ReturnsNoTestCases()
        {
            var framework = new XunitTestFramework();
            var mockAssembly = new MockAssemblyInfo();

            IEnumerable<ITestCase> results = framework.Find(mockAssembly.Object);

            Assert.Empty(results);
        }

        [Fact]
        public void RequestsOnlyPublicTypesFromAssembly()
        {
            var framework = new XunitTestFramework();
            var mockAssembly = new MockAssemblyInfo();

            framework.Find(mockAssembly.Object);

            mockAssembly.Verify(a => a.GetTypes(/*includePrivateTypes*/ false), Times.Once());
        }

        [Fact]
        public void CallsFindImplWhenTypesAreFoundInAssembly()
        {
            var mockFramework = new Mock<TestableXunitTestFramework> { CallBase = true };
            var objectTypeInfo = Reflector2.Wrap(typeof(object));
            var intTypeInfo = Reflector2.Wrap(typeof(int));
            var mockAssembly = new MockAssemblyInfo(types: new[] { objectTypeInfo, intTypeInfo });

            mockFramework.Object.Find(mockAssembly.Object).ToList();

            mockFramework.Verify(f => f.FindImpl(objectTypeInfo), Times.Once());
            mockFramework.Verify(f => f.FindImpl(intTypeInfo), Times.Once());
        }
    }

    public class FindByType
    {
        [Fact]
        public void GuardClause()
        {
            var framework = new XunitTestFramework();

            // REVIEW: Should we add Assert.ThrowsArgumentNull and friends?
            var ex = Record.Exception(() => framework.Find(type: null));

            var aex = Assert.IsType<ArgumentNullException>(ex);
            Assert.Equal("type", aex.ParamName);
        }

        [Fact]
        public void RequestsPublicAndPrivateMethodsFromType()
        {
            var framework = new XunitTestFramework();
            var type = new Mock<ITypeInfo>();

            framework.Find(type.Object).ToList();

            type.Verify(t => t.GetMethods(/*includePrivateMethods*/ true), Times.Once());
        }

        [Fact]
        public void CallsFindImplWhenMethodsAreFoundOnType()
        {
            var mockFramework = new Mock<TestableXunitTestFramework> { CallBase = true };
            var objectTypeInfo = Reflector2.Wrap(typeof(object));

            mockFramework.Object.Find(objectTypeInfo).ToList();

            mockFramework.Verify(f => f.FindImpl(objectTypeInfo), Times.Once());
        }
    }

    public class FindImpl
    {
        class ClassWithNoTests
        {
            public void NonTestMethod() { }
        }

        [Fact]
        public void ClassWithNoTests_ReturnsNoTestCases()
        {
            var framework = new TestableXunitTestFramework();
            var type = Reflector2.Wrap(typeof(ClassWithNoTests));

            IEnumerable<ITestCase> results = framework.FindImpl(type);

            Assert.Empty(results);
        }

        class ClassWithOneFact
        {
            [Fact2]
            public void TestMethod() { }
        }

        [Fact]
        public void AssemblyWithFact_ReturnsOneTestCaseOfTypeXunitTestCase()
        {
            var framework = new TestableXunitTestFramework();
            var type = Reflector2.Wrap(typeof(ClassWithOneFact));

            IEnumerable<ITestCase> results = framework.FindImpl(type);

            ITestCase testCase = Assert.Single(results);
            Assert.IsType<XunitTestCase>(testCase);
        }

        class ClassWithMixOfFactsAndNonFacts
        {
            [Fact2]
            public void TestMethod1() { }

            [Fact2]
            public void TestMethod2() { }

            public void NonTestMethod() { }
        }

        [Fact]
        public void AssemblyWithMixOfFactsAndNonTests_ReturnsTestCasesOnlyForFacts()
        {
            var framework = new TestableXunitTestFramework();
            var type = Reflector2.Wrap(typeof(ClassWithMixOfFactsAndNonFacts));

            IEnumerable<IMethodTestCase> results = framework.FindImpl(type).Cast<IMethodTestCase>();

            Assert.Equal(2, results.Count());
            Assert.Single(results, t => t.DisplayName == "XunitTestFrameworkTests+FindImpl+ClassWithMixOfFactsAndNonFacts.TestMethod1");
            Assert.Single(results, t => t.DisplayName == "XunitTestFrameworkTests+FindImpl+ClassWithMixOfFactsAndNonFacts.TestMethod2");
        }

        class TheoryWithInlineData
        {
            [Theory]
            [InlineData("Hello world")]
            [InlineData(42)]
            public void TheoryMethod(object value) { }
        }

        [Fact]
        public void AssemblyWithTheoryWithInlineData_ReturnsOneTestCasePerDataRecord()
        {
            var framework = new TestableXunitTestFramework();
            var type = Reflector2.Wrap(typeof(TheoryWithInlineData));

            IEnumerable<IMethodTestCase> results = framework.FindImpl(type).Cast<IMethodTestCase>();

            Assert.Equal(2, results.Count());
            Assert.Single(results, t => t.DisplayName == "XunitTestFrameworkTests+FindImpl+TheoryWithInlineData.TheoryMethod(value: \"Hello world\")");
            Assert.Single(results, t => t.DisplayName == "XunitTestFrameworkTests+FindImpl+TheoryWithInlineData.TheoryMethod(value: 42)");
        }
    }

    public class TestableXunitTestFramework : XunitTestFramework
    {
        public virtual IEnumerable<ITestCase> FindImpl(ITypeInfo type)
        {
            return base.FindImpl(new MockAssemblyInfo(types: new[] { type }).Object, type);
        }

        protected override IEnumerable<ITestCase> FindImpl(IAssemblyInfo assembly, ITypeInfo type)
        {
            return FindImpl(type);
        }
    }
}
