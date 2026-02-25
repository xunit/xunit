using System.Collections;
using System.Reflection;
using Xunit;
using Xunit.Sdk;

public class ReflectionExtensionsTests
{
	[Fact]
	public void GetDefaultValue()
	{
		Assert.Null(typeof(object).GetDefaultValue());
		Assert.Equal(0, typeof(int).GetDefaultValue());
	}

	[Theory(DisableDiscoveryEnumeration = true)]
	// No parameter
	[InlineData(nameof(DisplayNameClass.Parameterless), null, null, "Parameterless")]                           // match (no args)
	[InlineData(nameof(DisplayNameClass.Parameterless), new object?[0], null, "Parameterless()")]               // match (empty args)
	[InlineData(nameof(DisplayNameClass.Parameterless), new object?[] { 42 }, null, "Parameterless(???: 42)")]  // extra arg

	// One parameter
	[InlineData(nameof(DisplayNameClass.OneParameter), new object?[] { 42 }, null, "OneParameter(x: 42)")]  // match
	[InlineData(nameof(DisplayNameClass.OneParameter), new object?[0], null, "OneParameter(x: ???)")]       // missing arg

	// One generic parameter
#if XUNIT_AOT
	[InlineData(nameof(DisplayNameClass.OneGeneric), new object?[] { 42 }, new[] { typeof(int) }, "OneGeneric(x: 42)")]  // match
#else
	[InlineData(nameof(DisplayNameClass.OneGeneric), new object?[] { 42 }, new[] { typeof(int) }, "OneGeneric<Int32>(x: 42)")]  // match
#endif

	// Optional parameter
	[InlineData(nameof(DisplayNameClass.Optional), new object?[] { 42 }, null, "Optional(x: 42)")]  // match
	[InlineData(nameof(DisplayNameClass.Optional), new object?[0], null, "Optional(x: 2112)")]      // default value
	public void GetDisplayNameWithArguments(
		string methodName,
		object?[]? arguments,
		Type[]? genericTypes,
		string expectedDisplayName)
	{
		var method = typeof(DisplayNameClass).GetMethod(methodName);
		Assert.NotNull(method);

		var result = method.GetDisplayNameWithArguments(methodName, arguments, genericTypes);

		Assert.Equal(expectedDisplayName, result);
	}

#pragma warning disable CA1822 // Mark members as static
#pragma warning disable IDE0060 // Remove unused parameter

	class DisplayNameClass
	{
		public void Parameterless() { }
		public void OneParameter(int x) { }
		public void OneGeneric<T>(T x) { }
		public void Optional(int x = 2112) { }
	}

#pragma warning restore IDE0060 // Remove unused parameter
#pragma warning restore CA1822 // Mark members as static

	[Fact]
	public void IsFromLocalAssembly()
	{
		Assert.True(typeof(MyEnum).IsFromLocalAssembly());
#if NETFRAMEWORK
		Assert.False(typeof(System.Xml.ConformanceLevel).IsFromLocalAssembly());
#endif
	}

	enum MyEnum { }

	[Fact]
	public void IsNullable()
	{
		Assert.True(typeof(object).IsNullable());
		Assert.True(typeof(string).IsNullable());
		Assert.True(typeof(IEnumerable).IsNullable());
		Assert.True(typeof(char?).IsNullable());
		Assert.False(typeof(char).IsNullable());
	}

	[Fact]
	public void IsNullableEnum()
	{
		Assert.True(typeof(MyEnum?).IsNullableEnum());
		Assert.False(typeof(MyEnum).IsNullableEnum());
		Assert.False(typeof(char?).IsNullableEnum());
	}

	static void GenericMethod<T>(T _) { }

	[Fact]
	public void SafeName()
	{
		Assert.Equal("System.Object", typeof(object).SafeName());

		var genericMethod = typeof(ReflectionExtensionsTests).GetMethod(nameof(GenericMethod), BindingFlags.NonPublic | BindingFlags.Static);
		Assert.NotNull(genericMethod);
		var genericArgumentType = genericMethod.GetGenericArguments()[0];

		Assert.Equal("T", genericArgumentType.SafeName());
	}

	[Fact]
	public void ToCommaSeparatedList()
	{
		Assert.Equal("'System.Object', 'System.Int32'", new[] { typeof(object), typeof(int) }.ToCommaSeparatedList());
	}

	[Fact]
	public void ToSimpleName()
	{
		// Without namespace
		Assert.Equal("ReflectionExtensionsTests", typeof(ReflectionExtensionsTests).ToSimpleName());
		Assert.Equal("ReflectionExtensionsTests+DisplayNameClass", typeof(DisplayNameClass).ToSimpleName());

		// With namespace
		Assert.Equal("ParentClass", typeof(NS1.ParentClass).ToSimpleName());
		Assert.Equal("ParentClass+ChildClass", typeof(NS1.ParentClass.ChildClass).ToSimpleName());
	}

	[Fact]
	public void UnwrapNullable()
	{
		Assert.Equal(typeof(object), typeof(object).UnwrapNullable());
		Assert.Equal(typeof(int), typeof(int).UnwrapNullable());
		Assert.Equal(typeof(int), typeof(int?).UnwrapNullable());
	}
}

namespace NS1
{
	public class ParentClass
	{
		public class ChildClass { }
	}
}
