using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

public class CollectionAcceptanceTests : AcceptanceTest
{
    [Fact]
    public void TwoClasses_OneInExplicitCollection_OneInDefaultCollection()
    {
        var results = Run(new[] { typeof(ClassInExplicitCollection), typeof(ClassInDefaultCollection) });

        var defaultIndex = results.FindIndex(message => message is ITestCollectionStarting && ((ITestCollectionStarting)message).TestCollection.DisplayName.StartsWith("Test collection for "));
        Assert.NotEqual(-1, defaultIndex);
        AssertMessageSequence(results, defaultIndex, "CollectionAcceptanceTests+ClassInDefaultCollection.Passing");

        var explicitIndex = results.FindIndex(message => message is ITestCollectionStarting && ((ITestCollectionStarting)message).TestCollection.DisplayName == "Explicit Collection");
        Assert.NotEqual(-1, explicitIndex);
        AssertMessageSequence(results, explicitIndex, "CollectionAcceptanceTests+ClassInExplicitCollection.Passing");
    }

    private void AssertMessageSequence(List<ITestMessage> results, int defaultIndex, string testDisplayName)
    {
        Assert.Collection(results.Skip(defaultIndex).Take(13),
            message => Assert.IsAssignableFrom<ITestCollectionStarting>(message),
            message => Assert.IsAssignableFrom<ITestClassStarting>(message),
            message => Assert.IsAssignableFrom<ITestMethodStarting>(message),
            message => Assert.IsAssignableFrom<ITestCaseStarting>(message),
            message => Assert.IsAssignableFrom<ITestStarting>(message),
            message => Assert.IsAssignableFrom<ITestClassConstructionStarting>(message),
            message => Assert.IsAssignableFrom<ITestClassConstructionFinished>(message),
            message =>
            {
                var passed = Assert.IsAssignableFrom<ITestPassed>(message);
                Assert.Equal(testDisplayName, passed.TestDisplayName);
            },
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
