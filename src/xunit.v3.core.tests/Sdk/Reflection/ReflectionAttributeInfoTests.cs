using System;
using System.Linq;
using System.Reflection;
using Xunit;
using Xunit.Sdk;

public class ReflectionAttributeInfoTests
{
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
