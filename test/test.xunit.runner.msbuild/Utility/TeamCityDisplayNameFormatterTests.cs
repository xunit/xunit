using System.Reflection;
using NSubstitute;
using Xunit.Abstractions;

namespace Xunit.Runner.MSBuild.Visitors
{
    public class TeamCityDisplayNameFormatterTests
    {
        public TeamCityDisplayNameFormatter ClassUnderTest = new TeamCityDisplayNameFormatter();

        public class DisplayName_Test : TeamCityDisplayNameFormatterTests
        {
            [Fact]
            public void UsesDisplayName()
            {
                var test = Substitute.For<ITest>();
                test.DisplayName.Returns("Display Name");

                var result = ClassUnderTest.DisplayName(test);

                Assert.Equal("Display Name", result);
            }
        }

        public class DisplayName_TestCollection : TeamCityDisplayNameFormatterTests
        {
            [Fact]
            public void UsesAssemblyIndex()
            {
                var firstCollection = Mocks.TestCollection(Assembly.GetExecutingAssembly(), displayName: "Display Name");
                var secondCollection = Mocks.TestCollection(typeof(int).Assembly, displayName: "Display Name");

                var firstResult = ClassUnderTest.DisplayName(firstCollection);
                var secondResult = ClassUnderTest.DisplayName(secondCollection);

                Assert.Equal("Display Name (1)", firstResult);
                Assert.Equal("Display Name (2)", secondResult);
            }
        }
    }
}
