using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.v3;

public class CollectionAcceptanceTests : AcceptanceTestV3
{
	[Fact]
	public async ValueTask TwoClasses_OneInExplicitCollection_OneInDefaultCollection()
	{
		var results = await RunAsync(new[] { typeof(ClassInExplicitCollection), typeof(ClassInDefaultCollection) });

		var defaultCollectionStarting = Assert.Single(results.OfType<_TestCollectionStarting>().Where(x => x.TestCollectionDisplayName.StartsWith("Test collection for ")));
		var defaultResults = results.OfType<_TestCollectionMessage>().Where(x => x.TestCollectionUniqueID == defaultCollectionStarting.TestCollectionUniqueID);
		AssertMessageSequence(defaultResults, "CollectionAcceptanceTests+ClassInDefaultCollection.Passing");

		var explicitCollectionStarting = Assert.Single(results.OfType<_TestCollectionStarting>().Where(x => x.TestCollectionDisplayName == "Explicit Collection"));
		var explicitResults = results.OfType<_TestCollectionMessage>().Where(x => x.TestCollectionUniqueID == explicitCollectionStarting.TestCollectionUniqueID);
		AssertMessageSequence(explicitResults, "CollectionAcceptanceTests+ClassInExplicitCollection.Passing");
	}

	private void AssertMessageSequence(IEnumerable<_MessageSinkMessage> results, string testDisplayName)
	{
		Assert.Collection(
			results,
			message => Assert.IsType<_TestCollectionStarting>(message),
			message => Assert.IsType<_TestClassStarting>(message),
			message => Assert.IsType<_TestMethodStarting>(message),
			message => Assert.IsType<_TestCaseStarting>(message),
			message =>
			{
				var testStarting = Assert.IsType<_TestStarting>(message);
				Assert.Equal(testDisplayName, testStarting.TestDisplayName);
			},
			message => Assert.IsType<_TestClassConstructionStarting>(message),
			message => Assert.IsType<_TestClassConstructionFinished>(message),
			message => Assert.IsType<_BeforeTestStarting>(message),
			message => Assert.IsType<_BeforeTestFinished>(message),
			message => Assert.IsType<_AfterTestStarting>(message),
			message => Assert.IsType<_AfterTestFinished>(message),
			message => Assert.IsType<_TestPassed>(message),
			message => Assert.IsType<_TestFinished>(message),
			message => Assert.IsType<_TestCaseFinished>(message),
			message => Assert.IsType<_TestMethodFinished>(message),
			message => Assert.IsType<_TestClassFinished>(message),
			message => Assert.IsType<_TestCollectionFinished>(message)
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
