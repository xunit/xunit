using System;
using System.Globalization;
using System.Linq;
using Xunit;
using Xunit.Sdk;

public class ArgumentFormatterTests
{
    public class SimpleValues
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
        public static void StringValueTruncated()
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
            Assert.Equal(123.45M.ToString(CultureInfo.CurrentCulture), ArgumentFormatter.Format(123.45M));
        }

        [Fact]
        public static void DateTimeValue()
        {
            var now = DateTime.UtcNow;

            Assert.Equal<object>(now.ToString("o"), ArgumentFormatter.Format(now));
        }

        [Fact]
        public static void DateTimeOffsetValue()
        {
            var now = DateTimeOffset.UtcNow;

            Assert.Equal<object>(now.ToString("o"), ArgumentFormatter.Format(now));
        }

        [Fact]
        public static void TypeValue()
        {
            Assert.Equal("typeof(System.String)", ArgumentFormatter.Format(typeof(string)));
        }
    }

    public class Enumerables
    {
        [Fact]
        public static void EnumerableValue()
        {
            var expected = String.Format("[1, {0}, \"Hello, world!\"]", 2.3M.ToString(CultureInfo.CurrentCulture));

            Assert.Equal(expected, ArgumentFormatter.Format(new object[] { 1, 2.3M, "Hello, world!" }));
        }

        [Fact]
        public static void OnlyFirstFewValuesOfEnumerableAreRendered()
        {
            Assert.Equal("[0, 1, 2, 3, 4, ...]", ArgumentFormatter.Format(Enumerable.Range(0, Int32.MaxValue)));
        }

        [Fact]
        public static void EnumerablesAreRenderedWithMaximumDepthToPreventInfiniteRecursion()
        {
            object[] looping = new object[2];
            looping[0] = 42;
            looping[1] = looping;

            Assert.Equal("[42, [42, [...]]]", ArgumentFormatter.Format(looping));
        }
    }

    public class ComplexTypes
    {
        [Fact]
        public static void ReturnsValuesInAlphabeticalOrder()
        {
            var expected = String.Format("MyComplexType {{ MyPublicField = 42, MyPublicProperty = {0} }}", 21.12M.ToString(CultureInfo.CurrentCulture));

            Assert.Equal(expected, ArgumentFormatter.Format(new MyComplexType()));
        }

        public class MyComplexType
        {
#pragma warning disable 414
            private string MyPrivateField = "Hello, world";
#pragma warning restore 414

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
            var expected = String.Format("MyComplexTypeWrapper {{ c = 'A', s = \"Hello, world!\", t = MyComplexType {{ MyPublicField = 42, MyPublicProperty = {0} }} }}", 21.12M.ToString(CultureInfo.CurrentCulture));

            Assert.Equal(expected, ArgumentFormatter.Format(new MyComplexTypeWrapper()));
        }

        public class MyComplexTypeWrapper
        {
            public MyComplexType t = new MyComplexType();
            public char c = 'A';
            public string s = "Hello, world!";
        }

        [Fact]
        public static void Empty()
        {
            Assert.Equal("Object { }", ArgumentFormatter.Format(new object()));
        }

        [Fact]
        public static void WithThrowingPropertyGetter()
        {
            Assert.Equal("ThrowingGetter { MyThrowingProperty = (throws NotImplementedException) }", ArgumentFormatter.Format(new ThrowingGetter()));
        }

        public class ThrowingGetter
        {
            public string MyThrowingProperty { get { throw new NotImplementedException(); } }
        }

        [Fact]
        public static void LimitsOutputToFirstFewValues()
        {
            Assert.Equal(@"Big { MyField1 = 42, MyField2 = ""Hello, world!"", MyProp1 = 21.12, MyProp2 = typeof(ArgumentFormatterTests+ComplexTypes+Big), MyProp3 = 2014-04-17T07:45:23.0000000+00:00, ... }", ArgumentFormatter.Format(new Big()));
        }

        public class Big
        {
            public string MyField2 = "Hello, world!";

            public decimal MyProp1 { get; set; }

            public object MyProp4 { get; set; }

            public object MyProp3 { get; set; }

            public int MyField1 = 42;

            public Type MyProp2 { get; set; }

            public Big()
            {
                MyProp1 = 21.12M;
                MyProp2 = typeof(Big);
                MyProp3 = new DateTimeOffset(2014, 04, 17, 07, 45, 23, TimeSpan.Zero);
                MyProp4 = "Should not be shown";
            }
        }

        [Fact]
        public static void TypesAreRenderedWithMaximumDepthToPreventInfiniteRecursion()
        {
            Assert.Equal("Looping { Me = Looping { Me = Looping { ... } } }", ArgumentFormatter.Format(new Looping()));
        }

        public class Looping
        {
            public Looping Me;

            public Looping()
            {
                Me = this;
            }
        }
    }
}
