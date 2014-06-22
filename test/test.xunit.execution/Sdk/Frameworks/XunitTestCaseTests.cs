using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using NSubstitute;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;
using IAttributeInfo = Xunit.Abstractions.IAttributeInfo;

public class XunitTestCaseTests
{
    [Fact]
    public static void DefaultBehavior()
    {
        var testMethod = Mocks.TestMethod("MockType", "MockMethod");

        var testCase = new XunitTestCase(testMethod);

        Assert.Equal("MockType.MockMethod", testCase.DisplayName);
        Assert.Null(testCase.SkipReason);
        Assert.Empty(testCase.Traits);
    }

    [Fact]
    public static void SkipReason()
    {
        var testMethod = Mocks.TestMethod("MockType", "MockMethod", skip: "Skip Reason");

        var testCase = new XunitTestCase(testMethod);

        Assert.Equal("Skip Reason", testCase.SkipReason);
    }

    [Fact]
    public static void DisposesArguments()
    {
        var disposable1 = Substitute.For<IDisposable>();
        var disposable2 = Substitute.For<IDisposable>();
        var testMethod = Mocks.TestMethod();
        var testCase = new XunitTestCase(testMethod, new[] { disposable1, disposable2 });

        testCase.Dispose();

        disposable1.Received(1).Dispose();
        disposable2.Received(1).Dispose();
    }

    public class Traits : AcceptanceTest
    {
        [Fact]
        public static void TraitsOnTestMethod()
        {
            var trait1 = Mocks.TraitAttribute("Trait1", "Value1");
            var trait2 = Mocks.TraitAttribute("Trait2", "Value2");
            var testMethod = Mocks.TestMethod(methodAttributes: new[] { trait1, trait2 });

            var testCase = new XunitTestCase(testMethod);

            Assert.Equal("Value1", Assert.Single(testCase.Traits["Trait1"]));
            Assert.Equal("Value2", Assert.Single(testCase.Traits["Trait2"]));
        }

        [Fact]
        public static void TraitsOnTestClass()
        {
            var trait1 = Mocks.TraitAttribute("Trait1", "Value1");
            var trait2 = Mocks.TraitAttribute("Trait2", "Value2");
            var testMethod = Mocks.TestMethod(classAttributes: new[] { trait1, trait2 });

            var testCase = new XunitTestCase(testMethod);

            Assert.Equal("Value1", Assert.Single(testCase.Traits["Trait1"]));
            Assert.Equal("Value2", Assert.Single(testCase.Traits["Trait2"]));
        }

        [Fact]
        public void CustomTrait()
        {
            var passingTests = Run<ITestPassed>(typeof(ClassWithCustomTraitTest));

            Assert.Collection(passingTests,
                passingTest => Assert.Collection(passingTest.TestCase.Traits.OrderBy(x => x.Key),
                    namedTrait =>
                    {
                        Assert.Equal("Author", namedTrait.Key);
                        Assert.Collection(namedTrait.Value, value => Assert.Equal("Some Schmoe", value));
                    },
                    namedTrait =>
                    {
                        Assert.Equal("Bug", namedTrait.Key);
                        Assert.Collection(namedTrait.Value, value => Assert.Equal("2112", value));
                    }
                )
            );
        }

        class ClassWithCustomTraitTest
        {
            [Fact]
            [Bug(2112)]
            [Trait("Author", "Some Schmoe")]
            public static void BugFix() { }
        }

        public class BugDiscoverer : ITraitDiscoverer
        {
            public IEnumerable<KeyValuePair<string, string>> GetTraits(IAttributeInfo traitAttribute)
            {
                var ctorArgs = traitAttribute.GetConstructorArguments().ToList();
                yield return new KeyValuePair<string, string>("Bug", ctorArgs[0].ToString());
            }
        }

        [TraitDiscoverer("XunitTestCaseTests+Traits+BugDiscoverer", "test.xunit.execution")]
        class BugAttribute : Attribute, ITraitAttribute
        {
            public BugAttribute(int id) { }
        }
    }

