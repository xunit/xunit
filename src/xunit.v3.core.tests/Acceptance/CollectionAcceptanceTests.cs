using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Sdk;

public class CollectionAcceptanceTests : AcceptanceTestV3
{
	[Fact]
	public async ValueTask TwoClasses_OneInExplicitCollection_OneInDefaultCollection()
	{
		var results = await RunAsync([typeof(ClassInExplicitCollection), typeof(ClassInDefaultCollection)]);

		var defaultCollectionStarting = Assert.Single(results.OfType<ITestCollectionStarting>(), x => x.TestCollectionDisplayName.StartsWith("Test collection for "));
		var defaultResults = results.OfType<ITestCollectionMessage>().Where(x => x.TestCollectionUniqueID == defaultCollectionStarting.TestCollectionUniqueID);
		AssertMessageSequence(defaultResults, "CollectionAcceptanceTests+ClassInDefaultCollection.Passing");

		var explicitCollectionStarting = Assert.Single(results.OfType<ITestCollectionStarting>(), x => x.TestCollectionDisplayName == "Explicit Collection");
		var explicitResults = results.OfType<ITestCollectionMessage>().Where(x => x.TestCollectionUniqueID == explicitCollectionStarting.TestCollectionUniqueID);
		AssertMessageSequence(explicitResults, "CollectionAcceptanceTests+ClassInExplicitCollection.Passing");
	}

	private void AssertMessageSequence(IEnumerable<IMessageSinkMessage> results, string testDisplayName)
	{
		Assert.Collection(
			results,
			message => Assert.IsAssignableFrom<ITestCollectionStarting>(message),
			message => Assert.IsAssignableFrom<ITestClassStarting>(message),
			message => Assert.IsAssignableFrom<ITestMethodStarting>(message),
			message => Assert.IsAssignableFrom<ITestCaseStarting>(message),
			message =>
			{
				var testStarting = Assert.IsAssignableFrom<ITestStarting>(message);
				Assert.Equal(testDisplayName, testStarting.TestDisplayName);
			},
			message => Assert.IsAssignableFrom<ITestClassConstructionStarting>(message),
			message => Assert.IsAssignableFrom<ITestClassConstructionFinished>(message),
			message => Assert.IsAssignableFrom<ITestPassed>(message),
			message => Assert.IsAssignableFrom<ITestFinished>(message),
			message => Assert.IsAssignableFrom<ITestCaseFinished>(message),
			message => Assert.IsAssignableFrom<ITestMethodFinished>(message),
			message => Assert.IsAssignableFrom<ITestClassFinished>(message),
			message => Assert.IsAssignableFrom<ITestCollectionFinished>(message)
		);
	}

	[Collection("Explicit Collection")]
	class ClassInExplicitCollection
	{
		[Fact]
		public void Passing() { }
	}

	class ClassInDefaultCollection
	{
		[Fact]
		public void Passing() { }
	}
}
