using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit;
using Xunit.Sdk;

namespace Xunit1
{
    public class FactAttributeTests
    {
        [Fact]
        public void FactAttributeGeneratesFactCommand()
        {
            MethodInfo method = typeof(FactAttributeTests).GetMethod("DummyFactMethod");
            FactAttribute attribute = new FactAttribute();

            List<ITestCommand> results = new List<ITestCommand>(attribute.CreateTestCommands(Reflector.Wrap(method)));

            ITestCommand result = Assert.Single(results);
            Assert.IsType<FactCommand>(result);
        }

        [Fact]
        public void DefaultFactAttributeValues()
        {
            FactAttribute attrib = new FactAttribute();
            MethodInfo method = typeof(FactAttributeTests).GetMethod("DummyFactMethod");

            var commands = new List<ITestCommand>(attrib.CreateTestCommands(Reflector.Wrap(method)));

            ITestCommand command = Assert.Single(commands);
            FactCommand factCommand = Assert.IsType<FactCommand>(command);
            Assert.Equal("Xunit1.FactAttributeTests", factCommand.TypeName);
            Assert.Equal("DummyFactMethod", factCommand.MethodName);
            Assert.Equal("Xunit1.FactAttributeTests.DummyFactMethod", factCommand.DisplayName);
        }

        [Fact]
        public void NameOnFactAttributeOverridesDisplayName()
        {
            MethodInfo method = typeof(FactAttributeTests).GetMethod("CustomNamedFactMethod");
            FactAttribute attrib = method.GetCustomAttributes(true).OfType<FactAttribute>().Single();

            var commands = new List<ITestCommand>(attrib.CreateTestCommands(Reflector.Wrap(method)));

            ITestCommand command = Assert.Single(commands);
            FactCommand factCommand = Assert.IsType<FactCommand>(command);
            Assert.Equal("Custom display name", factCommand.DisplayName);
        }

        [Fact]
        public void SkipCanBeOverridenInDerivedAttribute()
        {
            MyFactAttribute attrib = new MyFactAttribute(7);

            string result = attrib.Skip;

            Assert.Equal("35", result);
        }

        class MyFactAttribute : FactAttribute
        {
            int value;

            public MyFactAttribute(int value)
            {
                this.value = value;
            }

            public override string Skip
            {
                get { return (value * 5).ToString(); }
                set { base.Skip = value; }
            }
        }

        public void DummyFactMethod() { }

        [Fact(DisplayName = "Custom display name")]
        public void CustomNamedFactMethod() { }
    }
}
