using Xunit;
using Xunit.Extensions;

namespace Xunit1.Extensions
{
    public class GenericTheoryTests
    {
        [Theory]
        [InlineData(42)]
        [InlineData(42L)]
        [InlineData(21.12)]
        [InlineData("Hello world")]
        public void OneGenericParameter<T>(T value)
        {
            Assert.Equal(value.GetType(), typeof(T));
        }

        [Theory]
        [InlineData(null)]
        public void TypeOfNullIsObject<T>(T value)
        {
            Assert.Null(value);
            Assert.Equal(typeof(object), typeof(T));
        }

        [Theory]
        [InlineData(42, "Hello world")]
        public void TwoGenericParametersOfTwoTypes<T1, T2>(T1 value1, T2 value2)
        {
            Assert.Equal<object>(42, value1);
            Assert.Equal<object>("Hello world", value2);
            Assert.Equal(typeof(int), typeof(T1));
            Assert.Equal(typeof(string), typeof(T2));
        }

        [Theory]
        [InlineData(42, 2112)]
        public void TwoCompatibleGenericParametersOfOneType<T>(T value1, T value2)
        {
            Assert.Equal<object>(42, value1);
            Assert.Equal<object>(2112, value2);
            Assert.Equal(typeof(int), typeof(T));
        }

        [Theory]
        [InlineData(42, "Hello world")]
        public void TwoIncompatibleGenericParametersOfOneType<T>(T value1, T value2)
        {
            Assert.Equal<object>(42, value1);
            Assert.Equal<object>("Hello world", value2);
            Assert.Equal(typeof(object), typeof(T));
        }

        [Theory]
        [InlineData(42)]
        public void GenericMethodWithoutGenericParameter<T>(int value)
        {
            Assert.Equal(42, value);
            Assert.Equal(typeof(object), typeof(T));
        }

        [Theory]
        [InlineData(null, "Hello world")]
        public void NullPlusReferenceTypeYieldsReferenceType<T>(T value1, T value2)
        {
            Assert.Equal(typeof(string), typeof(T));
        }

        [Theory]
        [InlineData(null, 42)]
        public void NullPlusValueTypeYieldsObject<T>(T value1, T value2)
        {
            Assert.Equal(typeof(object), typeof(T));
        }
    }
}
