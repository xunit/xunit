using System;
using System.Xml;
using Xunit;
using Xunit.Internal;
using Xunit.Sdk;

public class ReflectionExtensionsTests
{
	public class IsAssignableFrom
	{
		public static TheoryData<Type, Type, bool> AssignableData = new()
		{
			/////// REFERENCE TYPES ///////

			// object o = new object()
			{ typeof(object), typeof(object), true },

			// object o = 12
			{ typeof(object), typeof(int), true },

			// object o = new MySerializable()
			{ typeof(object), typeof(MySerializable), true },

			// object o = "Hello world"
			{ typeof(object), typeof(string), true },

			// string s = new object()
			{ typeof(string), typeof(object), false },

			/////// VALUE TYPES ///////

			// int i = 12
			{ typeof(int), typeof(int), true },

			// int i = new object()
			{ typeof(int), typeof(object), false },

			// int? i1 = null
			// int i2 = i1
			{ typeof(int), typeof(int?), false },

			/////// NULLABLE VALUE TYPES ///////

			// int? i = 12
			{ typeof(int?), typeof(int), true },

			// int? i1 = null
			// int? i2 = i1
			{ typeof(int?), typeof(int?), true },

			/////// INTERFACES ///////

			// IXunitSerializable s = new MySerializable()
			{ typeof(IXunitSerializable), typeof(MySerializable), true },

			// IXunitSerializable s = new SomeOtherXunitSerializable()
			// MySerialization s2 = s
			{ typeof(MySerializable), typeof(IXunitSerializable), false },
		};

		[Theory]
		[MemberData(nameof(AssignableData))]
		public void TypeToTypeBaseline(
			Type baseType,
			Type testType,
			bool expected)
		{
			var result = baseType.IsAssignableFrom(testType);

			Assert.Equal(expected, result);
		}

		[Theory]
		[MemberData(nameof(AssignableData))]
		public void TypeInfoToType(
			Type baseType,
			Type testType,
			bool expected)
		{
			var result = Reflector.Wrap(baseType).IsAssignableFrom(testType);

			Assert.Equal(expected, result);
		}

		[Theory]
		[MemberData(nameof(AssignableData))]
		public void TypeInfoToTypeInfo(
			Type baseType,
			Type testType,
			bool expected)
		{
			var result = Reflector.Wrap(baseType).IsAssignableFrom(Reflector.Wrap(testType));

			Assert.Equal(expected, result);
		}

		class MySerializable : IXunitSerializable
		{
			public void Deserialize(IXunitSerializationInfo info) =>
				throw new NotImplementedException();

			public void Serialize(IXunitSerializationInfo info) =>
				throw new NotImplementedException();
		}
	}

	public class IsFromLocalAssembly
	{
		[Fact]
		public void ForType()
		{
			Assert.True(typeof(MyEnum).IsFromLocalAssembly());
#if NETFRAMEWORK
			if (!EnvironmentHelper.IsMono)
				Assert.False(typeof(ConformanceLevel).IsFromLocalAssembly());
#endif
		}

		[Fact]
		public void ForTypeInfo()
		{
			Assert.True(Reflector.Wrap(typeof(MyEnum)).IsFromLocalAssembly());
#if NETFRAMEWORK
			if (!EnvironmentHelper.IsMono)
				Assert.False(Reflector.Wrap(typeof(ConformanceLevel)).IsFromLocalAssembly());
#endif
		}

		enum MyEnum { }
	}

	public class IsNullable
	{
		[Fact]
		public void ForType()
		{
			Assert.True(typeof(object).IsNullable());
			Assert.True(typeof(string).IsNullable());
			Assert.True(typeof(IXunitSerializable).IsNullable());
			Assert.True(typeof(char?).IsNullable());
			Assert.False(typeof(char).IsNullable());
		}

