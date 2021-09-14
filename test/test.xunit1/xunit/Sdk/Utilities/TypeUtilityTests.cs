using System;
using System.Collections.Generic;
using Xunit;
using Xunit.Sdk;

namespace Xunit1
{
    public class TypeUtilityTests
    {
        [Fact]
        public void ClassContains2MethodsWithTestAttribute()
        {
            List<IMethodInfo> testMethods =
                new List<IMethodInfo>(TypeUtility.GetTestMethods(Reflector.Wrap(typeof(ClassWith2TestMethods))));

            Assert.Equal(2, testMethods.Count);
        }

        [Fact]
        public void ClassContainsNoMethodsWithTestAttribute()
        {
            List<IMethodInfo> testMethods =
                new List<IMethodInfo>(TypeUtility.GetTestMethods(Reflector.Wrap(typeof(ClassWithNoTestMethods))));

            Assert.Equal(0, testMethods.Count);
        }

        [Fact]
        public void ClassContainsTestMethods()
        {
            Assert.True(TypeUtility.ContainsTestMethods(Reflector.Wrap(typeof(ClassWith2TestMethods))));
        }

        [Fact]
        public void ClassDoesNotContainTestMethods()
        {
            Assert.False(TypeUtility.ContainsTestMethods(Reflector.Wrap(typeof(ClassWithNoTestMethods))));
        }

        [Fact]
        public void ClassHasRunWithAttributeReturnsTypeThatDoesNotImplementsITestClassCommand()
        {
            Type testClassType = typeof(CustomRunWithInvalidClass);

            Assert.Null(TypeUtility.GetRunWith(Reflector.Wrap(testClassType)));
        }

        [Fact]
        public void ClassHasRunWithAttributeReturnsTypeThatImplementsITestClassCommand()
        {
            Type testClassType = typeof(CustomRunWithClass);
            Type commandType = TypeUtility.GetRunWith(Reflector.Wrap(testClassType)).Type;

            Assert.True(TypeUtility.ImplementsITestClassCommand(Reflector.Wrap(commandType)));
        }

        [Fact]
        public void ClassHasRunWithAttributeWithITestClassCommand()
        {
            Type testClassType = typeof(CustomRunWithClass);
            Assert.True(TypeUtility.HasRunWith(Reflector.Wrap(testClassType)));
        }

        [Fact]
        public void BaseClassCanHaveRunWithAttribute()
        {
            Type testClassType = typeof(RunWithDerivedClass);

            Type commandType = TypeUtility.GetRunWith(Reflector.Wrap(testClassType)).Type;

            Assert.NotNull(commandType);
        }

        [Fact]
        public void CanDetermineIfClassIsStatic()
        {
            Assert.False(TypeUtility.IsStatic(Reflector.Wrap(typeof(NonStaticClass))));
            Assert.True(TypeUtility.IsStatic(Reflector.Wrap(typeof(StaticClass))));
        }

        class ClassWith2TestMethods
        {
            [Fact]
            public void Method1() { }

            [Fact]
            public void Method2() { }
        }

        internal static class StaticClass { }

        internal class NonStaticClass { }

        internal class ClassWithNoTestMethods
        {
            public void NonTestMethod() { }
        }

        [RunWith(typeof(StubRunner))]
        class CustomRunWithClass { }

        [RunWith(typeof(string))]
        class CustomRunWithInvalidClass { }

        class RunWithDerivedClass : CustomRunWithClass { }

        internal class StubRunner : ITestClassCommand
        {
            public int Count
            {
                get { return 0; }
            }

            public object ObjectUnderTest
            {
                get { return null; }
            }

            public ITypeInfo TypeUnderTest
            {
                get { return null; }
                set { }
            }

            public int ChooseNextTest(ICollection<IMethodInfo> testsLeftToRun)
            {
                return 0;
            }

            public Exception ClassFinish()
            {
                return null;
            }

            public Exception ClassStart()
            {
                return null;
            }

            public IEnumerable<ITestCommand> EnumerateTestCommands(IMethodInfo testMethod)
            {
                return null;
            }

            public IEnumerable<IMethodInfo> EnumerateTestMethods()
            {
                return null;
            }

            public bool IsTestMethod(IMethodInfo testMethod)
            {
                return false;
            }
        }
    }
}
