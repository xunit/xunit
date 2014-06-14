using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using NSubstitute;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;
using IAttributeInfo = Xunit.Abstractions.IAttributeInfo;

public class XunitTestCaseTests
{
    [Fact]
    public static void DefaultFactAttribute()
    {
        var testCollection = new XunitTestCollection();
        var fact = Mocks.FactAttribute();
        var method = Mocks.MethodInfo();
        var type = Mocks.TypeInfo(methods: new[] { method });
        var assmInfo = Mocks.AssemblyInfo(types: new[] { type });

        var testCase = new XunitTestCase(testCollection, assmInfo, type, method, fact);

        Assert.Equal("MockType.MockMethod", testCase.DisplayName);
        Assert.Null(testCase.SkipReason);
        Assert.Empty(testCase.Traits);
    }

    [Fact]
    public static void SkipReason()
    {
        var testCollection = new XunitTestCollection();
        var fact = Mocks.FactAttribute(skip: "Skip Reason");
        var method = Mocks.MethodInfo();
        var type = Mocks.TypeInfo(methods: new[] { method });
        var assmInfo = Mocks.AssemblyInfo(types: new[] { type });

        var testCase = new XunitTestCase(testCollection, assmInfo, type, method, fact);

        Assert.Equal("Skip Reason", testCase.SkipReason);
    }

    [Fact]
    public static void DisposesArguments()
    {
        var disposable1 = Substitute.For<IDisposable>();
        var disposable2 = Substitute.For<IDisposable>();
        var testCollection = new XunitTestCollection();
        var fact = Mocks.FactAttribute();
        var method = Mocks.MethodInfo();
        var type = Mocks.TypeInfo(methods: new[] { method });
        var assmInfo = Mocks.AssemblyInfo(types: new[] { type });
        var testCase = new XunitTestCase(testCollection, assmInfo, type, method, fact, new[] { disposable1, disposable2 });

        testCase.Dispose();

        disposable1.Received(1).Dispose();
        disposable2.Received(1).Dispose();
    }

    public class Traits : AcceptanceTest
    {
        [Fact]
        public static void TraitsOnTestMethod()
        {
            var testCollection = new XunitTestCollection();
            var fact = Mocks.FactAttribute();
            var trait1 = Mocks.TraitAttribute("Trait1", "Value1");
            var trait2 = Mocks.TraitAttribute("Trait2", "Value2");
            var method = Mocks.MethodInfo(attributes: new[] { trait1, trait2 });
            var type = Mocks.TypeInfo(methods: new[] { method });
            var assmInfo = Mocks.AssemblyInfo(types: new[] { type });

            var testCase = new XunitTestCase(testCollection, assmInfo, type, method, fact);

            Assert.Equal("Value1", Assert.Single(testCase.Traits["Trait1"]));
            Assert.Equal("Value2", Assert.Single(testCase.Traits["Trait2"]));
        }

        [Fact]
        public static void TraitsOnTestClass()
        {
            var testCollection = new XunitTestCollection();
            var fact = Mocks.FactAttribute();
            var trait1 = Mocks.TraitAttribute("Trait1", "Value1");
            var trait2 = Mocks.TraitAttribute("Trait2", "Value2");
            var method = Mocks.MethodInfo();
            var type = Mocks.TypeInfo(methods: new[] { method }, attributes: new[] { trait1, trait2 });
            var assmInfo = Mocks.AssemblyInfo(types: new[] { type });

            var testCase = new XunitTestCase(testCollection, assmInfo, type, method, fact);

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
            var testCollection = new XunitTestCollection();
            var fact = Mocks.FactAttribute(displayName: "Custom Display Name");
            var method = Mocks.MethodInfo();
            var type = Mocks.TypeInfo(methods: new[] { method });
            var assmInfo = Mocks.AssemblyInfo(types: new[] { type });

            var testCase = new XunitTestCase(testCollection, assmInfo, type, method, fact);

            Assert.Equal("Custom Display Name", testCase.DisplayName);
        }

