using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Xunit;
using Xunit.Extensions;
using Xunit.Sdk;

public class TheoryAttributeTests
{
    [Fact]
    public void TestDataFromOleDb()
    {
        if (IntPtr.Size == 8)  // Test always fails in 64-bit; no JET engine
            return;

        string currentDirectory = Directory.GetCurrentDirectory();

        try
        {
            string executable = new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath;
            Directory.SetCurrentDirectory(Path.GetDirectoryName(executable));
            MethodInfo method = typeof(TestMethodCommandClass).GetMethod("TestViaOleDb");
            TheoryAttribute attr = (TheoryAttribute)(method.GetCustomAttributes(typeof(TheoryAttribute), false))[0];

            List<ITestCommand> commands = new List<ITestCommand>(attr.CreateTestCommands(Reflector.Wrap(method)));

            Assert.Equal(2, commands.Count);
            TheoryCommand command1 = Assert.IsType<TheoryCommand>(commands[0]);
            Assert.Equal(3, command1.Parameters.Length);
            Assert.Equal<object>(1D, command1.Parameters[0]);
            Assert.Equal<object>("Foo", command1.Parameters[1]);
            Assert.Equal<object>("Bar", command1.Parameters[2]);
            TheoryCommand command2 = Assert.IsType<TheoryCommand>(commands[1]);
            Assert.Equal(3, command2.Parameters.Length);
            Assert.Equal<object>(14D, command2.Parameters[0]);
            Assert.Equal<object>("Biff", command2.Parameters[1]);
            Assert.Equal<object>("Baz", command2.Parameters[2]);
        }
        finally
        {
            Directory.SetCurrentDirectory(currentDirectory);
        }
    }

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
    public void TestDataFromXls()
    {
        if (IntPtr.Size == 8)  // Test always fails in 64-bit; no JET engine
            return;

        MethodInfo method = typeof(TestMethodCommandClass).GetMethod("TestViaXls");
        TheoryAttribute attr = (TheoryAttribute)(method.GetCustomAttributes(typeof(TheoryAttribute), false))[0];

        List<ITestCommand> commands = new List<ITestCommand>(attr.CreateTestCommands(Reflector.Wrap(method)));

        Assert.Equal(2, commands.Count);
        TheoryCommand command1 = Assert.IsType<TheoryCommand>(commands[0]);
        Assert.Equal(3, command1.Parameters.Length);
        Assert.Equal<object>(1D, command1.Parameters[0]);
        Assert.Equal<object>("Foo", command1.Parameters[1]);
        Assert.Equal<object>("Bar", command1.Parameters[2]);
        TheoryCommand command2 = Assert.IsType<TheoryCommand>(commands[1]);
        Assert.Equal(3, command2.Parameters.Length);
        Assert.Equal<object>(14D, command2.Parameters[0]);
        Assert.Equal<object>("Biff", command2.Parameters[1]);
        Assert.Equal<object>("Baz", command2.Parameters[2]);
    }

    [Fact]
    public void ResolvedGenericTypeIsIncludedInDisplayName()
    {
        MethodInfo method = typeof(TestMethodCommandClass).GetMethod("GenericTest");
        TheoryAttribute attr = (TheoryAttribute)(method.GetCustomAttributes(typeof(TheoryAttribute), false))[0];

        List<ITestCommand> commands = new List<ITestCommand>(attr.CreateTestCommands(Reflector.Wrap(method)));

        Assert.Equal(4, commands.Count);
        TheoryCommand command1 = Assert.IsType<TheoryCommand>(commands[0]);
        Assert.Equal(@"TheoryAttributeTests+TestMethodCommandClass.GenericTest<Int32>(value: 42)", command1.DisplayName);
        TheoryCommand command2 = Assert.IsType<TheoryCommand>(commands[1]);
        Assert.Equal(@"TheoryAttributeTests+TestMethodCommandClass.GenericTest<String>(value: ""Hello, world!"")", command2.DisplayName);
        TheoryCommand command3 = Assert.IsType<TheoryCommand>(commands[2]);
        Assert.Equal(@"TheoryAttributeTests+TestMethodCommandClass.GenericTest<Int32[]>(value: System.Int32[])", command3.DisplayName);
        // TODO: Would like to see @"TheoryAttributeTests+TestMethodCommandClass.GenericTest<Int32[]>(value: Int32[] { 1, 2, 3 })"
        TheoryCommand command4 = Assert.IsType<TheoryCommand>(commands[3]);
        Assert.Equal(@"TheoryAttributeTests+TestMethodCommandClass.GenericTest<List<String>>(value: System.Collections.Generic.List`1[System.String])", command4.DisplayName);
        // TODO: Would like to see @"TheoryAttributeTests+TestMethodCommandClass.GenericTest<List<String>>(value: List<String> { ""a"", ""b"", ""c"" })"
    }

    internal class TestMethodCommandClass
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

        [Theory, OleDbData(
            @"Provider=Microsoft.Jet.OleDb.4.0; Data Source=DataTheories\UnitTestData.xls; Extended Properties=Excel 8.0",
            "SELECT x, y, z FROM Data")]
        public void TestViaOleDb(double x, string y, string z) { }

        [Theory, PropertyData("TheoryDataProperty")]
        public void TestViaProperty(int x) { }

        [Theory, PropertyData("TheoryDataProperty", PropertyType = typeof(ExternalData))]
        public void TestViaOtherTypeProperty(int x) { }

        [Theory, ExcelData(@"DataTheories\UnitTestData.xls", "SELECT x, y, z FROM Data")]
        public void TestViaXls(double x, string y, string z) { }

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