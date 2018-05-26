#if NETFRAMEWORK

using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

public class CollectionAcceptanceTests : AcceptanceTestV2
{
    [Fact]
    public void TwoClasses_OneInExplicitCollection_OneInDefaultCollection()
    {
        var results = Run(new[] { typeof(ClassInExplicitCollection), typeof(ClassInDefaultCollection) });

        var defaultResults = results.OfType<ITestCollectionMessage>().Where(message => message.TestCollection.DisplayName.StartsWith("Test collection for "));
        AssertMessageSequence(defaultResults, "CollectionAcceptanceTests+ClassInDefaultCollection.Passing");

        var explicitResults = results.OfType<ITestCollectionMessage>().Where(message => message.TestCollection.DisplayName == "Explicit Collection");
        AssertMessageSequence(explicitResults, "CollectionAcceptanceTests+ClassInExplicitCollection.Passing");
    }

    private void AssertMessageSequence(IEnumerable<IMessageSinkMessage> results, string testDisplayName)
    {
        Assert.Collection(results,
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
                Assert.Equal(testDisplayName, passed.Test.DisplayName);
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

#endif
