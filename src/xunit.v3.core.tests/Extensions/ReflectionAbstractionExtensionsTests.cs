using Xunit;
using Xunit.v3;

public class ReflectionAbstractionExtensionsTests
{
	public class ToRuntimeMethod
	{
		[Fact]
		public static void WhenUsingReflectionMethodInfo_ReturnsExistingMethodInfo()
		{
			var methodInfo = Mocks.MethodInfo<ToRuntimeMethod>("WhenUsingReflectionMethodInfo_ReturnsExistingMethodInfo");

			var result = methodInfo.ToRuntimeMethod();

			Assert.NotNull(result);
			Assert.Same(methodInfo.MethodInfo, result);
		}

		[Fact]
		public static void WhenUsingNonReflectionMethodInfo_MethodExists_ReturnsMethodInfo()
		{
#if BUILD_X86
			var typeInfo = Mocks.TypeInfo("ReflectionAbstractionExtensionsTests+ToRuntimeMethod", assemblyFileName: "xunit.v3.core.tests.x86.exe");
#else
			var typeInfo = Mocks.TypeInfo("ReflectionAbstractionExtensionsTests+ToRuntimeMethod", assemblyFileName: "xunit.v3.core.tests.exe");
#endif
			var methodInfo = Mocks.MethodInfo("WhenUsingNonReflectionMethodInfo_MethodExists_ReturnsMethodInfo", isStatic: true, type: typeInfo);

			var result = methodInfo.ToRuntimeMethod();

			Assert.NotNull(result);
			Assert.Same(typeof(ToRuntimeMethod).GetMethod("WhenUsingNonReflectionMethodInfo_MethodExists_ReturnsMethodInfo"), result);
		}

		[Fact]
		public static void WhenUsingNonReflectionMethodInfo_MethodDoesNotExist_ReturnsNull()
		{
			var typeInfo = Mocks.TypeInfo("ReflectionAbstractionExtensionsTests+ToRuntimeMethod", assemblyFileName: "xunit.v3.core.tests.exe");
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
			var typeInfo = Mocks.TypeInfo<ToRuntimeType>();

			var result = typeInfo.ToRuntimeType();

			Assert.NotNull(result);
			Assert.Same(typeInfo.Type, result);
		}

		[Fact]
		public static void WhenUsingNonReflectionTypeInfo_TypeExists_ReturnsType()
		{
#if BUILD_X86
			var typeInfo = Mocks.TypeInfo("ReflectionAbstractionExtensionsTests+ToRuntimeType", assemblyFileName: "xunit.v3.core.tests.x86.exe");
#else
			var typeInfo = Mocks.TypeInfo("ReflectionAbstractionExtensionsTests+ToRuntimeType", assemblyFileName: "xunit.v3.core.tests.exe");
#endif

			var result = typeInfo.ToRuntimeType();

			Assert.NotNull(result);
			Assert.Same(typeof(ToRuntimeType), result);
		}

		[Fact]
		public static void WhenUsingNonReflectionTypeInfo_TypeDoesNotExist_ReturnsNull()
		{
			var typeInfo = Mocks.TypeInfo("UnknownType", assemblyFileName: "xunit.v3.core.tests.exe");

			var result = typeInfo.ToRuntimeType();

			Assert.Null(result);
		}
	}
}
