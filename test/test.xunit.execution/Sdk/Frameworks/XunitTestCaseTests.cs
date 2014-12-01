﻿using System;
using System.Collections.Generic;
using System.Linq;
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
            public IEnumerable<KeyValuePair<string, string>> GetTraits(IAttributeInfo traitAttribute, object[] testMethodArguments)
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
        public static void CustomDisplayNameWithArguments()
        {
            var param1 = Mocks.ParameterInfo("p1");
            var param2 = Mocks.ParameterInfo("p2");
            var param3 = Mocks.ParameterInfo("p3");
            var testMethod = Mocks.TestMethod(displayName: "Custom Display Name", parameters: new[] { param1, param2, param3 });
            var arguments = new object[] { 42, "Hello, world!", 'A' };

            var testCase = new XunitTestCase(testMethod, arguments);

            Assert.Equal("Custom Display Name(p1: 42, p2: \"Hello, world!\", p3: 'A')", testCase.DisplayName);
        }
    }
}