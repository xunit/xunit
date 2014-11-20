using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

public class ReflectionAbstractionExtensionsTests
{
    public class ToRuntimeMethod
    {
        [Fact]
        public static void WhenUsingReflectionMethodInfo_ReturnsExistingMethodInfo()
        {
            var methodInfo = Mocks.ReflectionMethodInfo<ToRuntimeMethod>("WhenUsingReflectionMethodInfo_ReturnsExistingMethodInfo");

            var result = methodInfo.ToRuntimeMethod();

            Assert.NotNull(result);
            Assert.Same(methodInfo.MethodInfo, result);
        }

        [Fact]
        public static void WhenUsingNonReflectionMethodInfo_MethodExists_ReturnsMethodInfo()
        {
            var typeInfo = Mocks.TypeInfo("ReflectionAbstractionExtensionsTests+ToRuntimeMethod", assemblyFileName: "test.xunit.execution.dll");
            var methodInfo = Mocks.MethodInfo("WhenUsingNonReflectionMethodInfo_MethodExists_ReturnsMethodInfo", isStatic: true, type: typeInfo);

            var result = methodInfo.ToRuntimeMethod();

            Assert.NotNull(result);
            Assert.Same(typeof(ToRuntimeMethod).GetMethod("WhenUsingNonReflectionMethodInfo_MethodExists_ReturnsMethodInfo"), result);
        }

        [Fact]
        public static void WhenUsingNonReflectionMethodInfo_MethodDoesNotExist_ReturnsNull()
        {
            var typeInfo = Mocks.TypeInfo("ReflectionAbstractionExtensionsTests+ToRuntimeMethod", assemblyFileName: "test.xunit.execution.dll");
            var methodInfo = Mocks.MethodInfo("UnknownMethod", isStatic: true, type: typeInfo);

            var result = methodInfo.ToRuntimeMethod();

            Assert.Null(result);
        }
    }

    public class ToRuntimeType
    {
        [Fact]
        public static void WhenUsingReflectionTypeInfo_ReturnsExistingType()
        {
            var typeInfo = Mocks.ReflectionTypeInfo<ToRuntimeType>();

            var result = typeInfo.ToRuntimeType();

            Assert.NotNull(result);
            Assert.Same(typeInfo.Type, result);
        }

        [Fact]
        public static void WhenUsingNonReflectionTypeInfo_TypeExists_ReturnsType()
        {
            var typeInfo = Mocks.TypeInfo("ReflectionAbstractionExtensionsTests+ToRuntimeType", assemblyFileName: "test.xunit.execution.dll");

            var result = typeInfo.ToRuntimeType();

            Assert.NotNull(result);
            Assert.Same(typeof(ToRuntimeType), result);
        }

        [Fact]
        public static void WhenUsingNonReflectionTypeInfo_TypeDoesNotExist_ReturnsNull()
        {
            var typeInfo = Mocks.TypeInfo("UnknownType", assemblyFileName: "test.xunit.execution.dll");

            var result = typeInfo.ToRuntimeType();

            Assert.Null(result);
        }
    }
}
