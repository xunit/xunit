using System;
using System.Collections.Generic;
using System.Reflection;
using Xunit;
using Xunit.Sdk;

namespace Xunit1
{
    public class ReflectorTests
    {
        public class GetMethod
        {
            [Fact]
            public void CanFindPublicMethod()
            {
                ITypeInfo typeInfo = Reflector.Wrap(typeof(TestClass));

                IMethodInfo result = typeInfo.GetMethod("PublicMethod");

                Assert.NotNull(result);
            }

            [Fact]
            public void CanFindPublicStaticMethod()
            {
                ITypeInfo typeInfo = Reflector.Wrap(typeof(TestClass));

                IMethodInfo result = typeInfo.GetMethod("PublicStaticMethod");

                Assert.NotNull(result);
            }

            [Fact]
            public void CanFindPrivateMethod()
            {
                ITypeInfo typeInfo = Reflector.Wrap(typeof(TestClass));

                IMethodInfo result = typeInfo.GetMethod("PrivateMethod");

                Assert.NotNull(result);
            }

            [Fact]
            public void CanFindPrivateStaticMethod()
            {
                ITypeInfo typeInfo = Reflector.Wrap(typeof(TestClass));

                IMethodInfo result = typeInfo.GetMethod("PrivateStaticMethod");

                Assert.NotNull(result);
            }

            [Fact]
            public void NonExistantMethodReturnsNull()
            {
                ITypeInfo typeInfo = Reflector.Wrap(typeof(TestClass));

                IMethodInfo result = typeInfo.GetMethod("NonExistantMethod");

                Assert.Null(result);
            }
        }

        public class GetMethods
        {
            [Fact]
            public void ReturnsPublicAndPrivateStaticAndNonStaticMethods()
            {
                ITypeInfo typeInfo = Reflector.Wrap(typeof(TestClass));

                List<IMethodInfo> methods = new List<IMethodInfo>(typeInfo.GetMethods());

                foreach (string name in new string[] { "PrivateMethod", "PrivateStaticMethod", "PublicMethod", "PublicStaticMethod" })
                    Assert.NotNull(methods.Find(methodInfo => methodInfo.Name == name));

                Assert.Null(methods.Find(methodInfo => methodInfo.Name == "Property"));
            }
        }

        public class Invoke
        {
            [Fact]
            public void ThrowsException()
            {
                MethodInfo method = typeof(TestMethodCommandClass).GetMethod("ThrowsException");
                IMethodInfo wrappedMethod = Reflector.Wrap(method);
                TestMethodCommandClass obj = new TestMethodCommandClass();

                Exception ex = Record.Exception(() => wrappedMethod.Invoke(obj));

                Assert.IsType<InvalidOperationException>(ex);
            }

            [Fact]
            public void ThrowsTargetInvocationException()
            {
                MethodInfo method = typeof(TestMethodCommandClass).GetMethod("ThrowsTargetInvocationException");
                IMethodInfo wrappedMethod = Reflector.Wrap(method);
                TestMethodCommandClass obj = new TestMethodCommandClass();

                Exception ex = Record.Exception(() => wrappedMethod.Invoke(obj));

                Assert.IsType<TargetInvocationException>(ex);
            }

            [Fact]
            public void TurnsTargetParameterCountExceptionIntoParameterCountMismatchException()
            {
                MethodInfo method = typeof(TestMethodCommandClass).GetMethod("ThrowsException");
                IMethodInfo wrappedMethod = Reflector.Wrap(method);
                TestMethodCommandClass obj = new TestMethodCommandClass();

                Exception ex = Record.Exception(() => wrappedMethod.Invoke(obj, "Hello world"));

                Assert.IsType<ParameterCountMismatchException>(ex);
            }

            internal class TestMethodCommandClass
            {
                public void ThrowsException()
                {
                    throw new InvalidOperationException();
                }

                public void ThrowsTargetInvocationException()
                {
                    throw new TargetInvocationException(null);
                }
            }
        }

        internal class TestClass
        {
            public string Property
            {
                get { return null; }
            }

            [Fact]
            void PrivateMethod() { }

            [Fact]
            static void PrivateStaticMethod() { }

            [Fact]
            public void PublicMethod() { }

            [Fact]
            public static void PublicStaticMethod() { }
        }
    }
}
