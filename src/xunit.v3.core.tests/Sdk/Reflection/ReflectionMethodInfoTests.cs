using System;
using Xunit;
using Xunit.Sdk;
using Xunit.v3;

public class ReflectionMethodInfoTests
{
	public class GetCustomAttributes
	{
		readonly ReflectionMethodInfo methodInfo;

		public GetCustomAttributes()
		{
			var method = typeof(ClassUnderTest).GetMethod(nameof(ClassUnderTest.MethodUnderTest)) ?? throw new InvalidOperationException("Could not find ClassUnderTest.MethodUnderTest");
			methodInfo = new ReflectionMethodInfo(method);
		}

		class NonGenericAttribute : Attribute { }

		class GenericAttribute<T> : Attribute { }

		class ClassUnderTest
		{
			[NonGeneric]
			[Generic<int>]
			public void MethodUnderTest() { }
		}

		[Fact]
		public void GuardClause()
		{
			Assert.Throws<ArgumentNullException>("attributeType", () => methodInfo.GetCustomAttributes(null!));
		}

		[Fact]
		public void UnknownAttributeType_Throws()
		{
			var ex = Record.Exception(() => methodInfo.GetCustomAttributes("UnknownAttribute"));

			var argEx = Assert.IsType<ArgumentException>(ex);
			Assert.StartsWith("Type name 'UnknownAttribute' could not be found", ex.Message);
			Assert.Equal("assemblyQualifiedTypeName", argEx.ParamName);
		}

		[Fact]
		public void AttributeNotFound_ReturnsEmptyList()
		{
			var result = methodInfo.GetCustomAttributes(typeof(FactAttribute));

			Assert.Empty(result);
		}

		[Fact]
		public void FindsNonGenericAttribute()
		{
			var result = methodInfo.GetCustomAttributes(typeof(NonGenericAttribute));

			var attr = Assert.Single(result);
			Assert.Equal("ReflectionMethodInfoTests+GetCustomAttributes+NonGenericAttribute", attr.AttributeType.Name);
		}

		[Theory]
		[InlineData(typeof(GenericAttribute<>))]
		[InlineData(typeof(GenericAttribute<int>))]
		public void FindsGenericAttribute(Type attributeType)
		{
			var result = methodInfo.GetCustomAttributes(attributeType);

			var attr = Assert.Single(result);
			Assert.Equal($"ReflectionMethodInfoTests+GetCustomAttributes+GenericAttribute`1[[{typeof(int).AssemblyQualifiedName}]]", attr.AttributeType.Name);
		}
	}
}