		[Fact]
		public void ForTypeInfo()
		{
			Assert.True(Reflector.Wrap(typeof(object)).IsNullable());
			Assert.True(Reflector.Wrap(typeof(string)).IsNullable());
			Assert.True(Reflector.Wrap(typeof(IXunitSerializable)).IsNullable());
			Assert.True(Reflector.Wrap(typeof(char?)).IsNullable());
			Assert.False(Reflector.Wrap(typeof(char)).IsNullable());
		}
	}

	public class ToRuntimeMethod
	{
		[Fact]
		public static void WhenUsingReflectionMethodInfo_ReturnsExistingMethodInfo()
		{
			var methodInfo = TestData.MethodInfo<ToRuntimeMethod>("WhenUsingReflectionMethodInfo_ReturnsExistingMethodInfo");

			var result = methodInfo.ToRuntimeMethod();

			Assert.NotNull(result);
			Assert.Same(methodInfo.MethodInfo, result);
		}

		[Fact]
		public static void WhenUsingNonReflectionMethodInfo_MethodExists_ReturnsMethodInfo()
		{
#if BUILD_X86
			var typeInfo = Mocks.TypeInfo("ReflectionExtensionsTests+ToRuntimeMethod", assemblyFileName: "xunit.v3.common.tests.x86.exe");
#else
			var typeInfo = Mocks.TypeInfo("ReflectionExtensionsTests+ToRuntimeMethod", assemblyFileName: "xunit.v3.common.tests.exe");
#endif
			var methodInfo = Mocks.MethodInfo("WhenUsingNonReflectionMethodInfo_MethodExists_ReturnsMethodInfo", isStatic: true, type: typeInfo);

			var result = methodInfo.ToRuntimeMethod();

			Assert.NotNull(result);
			Assert.Same(typeof(ToRuntimeMethod).GetMethod("WhenUsingNonReflectionMethodInfo_MethodExists_ReturnsMethodInfo"), result);
		}

		[Fact]
		public static void WhenUsingNonReflectionMethodInfo_MethodDoesNotExist_ReturnsNull()
		{
			var typeInfo = Mocks.TypeInfo("ReflectionExtensionsTests+ToRuntimeMethod", assemblyFileName: "xunit.v3.common.tests.exe");
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
			var typeInfo = TestData.TypeInfo<ToRuntimeType>();

			var result = typeInfo.ToRuntimeType();

			Assert.NotNull(result);
			Assert.Same(typeInfo.Type, result);
		}

		[Fact]
		public static void WhenUsingNonReflectionTypeInfo_TypeExists_ReturnsType()
		{
#if BUILD_X86
			var typeInfo = Mocks.TypeInfo("ReflectionExtensionsTests+ToRuntimeType", assemblyFileName: "xunit.v3.common.tests.x86.exe");
#else
			var typeInfo = Mocks.TypeInfo("ReflectionExtensionsTests+ToRuntimeType", assemblyFileName: "xunit.v3.common.tests.exe");
#endif

			var result = typeInfo.ToRuntimeType();

			Assert.NotNull(result);
			Assert.Same(typeof(ToRuntimeType), result);
		}

		[Fact]
		public static void WhenUsingNonReflectionTypeInfo_TypeDoesNotExist_ReturnsNull()
		{
			var typeInfo = Mocks.TypeInfo("UnknownType", assemblyFileName: "xunit.v3.common.tests.exe");

			var result = typeInfo.ToRuntimeType();

			Assert.Null(result);
		}
	}

	public class UnwrapNullable
	{
		[Fact]
		public void ForType()
		{
			Assert.Equal(typeof(object), typeof(object).UnwrapNullable());
			Assert.Equal(typeof(int), typeof(int).UnwrapNullable());
			Assert.Equal(typeof(int), typeof(int?).UnwrapNullable());
		}

		[Fact]
		public void ForTypeInfo()
		{
			Assert.Equal("System.Object", Reflector.Wrap(typeof(object)).UnwrapNullable().Name);
			Assert.Equal("System.Int32", Reflector.Wrap(typeof(int)).UnwrapNullable().Name);
			Assert.Equal("System.Int32", Reflector.Wrap(typeof(int?)).UnwrapNullable().Name);
		}
	}
}
