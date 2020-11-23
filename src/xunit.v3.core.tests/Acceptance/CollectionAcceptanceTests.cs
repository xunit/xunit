using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Abstractions;
using Xunit.v3;

public class CollectionAcceptanceTests : AcceptanceTestV3
{
	[Fact(Skip = "This depends on a type of message filtering we can't do any more, come re-write this soon")]
	public async void TwoClasses_OneInExplicitCollection_OneInDefaultCollection()
	{
		var results = await RunAsync(new[] { typeof(ClassInExplicitCollection), typeof(ClassInDefaultCollection) });

		var defaultResults = results.OfType<ITestCollectionMessage>().Where(message => message.TestCollection.DisplayName.StartsWith("Test collection for "));
		AssertMessageSequence(defaultResults, "CollectionAcceptanceTests+ClassInDefaultCollection.Passing");

		var explicitResults = results.OfType<ITestCollectionMessage>().Where(message => message.TestCollection.DisplayName == "Explicit Collection");
		AssertMessageSequence(explicitResults, "CollectionAcceptanceTests+ClassInExplicitCollection.Passing");
	}

	private void AssertMessageSequence(IEnumerable<IMessageSinkMessage> results, string testDisplayName)
	{
		Assert.Collection(
			results,
			message => Assert.IsType<_TestCollectionStarting>(message),
			message => Assert.IsType<_TestClassStarting>(message),
			message => Assert.IsAssignableFrom<_TestMethodStarting>(message),
			message => Assert.IsAssignableFrom<_TestCaseStarting>(message),
			message => Assert.IsAssignableFrom<_TestStarting>(message),
			message => Assert.IsAssignableFrom<ITestClassConstructionStarting>(message),
			message => Assert.IsAssignableFrom<ITestClassConstructionFinished>(message),
			message => Assert.IsAssignableFrom<IBeforeTestStarting>(message),
			message => Assert.IsAssignableFrom<IBeforeTestFinished>(message),
			message => Assert.IsAssignableFrom<IAfterTestStarting>(message),
			message => Assert.IsAssignableFrom<_AfterTestFinished>(message),
			message => Assert.IsAssignableFrom<_TestPassed>(message),
			message => Assert.IsAssignableFrom<_TestFinished>(message),
			message => Assert.IsAssignableFrom<_TestCaseFinished>(message),
			message => Assert.IsAssignableFrom<_TestMethodFinished>(message),
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
