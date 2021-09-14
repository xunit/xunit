using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Xunit;
using Xunit.Extensions;
using Xunit.Sdk;

namespace Xunit1.Extensions
{
    public class TheoryAttributeTests
    {
        [Fact]
        public void TestDataFromProperty()
        {
            MethodInfo method = typeof(TestMethodCommandClass).GetMethod("TestViaProperty");
            TheoryAttribute attr = (TheoryAttribute)(method.GetCustomAttributes(typeof(TheoryAttribute), false))[0];

            List<ITestCommand> commands = new List<ITestCommand>(attr.CreateTestCommands(Reflector.Wrap(method)));

            ITestCommand command = Assert.Single(commands);
            TheoryCommand theoryCommand = Assert.IsType<TheoryCommand>(command);
            object parameter = Assert.Single(theoryCommand.Parameters);
            Assert.Equal(2, parameter);
        }

        [Fact]
        public void TestDataFromPropertyOnBaseClass()
        {
            MethodInfo method = typeof(TestMethodCommandClass).GetMethod("TestViaPropertyOnBaseClass");
            TheoryAttribute attr = (TheoryAttribute)(method.GetCustomAttributes(typeof(TheoryAttribute), false))[0];

            List<ITestCommand> commands = new List<ITestCommand>(attr.CreateTestCommands(Reflector.Wrap(method)));

            ITestCommand command = Assert.Single(commands);
            TheoryCommand theoryCommand = Assert.IsType<TheoryCommand>(command);
            object parameter = Assert.Single(theoryCommand.Parameters);
            Assert.Equal(4, parameter);
        }

        [Fact]
        public void TestDataFromOtherTypeProperty()
        {
            MethodInfo method = typeof(TestMethodCommandClass).GetMethod("TestViaOtherTypeProperty");
            TheoryAttribute attr = (TheoryAttribute)(method.GetCustomAttributes(typeof(TheoryAttribute), false))[0];

            List<ITestCommand> commands = new List<ITestCommand>(attr.CreateTestCommands(Reflector.Wrap(method)));

            ITestCommand command = Assert.Single(commands);
            TheoryCommand theoryCommand = Assert.IsType<TheoryCommand>(command);
            object parameter = Assert.Single(theoryCommand.Parameters);
            Assert.Equal(3, parameter);
        }

        [Fact]
        public void ResolvedGenericTypeIsIncludedInDisplayName()
        {
            MethodInfo method = typeof(TestMethodCommandClass).GetMethod("GenericTest");
            TheoryAttribute attr = (TheoryAttribute)(method.GetCustomAttributes(typeof(TheoryAttribute), false))[0];

            List<ITestCommand> commands = new List<ITestCommand>(attr.CreateTestCommands(Reflector.Wrap(method)));

            Assert.Equal(4, commands.Count);
            TheoryCommand command1 = Assert.IsType<TheoryCommand>(commands[0]);
            Assert.Equal(@"Xunit1.Extensions.TheoryAttributeTests+TestMethodCommandClass.GenericTest<Int32>(value: 42)", command1.DisplayName);
            TheoryCommand command2 = Assert.IsType<TheoryCommand>(commands[1]);
            Assert.Equal(@"Xunit1.Extensions.TheoryAttributeTests+TestMethodCommandClass.GenericTest<String>(value: ""Hello, world!"")", command2.DisplayName);
            TheoryCommand command3 = Assert.IsType<TheoryCommand>(commands[2]);
            Assert.Equal(@"Xunit1.Extensions.TheoryAttributeTests+TestMethodCommandClass.GenericTest<Int32[]>(value: System.Int32[])", command3.DisplayName);
            // TODO: Would like to see @"TheoryAttributeTests+TestMethodCommandClass.GenericTest<Int32[]>(value: Int32[] { 1, 2, 3 })"
            TheoryCommand command4 = Assert.IsType<TheoryCommand>(commands[3]);
            Assert.Equal(@"Xunit1.Extensions.TheoryAttributeTests+TestMethodCommandClass.GenericTest<List<String>>(value: System.Collections.Generic.List`1[System.String])", command4.DisplayName);
            // TODO: Would like to see @"TheoryAttributeTests+TestMethodCommandClass.GenericTest<List<String>>(value: List<String> { ""a"", ""b"", ""c"" })"
        }

        internal class TestMethodCommandClassBaseClass
        {
            public static IEnumerable<object[]> BasePropertyData
            {
                get { yield return new object[] { 4 }; }
            }
        }

        internal class TestMethodCommandClass : TestMethodCommandClassBaseClass
        {
            public static IEnumerable<object[]> GenericData
            {
                get
                {
                    yield return new object[] { 42 };
                    yield return new object[] { "Hello, world!" };
                    yield return new object[] { new int[] { 1, 2, 3 } };
                    yield return new object[] { new List<string> { "a", "b", "c" } };
                }
            }

            public static IEnumerable<object[]> TheoryDataProperty
            {
                get { yield return new object[] { 2 }; }
            }

            [Theory, PropertyData("TheoryDataProperty")]
            public void TestViaProperty(int x) { }

            [Theory, PropertyData("BasePropertyData")]
            public void TestViaPropertyOnBaseClass(int x) { }

            [Theory, PropertyData("TheoryDataProperty", PropertyType = typeof(ExternalData))]
            public void TestViaOtherTypeProperty(int x) { }

            [Theory, PropertyData("GenericData")]
            public void GenericTest<T>(T value) { }

            class ExternalData
            {
                public static IEnumerable<object[]> TheoryDataProperty
                {
                    get { yield return new object[] { 3 }; }
                }
            }
        }
    }
}
