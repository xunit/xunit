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

	static void AssertMessageSequence(IEnumerable<IMessageSinkMessage> results, string testDisplayName)
	{
		Assert.Collection(
			results,
			message => Assert.IsType<ITestCollectionStarting>(message, exactMatch: false),
			message => Assert.IsType<ITestClassStarting>(message, exactMatch: false),
			message => Assert.IsType<ITestMethodStarting>(message, exactMatch: false),
			message => Assert.IsType<ITestCaseStarting>(message, exactMatch: false),
			message =>
			{
				var testStarting = Assert.IsType<ITestStarting>(message, exactMatch: false);
				Assert.Equal(testDisplayName, testStarting.TestDisplayName);
			},
			message => Assert.IsType<ITestClassConstructionStarting>(message, exactMatch: false),
			message => Assert.IsType<ITestClassConstructionFinished>(message, exactMatch: false),
			message => Assert.IsType<ITestPassed>(message, exactMatch: false),
			message => Assert.IsType<ITestFinished>(message, exactMatch: false),
			message => Assert.IsType<ITestCaseFinished>(message, exactMatch: false),
			message => Assert.IsType<ITestMethodFinished>(message, exactMatch: false),
			message => Assert.IsType<ITestClassFinished>(message, exactMatch: false),
			message => Assert.IsType<ITestCollectionFinished>(message, exactMatch: false)
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
