using Xunit;
using Xunit.Runner.Common;

public class MessageMetadataCacheTests
{
	[Fact]
	public void AssemblyTest()
	{
		var starting = TestData.TestAssemblyStarting();
		var failure = TestData.TestAssemblyCleanupFailure();
		var finished = TestData.TestAssemblyFinished();
		var cache = new MessageMetadataCache();

		// Starts empty
		Assert.Null(cache.TryGetAssemblyMetadata(failure));

		// Set the cache, should be able to retrieve it
		cache.Set(starting);
		Assert.Same(starting, cache.TryGetAssemblyMetadata(failure));
		Assert.Same(starting, cache.TryGetAssemblyMetadata(failure.AssemblyUniqueID, remove: false));
		Assert.Same(starting, cache.TryGetAssemblyMetadata(failure.AssemblyUniqueID, remove: true));

		// After removal, we're empty again
		Assert.Null(cache.TryGetAssemblyMetadata(failure));

		// Now set and use the explicit finish removal
		cache.Set(starting);
		Assert.Same(starting, cache.TryRemove(finished));

		// Finish empty
		Assert.Null(cache.TryGetAssemblyMetadata(failure));
	}

	[Fact]
	public void TestCaseTest()
	{
		var starting = TestData.TestCaseStarting();
		var failure = TestData.TestCaseCleanupFailure();
		var finished = TestData.TestCaseFinished();
		var cache = new MessageMetadataCache();

		// Starts empty
		Assert.Null(cache.TryGetTestCaseMetadata(failure));

		// Set the cache, should be able to retrieve it
		cache.Set(starting);
		Assert.Same(starting, cache.TryGetTestCaseMetadata(failure));
		Assert.Same(starting, cache.TryGetTestCaseMetadata(failure.TestCaseUniqueID, remove: false));
		Assert.Same(starting, cache.TryGetTestCaseMetadata(failure.TestCaseUniqueID, remove: true));

		// After removal, we're empty again
		Assert.Null(cache.TryGetTestCaseMetadata(failure));

		// Now set and use the explicit finish removal
		cache.Set(starting);
		Assert.Same(starting, cache.TryRemove(finished));

		// Finish empty
		Assert.Null(cache.TryGetTestCaseMetadata(failure));
	}

	[Fact]
	public void TestClassTest()
	{
		var starting = TestData.TestClassStarting();
		var failure = TestData.TestClassCleanupFailure();
		var finished = TestData.TestClassFinished();
		var cache = new MessageMetadataCache();

		// Starts empty
		Assert.Null(cache.TryGetClassMetadata(failure));

		// Set the cache, should be able to retrieve it
		cache.Set(starting);
		Assert.Same(starting, cache.TryGetClassMetadata(failure));
		Assert.Same(starting, cache.TryGetClassMetadata(failure.TestClassUniqueID!, remove: false));
		Assert.Same(starting, cache.TryGetClassMetadata(failure.TestClassUniqueID!, remove: true));

		// After removal, we're empty again
		Assert.Null(cache.TryGetClassMetadata(failure));

		// Now set and use the explicit finish removal
		cache.Set(starting);
		Assert.Same(starting, cache.TryRemove(finished));

		// Finish empty
		Assert.Null(cache.TryGetClassMetadata(failure));
	}

	[Fact]
	public void TestCollectionTest()
	{
		var starting = TestData.TestCollectionStarting();
		var failure = TestData.TestCollectionCleanupFailure();
		var finished = TestData.TestCollectionFinished();
		var cache = new MessageMetadataCache();

		// Starts empty
		Assert.Null(cache.TryGetCollectionMetadata(failure));

		// Set the cache, should be able to retrieve it
		cache.Set(starting);
		Assert.Same(starting, cache.TryGetCollectionMetadata(failure));
		Assert.Same(starting, cache.TryGetCollectionMetadata(failure.TestCollectionUniqueID, remove: false));
		Assert.Same(starting, cache.TryGetCollectionMetadata(failure.TestCollectionUniqueID, remove: true));

		// After removal, we're empty again
		Assert.Null(cache.TryGetCollectionMetadata(failure));

		// Now set and use the explicit finish removal
		cache.Set(starting);
		Assert.Same(starting, cache.TryRemove(finished));

		// Finish empty
		Assert.Null(cache.TryGetCollectionMetadata(failure));
	}


	[Fact]
	public void TestTest()
	{
		var starting = TestData.TestStarting();
		var failure = TestData.TestCleanupFailure();
		var finished = TestData.TestFinished();
		var cache = new MessageMetadataCache();

		// Starts empty
		Assert.Null(cache.TryGetTestMetadata(failure));

		// Set the cache, should be able to retrieve it
		cache.Set(starting);
		Assert.Same(starting, cache.TryGetTestMetadata(failure));
		Assert.Same(starting, cache.TryGetTestMetadata(failure.TestUniqueID, remove: false));
		Assert.Same(starting, cache.TryGetTestMetadata(failure.TestUniqueID, remove: true));

		// After removal, we're empty again
		Assert.Null(cache.TryGetTestMetadata(failure));

		// Now set and use the explicit finish removal
		cache.Set(starting);
		Assert.Same(starting, cache.TryRemove(finished));

		// Finish empty
		Assert.Null(cache.TryGetTestMetadata(failure));
	}


	[Fact]
	public void TestMethodTest()
	{
		var starting = TestData.TestMethodStarting();
		var failure = TestData.TestMethodCleanupFailure();
		var finished = TestData.TestMethodFinished();
		var cache = new MessageMetadataCache();

		// Starts empty
		Assert.Null(cache.TryGetMethodMetadata(failure));

		// Set the cache, should be able to retrieve it
		cache.Set(starting);
		Assert.Same(starting, cache.TryGetMethodMetadata(failure));
		Assert.Same(starting, cache.TryGetMethodMetadata(failure.TestMethodUniqueID!, remove: false));
		Assert.Same(starting, cache.TryGetMethodMetadata(failure.TestMethodUniqueID!, remove: true));

		// After removal, we're empty again
		Assert.Null(cache.TryGetMethodMetadata(failure));

		// Now set and use the explicit finish removal
		cache.Set(starting);
		Assert.Same(starting, cache.TryRemove(finished));

		// Finish empty
		Assert.Null(cache.TryGetMethodMetadata(failure));
	}
}
