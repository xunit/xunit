using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Sdk;

public class ReflectionAttributeInfoTests
{
    public class GetNamedArgument
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
}
