using System;
using Xunit;
using Xunit.Sdk;
using Xunit.v3;

public class ReflectionAssemblyInfoTests
{
	public class GetCustomAttributes
	{
		readonly ReflectionAssemblyInfo assemblyInfo;

		public class NonGenericAttribute : Attribute { }

		public class GenericAttribute<T> : Attribute { }

		public GetCustomAttributes()
		{
			var attributeInfos = new[] { Mocks.AttributeInfo<NonGenericAttribute>(), Mocks.AttributeInfo<GenericAttribute<int>>() };
			assemblyInfo = new ReflectionAssemblyInfo(typeof(ReflectionAssemblyInfo).Assembly, attributeInfos);
		}

		[Fact]
		public void GuardClause()
		{
			Assert.Throws<ArgumentNullException>("attributeType", () => assemblyInfo.GetCustomAttributes(null!));
		}

		[Fact]
		public void UnknownAttributeType_Throws()
		{
			var ex = Record.Exception(() => assemblyInfo.GetCustomAttributes("UnknownAttribute"));

			var argEx = Assert.IsType<ArgumentException>(ex);
			Assert.StartsWith("Type name 'UnknownAttribute' could not be found", ex.Message);
			Assert.Equal("assemblyQualifiedTypeName", argEx.ParamName);
		}

		[Fact]
		public void AttributeNotFound_ReturnsEmptyList()
		{
			var result = assemblyInfo.GetCustomAttributes(typeof(FactAttribute));

			Assert.Empty(result);
		}

		[Fact]
		public void FindsNonGenericAttribute()
		{
			var result = assemblyInfo.GetCustomAttributes(typeof(NonGenericAttribute));

			var attr = Assert.Single(result);
			Assert.Equal("ReflectionAssemblyInfoTests+GetCustomAttributes+NonGenericAttribute", attr.AttributeType.Name);
		}

		[Theory]
		[InlineData(typeof(GenericAttribute<>))]
		[InlineData(typeof(GenericAttribute<int>))]
		public void FindsGenericAttribute(Type attributeType)
		{
			var result = assemblyInfo.GetCustomAttributes(attributeType);

			var attr = Assert.Single(result);
			Assert.Equal($"ReflectionAssemblyInfoTests+GetCustomAttributes+GenericAttribute`1[[{typeof(int).AssemblyQualifiedName}]]", attr.AttributeType.Name);
		}
	}
}
