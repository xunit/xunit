using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit.Sdk;

namespace Xunit
{
    public class ArgumentFormatterTests
    {
        [Theory]
        [InlineData(typeof(int), "typeof(int)")]
        [InlineData(typeof(long), "typeof(long)")]
        [InlineData(typeof(string), "typeof(string)")]
        [InlineData(typeof(String), "typeof(string)")]
        [InlineData(typeof(int), "typeof(int)")]
        [InlineData(typeof(List<int>), "typeof(System.Collections.Generic.List<int>)")]
        [InlineData(typeof(Dictionary<int, string>), "typeof(System.Collections.Generic.Dictionary<int, string>)")]
        [InlineData(typeof(List<>), "typeof(System.Collections.Generic.List<>)")]
        [InlineData(typeof(Dictionary<,>), "typeof(System.Collections.Generic.Dictionary<,>)")]
        public void ArgumentFormatterFormatTypeNames(Type type, string expectedResult)
        {
            Assert.Equal(ArgumentFormatter.Format(type), expectedResult);
        }

        [Fact]
        public void ArgumentFormatterFormatTypeNameGenericTypeParameter()
        {
            var typeInfo = typeof (List<>).GetTypeInfo();
            var genericTypeParameters = typeInfo.GenericTypeParameters;
            var parameterType = genericTypeParameters.First();
                                
            Assert.Equal(ArgumentFormatter.Format(parameterType), "typeof(T)");
        }

        [Fact]
        public void ArgumentFormatterFormatTypeNameGenericTypeParameters()
        {
            var typeInfo = typeof(Dictionary<,>).GetTypeInfo();
            var genericTypeParameters = typeInfo.GenericTypeParameters;
            var parameterTKey = genericTypeParameters.First();

            Assert.Equal(ArgumentFormatter.Format(parameterTKey), "typeof(TKey)");

            var parameterTValue = genericTypeParameters.Last();
            Assert.Equal(ArgumentFormatter.Format(parameterTValue), "typeof(TValue)");

        }
    }
}