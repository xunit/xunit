using System;
using Xunit;
using Xunit.Sdk;

public class ArgumentFormatterTests
{
    [Fact]
    public static void NullValue()
    {
        Assert.Equal("null", ArgumentFormatter.Format(null));
    }

    [Fact]
    public static void StringValue()
    {
        Assert.Equal("\"Hello, world!\"", ArgumentFormatter.Format("Hello, world!"));
    }

    [Fact]
    public static void StringValueTruncatedPast50Characters()
    {
        Assert.Equal("\"----|----1----|----2----|----3----|----4----|----5\"...", ArgumentFormatter.Format("----|----1----|----2----|----3----|----4----|----5-"));
    }

    [Fact]
    public static void CharacterValue()
    {
        Assert.Equal("'a'", ArgumentFormatter.Format('a'));
    }

    [Fact]
    public static void DecimalValue()
    {
        Assert.Equal("123.45", ArgumentFormatter.Format(123.45M));
    }

    [Fact]
    public static void EnumerableValue()
    {
        Assert.Equal("[1, 2.3, \"Hello, world!\"]", ArgumentFormatter.Format(new object[] { 1, 2.3M, "Hello, world!" }));
    }

    [Fact]
    public static void ComplexTypeReturnsValuesInAlphabeticalOrder()
    {
        Assert.Equal("MyComplexType { MyPublicField = 42, MyPublicProperty = 21.12 }", ArgumentFormatter.Format(new MyComplexType()));
    }

    public class MyComplexType
    {
        private string MyPrivateField = "Hello, world";

        public static int MyPublicStaticField = 2112;

        public decimal MyPublicProperty { get; private set; }

        public int MyPublicField = 42;

        public MyComplexType()
        {
            MyPublicProperty = 21.12M;
        }
    }

    [Fact]
    public static void ComplexTypeInsideComplexType()
    {
        Assert.Equal("MyComplexTypeWrapper { c = 'A', s = \"Hello, world!\", t = MyComplexType { MyPublicField = 42, MyPublicProperty = 21.12 } }", ArgumentFormatter.Format(new MyComplexTypeWrapper()));
    }

    public class MyComplexTypeWrapper
    {
        public MyComplexType t = new MyComplexType();
        public char c = 'A';
        public string s = "Hello, world!";
    }

    [Fact]
    public static void EmptyComplexType()
    {
        Assert.Equal("Object { }",  ArgumentFormatter.Format(new object()));
    }

    [Fact]
    public static void ComplexTypeWithThrowingPropertyGetter()
    {
        Assert.Equal("ThrowingGetter { MyThrowingProperty = (throws NotImplementedException) }", ArgumentFormatter.Format(new ThrowingGetter()));
    }

    public class ThrowingGetter
    {
        public string MyThrowingProperty { get { throw new NotImplementedException(); } }
    }
}