        [Fact]
        public static void CorrectNumberOfTestArguments()
        {
            var testCollection = new XunitTestCollection();
            var fact = Mocks.FactAttribute();
            var param1 = Mocks.ParameterInfo("p1");
            var param2 = Mocks.ParameterInfo("p2");
            var param3 = Mocks.ParameterInfo("p3");
            var method = Mocks.MethodInfo(parameters: new[] { param1, param2, param3 });
            var type = Mocks.TypeInfo(methods: new[] { method });
            var assmInfo = Mocks.AssemblyInfo(types: new[] { type });
            var arguments = new object[] { 42, "Hello, world!", 'A' };

            var testCase = new XunitTestCase(testCollection, assmInfo, type, method, fact, arguments);

            Assert.Equal("MockType.MockMethod(p1: 42, p2: \"Hello, world!\", p3: 'A')", testCase.DisplayName);
        }

        [Fact]
        public static void NotEnoughTestArguments()
        {
            var testCollection = new XunitTestCollection();
            var fact = Mocks.FactAttribute();
            var param = Mocks.ParameterInfo("p1");
            var method = Mocks.MethodInfo(parameters: new[] { param });
            var type = Mocks.TypeInfo(methods: new[] { method });
            var assmInfo = Mocks.AssemblyInfo(types: new[] { type });

            var testCase = new XunitTestCase(testCollection, assmInfo, type, method, fact, new object[0]);

            Assert.Equal("MockType.MockMethod(p1: ???)", testCase.DisplayName);
        }

        [CulturedFact]
        public static void TooManyTestArguments()
        {
            var testCollection = new XunitTestCollection();
            var fact = Mocks.FactAttribute();
            var param = Mocks.ParameterInfo("p1");
            var method = Mocks.MethodInfo(parameters: new[] { param });
            var type = Mocks.TypeInfo(methods: new[] { method });
            var assmInfo = Mocks.AssemblyInfo(types: new[] { type });
            var arguments = new object[] { 42, 21.12 };

            var testCase = new XunitTestCase(testCollection, assmInfo, type, method, fact, arguments);

            Assert.Equal(String.Format("MockType.MockMethod(p1: 42, ???: {0})", 21.12), testCase.DisplayName);
        }
    }

    public class Serialization
    {
        [Fact]
        public static void CanRoundTrip_PublicClass_PublicTestMethod()
        {
            var serializer = new BinaryFormatter();
            var testCase = Create(typeof(Serialization), "CanRoundTrip_PublicClass_PublicTestMethod");
            var memoryStream = new MemoryStream();

            serializer.Serialize(memoryStream, testCase);
            memoryStream.Position = 0;

            Assert.DoesNotThrow(() => serializer.Deserialize(memoryStream));
        }

        [Fact]
        public static void CanRoundTrip_PublicClass_PrivateTestMethod()
        {
            var serializer = new BinaryFormatter();
            var testCase = Create(typeof(Serialization), "CanRoundTrip_PublicClass_PrivateTestMethod");
            var memoryStream = new MemoryStream();

            serializer.Serialize(memoryStream, testCase);
            memoryStream.Position = 0;

            Assert.DoesNotThrow(() => serializer.Deserialize(memoryStream));
        }

        [Fact]
        public static void CannotRoundTrip_PrivateClass()
        {
            var serializer = new BinaryFormatter();
            var testCase = Create(typeof(PrivateClass), "TestMethod");
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
            var value = Create(typeof(ClassUnderTest), "TestMethod").UniqueID;

            Assert.NotEmpty(value);
        }

        [Fact]
        public static void UniqueID_Arguments()
        {
            var value42 = Create(typeof(ClassUnderTest), "TestMethod", 42).UniqueID;
            var valueHelloWorld = Create(typeof(ClassUnderTest), "TestMethod", "Hello, world!").UniqueID;
            var valueNull = Create(typeof(ClassUnderTest), "TestMethod", (string)null).UniqueID;

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

    static XunitTestCase Create(Type typeUnderTest, string methodName, params object[] arguments)
    {
        var testCollection = new XunitTestCollection();
        var methodUnderTest = typeUnderTest.GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
        var assembly = Reflector.Wrap(typeUnderTest.Assembly);
        var type = Reflector.Wrap(typeUnderTest);
        var method = Reflector.Wrap(methodUnderTest);
        var fact = Reflector.Wrap(CustomAttributeData.GetCustomAttributes(methodUnderTest)
                                                     .Single(cad => cad.AttributeType == typeof(FactAttribute)));

        return new XunitTestCase(testCollection, assembly, type, method, fact, arguments.Length == 0 ? null : arguments);
    }
}