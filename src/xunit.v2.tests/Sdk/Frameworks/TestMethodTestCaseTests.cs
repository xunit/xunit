using System;
using NSubstitute;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;
using Xunit.Serialization;
using TestMethodDisplay = Xunit.Sdk.TestMethodDisplay;
using TestMethodDisplayOptions = Xunit.Sdk.TestMethodDisplayOptions;

public class TestMethodTestCaseTests
{
    [Fact]
    public static void DefaultBehavior()
    {
        var testMethod = Mocks.TestMethod("MockType", "MockMethod");

        var testCase = new TestableTestMethodTestCase(testMethod);

        Assert.Equal("MockType.MockMethod", testCase.DisplayName);
        Assert.Null(testCase.SkipReason);
        Assert.Empty(testCase.Traits);
    }

    [Fact]
    public static void DisposesArguments()
    {
        var disposable1 = Substitute.For<IDisposable>();
        var disposable2 = Substitute.For<IDisposable>();
        var testMethod = Mocks.TestMethod();
        var testCase = new TestableTestMethodTestCase(testMethod, new[] { disposable1, disposable2 });

        testCase.Dispose();

        disposable1.Received(1).Dispose();
        disposable2.Received(1).Dispose();
    }

    public class DisplayName
    {
        [Fact]
        public static void CorrectNumberOfTestArguments()
        {
            var param1 = Mocks.ParameterInfo("p1");
            var param2 = Mocks.ParameterInfo("p2");
            var param3 = Mocks.ParameterInfo("p3");
            var testMethod = Mocks.TestMethod(parameters: new[] { param1, param2, param3 });
            var arguments = new object[] { 42, "Hello, world!", 'A' };

            var testCase = new TestableTestMethodTestCase(testMethod, arguments);

            Assert.Equal($"{testMethod.TestClass.Class.Name}.{testMethod.Method.Name}(p1: 42, p2: \"Hello, world!\", p3: 'A')", testCase.DisplayName);
        }

        [Fact]
        public static void NotEnoughTestArguments()
        {
            var param = Mocks.ParameterInfo("p1");
            var testMethod = Mocks.TestMethod(parameters: new[] { param });

            var testCase = new TestableTestMethodTestCase(testMethod, new object[0]);

            Assert.Equal($"{testMethod.TestClass.Class.Name}.{testMethod.Method.Name}(p1: ???)", testCase.DisplayName);
        }

        [CulturedFact]
        public static void TooManyTestArguments()
        {
            var param = Mocks.ParameterInfo("p1");
            var testMethod = Mocks.TestMethod(parameters: new[] { param });
            var arguments = new object[] { 42, 21.12M };

            var testCase = new TestableTestMethodTestCase(testMethod, arguments);

            Assert.Equal($"{testMethod.TestClass.Class.Name}.{testMethod.Method.Name}(p1: 42, ???: {21.12})", testCase.DisplayName);
        }

        [Theory]
        [InlineData(TestMethodDisplay.ClassAndMethod, "TestMethodTestCaseTests+DisplayName.OverrideDefaultMethodDisplay")]
        [InlineData(TestMethodDisplay.Method, "OverrideDefaultMethodDisplay")]
        public static void OverrideDefaultMethodDisplay(TestMethodDisplay methodDisplay, string expectedDisplayName)
        {
            var testMethod = Mocks.TestMethod(typeof(DisplayName), "OverrideDefaultMethodDisplay");

            var testCase = new TestableTestMethodTestCase(testMethod, defaultMethodDisplay: methodDisplay);

            Assert.Equal(expectedDisplayName, testCase.DisplayName);
        }
    }

    public class Serialization
    {
        [Fact]
        public static void CanRoundTrip_PublicClass_PublicTestMethod()
        {
            var testCase = TestableTestMethodTestCase.Create<Serialization>("CanRoundTrip_PublicClass_PublicTestMethod");

            var serialized = XunitSerializationInfo.Serialize(testCase);
            var deserialized = XunitSerializationInfo.Deserialize(typeof(TestableTestMethodTestCase), serialized);

            Assert.NotNull(deserialized);
        }

        [Fact]
        public static void CanRoundTrip_PublicClass_PrivateTestMethod()
        {
            var testCase = TestableTestMethodTestCase.Create<Serialization>("CanRoundTrip_PublicClass_PrivateTestMethod");

            var serialized = XunitSerializationInfo.Serialize(testCase);
            var deserialized = XunitSerializationInfo.Deserialize(typeof(TestableTestMethodTestCase), serialized);

            Assert.NotNull(deserialized);
        }

        [Fact]
        public static void CanRoundTrip_PrivateClass()
        {
            var testCase = TestableTestMethodTestCase.Create<PrivateClass>("TestMethod");

            var serialized = XunitSerializationInfo.Serialize(testCase);
            var deserialized = XunitSerializationInfo.Deserialize(typeof(TestableTestMethodTestCase), serialized);

            Assert.NotNull(deserialized);
        }

        class PrivateClass
        {
            [Fact]
            public static void TestMethod()
            {
                Assert.True(false);
            }
        }
    }

    public class UniqueID
    {
        [Fact]
        public static void UniqueID_NoArguments()
        {
            var value = TestableTestMethodTestCase.Create<ClassUnderTest>("TestMethod").UniqueID;

            Assert.NotEmpty(value);
        }

        [Fact]
        public static void UniqueID_Arguments()
        {
            var value42 = TestableTestMethodTestCase.Create<ClassUnderTest>("TestMethod", new object[] { 42 }).UniqueID;
            var valueHelloWorld = TestableTestMethodTestCase.Create<ClassUnderTest>("TestMethod", new object[] { "Hello, world!" }).UniqueID;
            var valueNull = TestableTestMethodTestCase.Create<ClassUnderTest>("TestMethod", new object[] { (string)null }).UniqueID;

            Assert.NotEmpty(value42);
            Assert.NotEqual(value42, valueHelloWorld);
            Assert.NotEqual(value42, valueNull);
        }

        class ClassUnderTest
        {
            [Fact]
            public static void TestMethod() { }
        }
    }

    class TestableTestMethodTestCase : TestMethodTestCase
    {
        public TestableTestMethodTestCase() { }

        public TestableTestMethodTestCase(ITestMethod testMethod,
                                          object[] testMethodArguments = null,
                                          TestMethodDisplay defaultMethodDisplay = TestMethodDisplay.ClassAndMethod,
                                          TestMethodDisplayOptions defaultMethodDisplayOptions = TestMethodDisplayOptions.None)
            : base(defaultMethodDisplay, defaultMethodDisplayOptions, testMethod, testMethodArguments) { }

        public static TestableTestMethodTestCase Create<TClass>(string methodName, object[] testMethodArguments = null)
        {
            var testMethod = Mocks.TestMethod(typeof(TClass), methodName);
            return new TestableTestMethodTestCase(testMethod, testMethodArguments);
        }
    }
}
