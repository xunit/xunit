using System;
using Xunit;
using Xunit.Sdk;
using Xunit.v3;

public class ReflectionTypeInfoTests
{
	public class GetCustomAttributes
	{
		readonly ReflectionTypeInfo typeInfo;

		public GetCustomAttributes()
		{
			typeInfo = new ReflectionTypeInfo(typeof(ClassUnderTest));
		}

		class NonGenericAttribute : Attribute { }

		class GenericAttribute<T> : Attribute { }

		[NonGeneric]
		[Generic<int>]
		class ClassUnderTest { }

		[Fact]
		public void GuardClause()
		{
			Assert.Throws<ArgumentNullException>("attributeType", () => typeInfo.GetCustomAttributes(null!));
		}

		[Fact]
		public void UnknownAttributeType_Throws()
		{
			var ex = Record.Exception(() => typeInfo.GetCustomAttributes("UnknownAttribute"));

			var argEx = Assert.IsType<ArgumentException>(ex);
			Assert.StartsWith("Type name 'UnknownAttribute' could not be found", ex.Message);
			Assert.Equal("assemblyQualifiedTypeName", argEx.ParamName);
		}

		[Fact]
		public void AttributeNotFound_ReturnsEmptyList()
		{
			var result = typeInfo.GetCustomAttributes(typeof(FactAttribute));

			Assert.Empty(result);
		}

		[Fact]
		public void FindsNonGenericAttribute()
		{
			var result = typeInfo.GetCustomAttributes(typeof(NonGenericAttribute));

			var attr = Assert.Single(result);
			Assert.Equal("ReflectionTypeInfoTests+GetCustomAttributes+NonGenericAttribute", attr.AttributeType.Name);
		}

		[Theory]
		[InlineData(typeof(GenericAttribute<>))]
		[InlineData(typeof(GenericAttribute<int>))]
		public void FindsGenericAttribute(Type attributeType)
		{
			var result = typeInfo.GetCustomAttributes(attributeType);

			var attr = Assert.Single(result);
			Assert.Equal($"ReflectionTypeInfoTests+GetCustomAttributes+GenericAttribute`1[[{typeof(int).AssemblyQualifiedName}]]", attr.AttributeType.Name);
		}
	}
}
