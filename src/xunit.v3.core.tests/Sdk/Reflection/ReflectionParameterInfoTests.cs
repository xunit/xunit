using System;
using System.Linq;
using Xunit;
using Xunit.Sdk;
using Xunit.v3;

public class ReflectionParameterInfoTests
{
	public class GetCustomAttributes
	{
		readonly ReflectionParameterInfo parameterInfo;

		public GetCustomAttributes()
		{
			var method = typeof(ClassUnderTest).GetMethod(nameof(ClassUnderTest.MethodUnderTest)) ?? throw new InvalidOperationException("Could not find ClassUnderTest.MethodUnderTest");
			parameterInfo = new ReflectionParameterInfo(method.GetParameters().Single());
		}

		class NonGenericAttribute : Attribute { }

		class GenericAttribute<T> : Attribute { }

		class ClassUnderTest
		{
			public void MethodUnderTest([NonGeneric, Generic<int>] int _) { }
		}

		[Fact]
		public void GuardClause()
		{
			Assert.Throws<ArgumentNullException>("attributeType", () => parameterInfo.GetCustomAttributes(null!));
		}

		[Fact]
		public void UnknownAttributeType_Throws()
		{
			var ex = Record.Exception(() => parameterInfo.GetCustomAttributes("UnknownAttribute"));

			var argEx = Assert.IsType<ArgumentException>(ex);
			Assert.StartsWith("Type name 'UnknownAttribute' could not be found", ex.Message);
			Assert.Equal("assemblyQualifiedTypeName", argEx.ParamName);
		}

		[Fact]
		public void AttributeNotFound_ReturnsEmptyList()
		{
			var result = parameterInfo.GetCustomAttributes(typeof(FactAttribute));

			Assert.Empty(result);
		}

		[Fact]
		public void FindsNonGenericAttribute()
		{
			var result = parameterInfo.GetCustomAttributes(typeof(NonGenericAttribute));

			var attr = Assert.Single(result);
			Assert.Equal("ReflectionParameterInfoTests+GetCustomAttributes+NonGenericAttribute", attr.AttributeType.Name);
		}

		[Theory]
		[InlineData(typeof(GenericAttribute<>))]
		[InlineData(typeof(GenericAttribute<int>))]
		public void FindsGenericAttribute(Type attributeType)
		{
			var result = parameterInfo.GetCustomAttributes(attributeType);

			var attr = Assert.Single(result);
			Assert.Equal($"ReflectionParameterInfoTests+GetCustomAttributes+GenericAttribute`1[[{typeof(int).AssemblyQualifiedName}]]", attr.AttributeType.Name);
		}
	}
}