    public class DisplayName
    {
        [Fact]
        public static void CustomDisplayName()
        {
            var testMethod = Mocks.TestMethod(displayName: "Custom Display Name");

            var testCase = new XunitTestCase(testMethod);

            Assert.Equal("Custom Display Name", testCase.DisplayName);
        }

        [Fact]
        public static void CorrectNumberOfTestArguments()
        {
            var param1 = Mocks.ParameterInfo("p1");
            var param2 = Mocks.ParameterInfo("p2");
            var param3 = Mocks.ParameterInfo("p3");
            var testMethod = Mocks.TestMethod(parameters: new[] { param1, param2, param3 });
            var arguments = new object[] { 42, "Hello, world!", 'A' };

            var testCase = new XunitTestCase(testMethod, arguments);

            Assert.Equal("MockType.MockMethod(p1: 42, p2: \"Hello, world!\", p3: 'A')", testCase.DisplayName);
        }

        [Fact]
        public static void NotEnoughTestArguments()
        {
            var param = Mocks.ParameterInfo("p1");
            var testMethod = Mocks.TestMethod(parameters: new[] { param });

            var testCase = new XunitTestCase(testMethod, new object[0]);

            Assert.Equal("MockType.MockMethod(p1: ???)", testCase.DisplayName);
        }

        [CulturedFact]
        public static void TooManyTestArguments()
        {
            var param = Mocks.ParameterInfo("p1");
            var testMethod = Mocks.TestMethod(parameters: new[] { param });
            var arguments = new object[] { 42, 21.12M };

            var testCase = new XunitTestCase(testMethod, arguments);

            Assert.Equal(String.Format("MockType.MockMethod(p1: 42, ???: {0})", 21.12), testCase.DisplayName);
        }
    }

    public class Serialization
    {
        [Fact]
        public static void CanRoundTrip_PublicClass_PublicTestMethod()
        {
            var serializer = new BinaryFormatter();
            var testCase = Mocks.XunitTestCase<Serialization>("CanRoundTrip_PublicClass_PublicTestMethod");
            var memoryStream = new MemoryStream();

            serializer.Serialize(memoryStream, testCase);
            memoryStream.Position = 0;

            Assert.DoesNotThrow(() => serializer.Deserialize(memoryStream));
        }

        [Fact]
        public static void CanRoundTrip_PublicClass_PrivateTestMethod()
        {
            var serializer = new BinaryFormatter();
            var testCase = Mocks.XunitTestCase<Serialization>("CanRoundTrip_PublicClass_PrivateTestMethod");
            var memoryStream = new MemoryStream();

            serializer.Serialize(memoryStream, testCase);
            memoryStream.Position = 0;

            Assert.DoesNotThrow(() => serializer.Deserialize(memoryStream));
        }

        [Fact]
        public static void CannotRoundTrip_PrivateClass()
        {
            var serializer = new BinaryFormatter();
            var testCase = Mocks.XunitTestCase<PrivateClass>("TestMethod");
            var memoryStream = new MemoryStream();

            serializer.Serialize(memoryStream, testCase);
            memoryStream.Position = 0;

            Assert.DoesNotThrow(() => serializer.Deserialize(memoryStream));
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
            var value = Mocks.XunitTestCase<ClassUnderTest>("TestMethod").UniqueID;

            Assert.NotEmpty(value);
        }

        [Fact]
        public static void UniqueID_Arguments()
        {
            var value42 = Mocks.XunitTestCase<ClassUnderTest>("TestMethod", testMethodArguments: new object[] { 42 }).UniqueID;
            var valueHelloWorld = Mocks.XunitTestCase<ClassUnderTest>("TestMethod", testMethodArguments: new object[] { "Hello, world!" }).UniqueID;
            var valueNull = Mocks.XunitTestCase<ClassUnderTest>("TestMethod", testMethodArguments: new object[] { (string)null }).UniqueID;

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
}