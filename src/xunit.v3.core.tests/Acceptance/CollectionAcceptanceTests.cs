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

		var defaultCollectionStarting = Assert.Single(results.OfType<TestCollectionStarting>().Where(x => x.TestCollectionDisplayName.StartsWith("Test collection for ")));
		var defaultResults = results.OfType<TestCollectionMessage>().Where(x => x.TestCollectionUniqueID == defaultCollectionStarting.TestCollectionUniqueID);
		AssertMessageSequence(defaultResults, "CollectionAcceptanceTests+ClassInDefaultCollection.Passing");

		var explicitCollectionStarting = Assert.Single(results.OfType<TestCollectionStarting>().Where(x => x.TestCollectionDisplayName == "Explicit Collection"));
		var explicitResults = results.OfType<TestCollectionMessage>().Where(x => x.TestCollectionUniqueID == explicitCollectionStarting.TestCollectionUniqueID);
		AssertMessageSequence(explicitResults, "CollectionAcceptanceTests+ClassInExplicitCollection.Passing");
	}

	private void AssertMessageSequence(IEnumerable<MessageSinkMessage> results, string testDisplayName)
	{
		Assert.Collection(
			results,
			message => Assert.IsType<TestCollectionStarting>(message),
			message => Assert.IsType<TestClassStarting>(message),
			message => Assert.IsType<TestMethodStarting>(message),
			message => Assert.IsType<TestCaseStarting>(message),
			message =>
			{
				var testStarting = Assert.IsType<TestStarting>(message);
				Assert.Equal(testDisplayName, testStarting.TestDisplayName);
			},
			message => Assert.IsType<TestClassConstructionStarting>(message),
			message => Assert.IsType<TestClassConstructionFinished>(message),
			message => Assert.IsType<TestPassed>(message),
			message => Assert.IsType<TestFinished>(message),
			message => Assert.IsType<TestCaseFinished>(message),
			message => Assert.IsType<TestMethodFinished>(message),
			message => Assert.IsType<TestClassFinished>(message),
			message => Assert.IsType<TestCollectionFinished>(message)
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
