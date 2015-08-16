using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Sdk;

public class ArgumentFormatterTests
{
    public class SimpleValues
    {
        [CulturedFact]
        public static void NullValue()
        {
            Assert.Equal("null", ArgumentFormatter.Format(null));
        }

        [CulturedFact]
        public static void StringValue()
        {
            Assert.Equal("\"Hello, world!\"", ArgumentFormatter.Format("Hello, world!"));
        }

        [CulturedFact]
        public static void StringValueTruncated()
        {
            Assert.Equal("\"----|----1----|----2----|----3----|----4----|----5\"...", ArgumentFormatter.Format("----|----1----|----2----|----3----|----4----|----5-"));
        }

        [Theory]
        // Printable
        [InlineData(' ', "' '")]
        [InlineData('a', "'a'")]
        [InlineData('1', "'1'")]
        [InlineData('!', "'!'")]
        [InlineData('©', "'©'")]
        [InlineData('╬', "'╬'")]
        [InlineData('ئ', "'ئ'")]
        // Unprintable
        [InlineData(char.MinValue, "0x0000")]
        [InlineData(char.MaxValue, "0xffff")]
        public static void CharacterValue(char value, string expected)
        {
            Assert.Equal(expected, ArgumentFormatter.Format(value));
        }

        [CulturedFact]
        public static void DecimalValue()
        {
            Assert.Equal(123.45M.ToString(), ArgumentFormatter.Format(123.45M));
        }

        [CulturedFact]
        public static void DateTimeValue()
        {
            var now = DateTime.UtcNow;

            Assert.Equal(now.ToString("o"), ArgumentFormatter.Format(now));
        }

        [CulturedFact]
        public static void DateTimeOffsetValue()
        {
            var now = DateTimeOffset.UtcNow;

            Assert.Equal(now.ToString("o"), ArgumentFormatter.Format(now));
        }

        [Fact]
        public static async void TaskValue()
        {
            var task = Task.Run(() => { });
            await task;

            Assert.Equal("Task { Status = RanToCompletion }", ArgumentFormatter.Format(task));
        }

        [Fact]
        public static void TaskGenericValue()
        {
            var taskCompletionSource = new TaskCompletionSource<int>();
            taskCompletionSource.SetException(new DivideByZeroException());

            Assert.Equal("Task<int> { Status = Faulted }", ArgumentFormatter.Format(taskCompletionSource.Task));
        }

        [Theory]
        [InlineData(typeof(string), "typeof(string)")]
        [InlineData(typeof(int[]), "typeof(int[])")]
        [InlineData(typeof(DateTime[,]), "typeof(System.DateTime[,])")]
        [InlineData(typeof(decimal[][,]), "typeof(decimal[][,])")]
        [InlineData(typeof(IEnumerable<>), "typeof(System.Collections.Generic.IEnumerable<>)")]
        [InlineData(typeof(IEnumerable<int>), "typeof(System.Collections.Generic.IEnumerable<int>)")]
        [InlineData(typeof(IDictionary<,>), "typeof(System.Collections.Generic.IDictionary<,>)")]
        [InlineData(typeof(IDictionary<string, DateTime>), "typeof(System.Collections.Generic.IDictionary<string, System.DateTime>)")]
        [InlineData(typeof(IDictionary<string[,], DateTime[,][]>), "typeof(System.Collections.Generic.IDictionary<string[,], System.DateTime[,][]>)")]
        [InlineData(typeof(bool?), "typeof(bool?)")]
        [InlineData(typeof(bool?[]), "typeof(bool?[])")]
        [InlineData(typeof(Uri), "typeof(System.Uri)")]
        public static void TypeValue(Type type, string expected)
        {
            Assert.Equal(expected, ArgumentFormatter.Format(type));
        }
    }

    public class Enumerables
    {
        [CulturedFact]
        public static void EnumerableValue()
        {
            var expected = $"[1, {2.3M}, \"Hello, world!\"]";

            Assert.Equal(expected, ArgumentFormatter.Format(new object[] { 1, 2.3M, "Hello, world!" }));
        }

        [CulturedFact]
        public static void OnlyFirstFewValuesOfEnumerableAreRendered()
        {
            Assert.Equal("[0, 1, 2, 3, 4, ...]", ArgumentFormatter.Format(Enumerable.Range(0, int.MaxValue)));
        }

        [CulturedFact]
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
        [CulturedFact]
        public static void ReturnsValuesInAlphabeticalOrder()
        {
            var expected = $"MyComplexType {{ MyPublicField = 42, MyPublicProperty = {21.12M} }}";

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

        [CulturedFact]
        public static void ComplexTypeInsideComplexType()
        {
            var expected = $"MyComplexTypeWrapper {{ c = 'A', s = \"Hello, world!\", t = MyComplexType {{ MyPublicField = 42, MyPublicProperty = {21.12M} }} }}";

            Assert.Equal(expected, ArgumentFormatter.Format(new MyComplexTypeWrapper()));
        }

        public class MyComplexTypeWrapper
        {
            public MyComplexType t = new MyComplexType();
            public char c = 'A';
            public string s = "Hello, world!";
        }

        [CulturedFact]
        public static void Empty()
        {
            Assert.Equal("Object { }", ArgumentFormatter.Format(new object()));
        }

        [CulturedFact]
        public static void WithThrowingPropertyGetter()
        {
            Assert.Equal("ThrowingGetter { MyThrowingProperty = (throws NotImplementedException) }", ArgumentFormatter.Format(new ThrowingGetter()));
        }

        public class ThrowingGetter
        {
            public string MyThrowingProperty { get { throw new NotImplementedException(); } }
        }

        [CulturedFact]
        public static void LimitsOutputToFirstFewValues()
        {
            var expected = $@"Big {{ MyField1 = 42, MyField2 = ""Hello, world!"", MyProp1 = {21.12}, MyProp2 = typeof(ArgumentFormatterTests+ComplexTypes+Big), MyProp3 = 2014-04-17T07:45:23.0000000+00:00, ... }}";

            Assert.Equal(expected, ArgumentFormatter.Format(new Big()));
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

        [CulturedFact]
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

        [Fact]
        public static void WhenCustomTypeImplementsToString_UsesToString()
        {
            Assert.Equal("This is what you should show", ArgumentFormatter.Format(new TypeWithToString()));
        }

        public class TypeWithToString
        {
            public override string ToString()
            {
                return "This is what you should show";
            }
        }
    }
}
