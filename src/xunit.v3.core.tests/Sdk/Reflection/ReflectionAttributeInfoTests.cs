using System;
using System.Linq;
using System.Reflection;
using Xunit;
using Xunit.Sdk;
using Xunit.v3;

public class ReflectionAttributeInfoTests
{
	public class GetCustomAttributes
	{
		readonly ReflectionAttributeInfo attributeInfo;

		class NonGenericAttribute : Attribute { }

		class GenericAttribute<T> : Attribute { }

		[NonGeneric]
		[GenericAttribute<int>]
		class AttributeUnderTest : Attribute { }

		[AttributeUnderTest]
		class ClassUnderTest { }

		public GetCustomAttributes()
		{
			var attributeData = CustomAttributeData.GetCustomAttributes(typeof(ClassUnderTest)).Single(cad => cad.AttributeType == typeof(AttributeUnderTest));
			attributeInfo = new ReflectionAttributeInfo(attributeData);
		}

		[Fact]
		public void GuardClause()
		{
			Assert.Throws<ArgumentNullException>("attributeType", () => attributeInfo.GetCustomAttributes(null!));
		}

		[Fact]
		public void UnknownAttributeType_Throws()
		{
			var ex = Record.Exception(() => attributeInfo.GetCustomAttributes("UnknownAttribute"));

			var argEx = Assert.IsType<ArgumentException>(ex);
			Assert.StartsWith("Type name 'UnknownAttribute' could not be found", ex.Message);
			Assert.Equal("assemblyQualifiedTypeName", argEx.ParamName);
		}

		[Fact]
		public void AttributeNotFound_ReturnsEmptyList()
		{
			var result = attributeInfo.GetCustomAttributes(typeof(FactAttribute));

			Assert.Empty(result);
		}

		[Fact]
		public void FindsNonGenericAttribute()
		{
			var result = attributeInfo.GetCustomAttributes(typeof(NonGenericAttribute));

			var attr = Assert.Single(result);
			Assert.Equal("ReflectionAttributeInfoTests+GetCustomAttributes+NonGenericAttribute", attr.AttributeType.Name);
		}

		[Theory]
		[InlineData(typeof(GenericAttribute<>))]
		[InlineData(typeof(GenericAttribute<int>))]
		public void FindsGenericAttribute(Type attributeType)
		{
			var result = attributeInfo.GetCustomAttributes(attributeType);

			var attr = Assert.Single(result);
			Assert.Equal($"ReflectionAttributeInfoTests+GetCustomAttributes+GenericAttribute`1[[{typeof(int).AssemblyQualifiedName}]]", attr.AttributeType.Name);
		}
	}

	public class GetNamedArgument
	{
		public class NamedValueDoesNotExist
		{
			class AttributeUnderTest : Attribute { }

			[AttributeUnderTest]
			class ClassWithAttribute { }

			[Fact]
			public void ReturnsNull()
			{
				var attributeData = CustomAttributeData.GetCustomAttributes(typeof(ClassWithAttribute)).Single(cad => cad.AttributeType == typeof(AttributeUnderTest));
				var attributeInfo = new ReflectionAttributeInfo(attributeData);

				var result = attributeInfo.GetNamedArgument<int?>("IntValue");

				Assert.Null(result);
			}
		}

		public class Properties
		{
			class AttributeUnderTest : Attribute
			{
				public int IntValue { get; set; }
			}

			[AttributeUnderTest(IntValue = 42)]
			class ClassWithAttributeValue { }

			[Fact]
			public void ReturnsValue()
			{
				var attributeData = CustomAttributeData.GetCustomAttributes(typeof(ClassWithAttributeValue)).Single(cad => cad.AttributeType == typeof(AttributeUnderTest));
				var attributeInfo = new ReflectionAttributeInfo(attributeData);

				var result = attributeInfo.GetNamedArgument<int>("IntValue");

				Assert.Equal(42, result);
			}

			[AttributeUnderTest]
			class ClassWithoutAttributeValue { }

			[Fact]
			public void ReturnsDefaultValueWhenValueIsNotSet()
			{
				var attributeData = CustomAttributeData.GetCustomAttributes(typeof(ClassWithoutAttributeValue)).Single(cad => cad.AttributeType == typeof(AttributeUnderTest));
				var attributeInfo = new ReflectionAttributeInfo(attributeData);

				var result = attributeInfo.GetNamedArgument<int>("IntValue");

				Assert.Equal(0, result);
			}
		}

		public class Fields
		{
			class AttributeUnderTest : Attribute
			{
				public int IntValue;
			}

			[AttributeUnderTest(IntValue = 42)]
			class ClassWithAttributeValue { }

			[Fact]
			public void ReturnsValue()
			{
				var attributeData = CustomAttributeData.GetCustomAttributes(typeof(ClassWithAttributeValue)).Single(cad => cad.AttributeType == typeof(AttributeUnderTest));
				var attributeInfo = new ReflectionAttributeInfo(attributeData);

				var result = attributeInfo.GetNamedArgument<int>("IntValue");

				Assert.Equal(42, result);
			}

			[AttributeUnderTest]
			class ClassWithoutAttributeValue { }

			[Fact]
			public void ReturnsDefaultValueWhenValueIsNotSet()
			{
				var attributeData = CustomAttributeData.GetCustomAttributes(typeof(ClassWithoutAttributeValue)).Single(cad => cad.AttributeType == typeof(AttributeUnderTest));
				var attributeInfo = new ReflectionAttributeInfo(attributeData);

				var result = attributeInfo.GetNamedArgument<int>("IntValue");

				Assert.Equal(0, result);
			}
		}
	}
}
