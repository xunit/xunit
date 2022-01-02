using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Sdk;
using Xunit.v3;

[assembly: XunitTestCaseRunnerBaseTests.BeforeAfterOnAssembly]

public class XunitTestCaseRunnerBaseTests
{
	[Fact]
	public static void BeforeAfterTestAttributesComeFromTestCollectionAndTestClassAndTestMethod()
	{
		var collection = Mocks.TestCollection(definition: Reflector.Wrap(typeof(BeforeAfterCollection)));
		var testCase = TestData.XunitTestCase<ClassUnderTest>("Passing", collection);
		var runner = new TestableXunitTestCaseRunnerBase();

		var result = runner.GetBeforeAfterTestAttributes(testCase);

		Assert.Collection(
			result.OrderBy(a => a.GetType().Name),
			attr => Assert.IsType<BeforeAfterOnAssembly>(attr),
			attr => Assert.IsType<BeforeAfterOnClass>(attr),
			attr => Assert.IsType<BeforeAfterOnCollection>(attr),
			attr => Assert.IsType<BeforeAfterOnMethod>(attr)
		);
	}

	[BeforeAfterOnCollection]
	class BeforeAfterCollection { }

	[BeforeAfterOnClass]
	class ClassUnderTest
	{
		[Fact]
		[BeforeAfterOnMethod]
		public void Passing() { }
	}

	class BeforeAfterOnCollection : BeforeAfterTestAttribute { }
	class BeforeAfterOnClass : BeforeAfterTestAttribute { }
	class BeforeAfterOnMethod : BeforeAfterTestAttribute { }
	public class BeforeAfterOnAssembly : BeforeAfterTestAttribute { }

	class TestableXunitTestCaseRunnerBase : XunitTestCaseRunnerBase<XunitTestCaseRunnerContext>
	{
		public IReadOnlyCollection<BeforeAfterTestAttribute> GetBeforeAfterTestAttributes(IXunitTestCase testCase)
		{
			var testMethodArguments = new object?[0];
			var result = Initialize(testCase, ref testMethodArguments);
			return result.BeforeAfterTestAttributes;
		}
	}
}
