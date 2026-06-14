using Xunit.Sdk;

namespace Xunit.Runner.Common;

/// <summary>
/// Caches message metadata for xUnit.net v3 messages. The metadata which is cached depends on
/// the message that is passed (for example, looking up with an <see cref="ITestAssemblyMessage"/>
/// will return an <see cref="IAssemblyMetadata"/>). Storage methods require the "Starting" versions
/// of messages (as these are the ones which contain the metadata), and removal methods require the
/// "Finished" versions of messages.
/// </summary>
public class MessageMetadataCache
{
	readonly Dictionary<string, RefCount> cache = [];

	/// <summary>
	/// Sets <see cref="IAssemblyMetadata"/> into the cache.
	/// </summary>
	/// <param name="message">The message that contains the metadata.</param>
	public void Set(ITestAssemblyStarting message)
	{
		Guard.ArgumentNotNull(message);
		Guard.NotNull(
			() => string.Format(
				CultureInfo.CurrentCulture,
				"{0} cannot be null when setting metadata for {1}",
				nameof(ITestAssemblyStarting.AssemblyUniqueID),
				typeof(ITestAssemblyStarting).SafeName()
			),
			message.AssemblyUniqueID
		);

		InternalSet(message.AssemblyUniqueID, message);
	}

	/// <summary>
	/// Sets <see cref="ITestCaseMetadata"/> into the cache.
	/// </summary>
	/// <param name="message">The message that contains the metadata.</param>
	public void Set(ITestCaseStarting message)
	{
		Guard.ArgumentNotNull(message);
		Guard.NotNull(
			() => string.Format(
				CultureInfo.CurrentCulture,
				"{0} cannot be null when setting metadata for {1}",
				nameof(ITestCaseStarting.TestCaseUniqueID),
				typeof(ITestCaseStarting).SafeName()
			),
			message.TestCaseUniqueID
		);

		InternalSet(message.TestCaseUniqueID, message);
	}

	/// <summary>
	/// Sets <see cref="ITestClassMetadata"/> into the cache.
	/// </summary>
	/// <param name="message">The message that contains the metadata.</param>
	public void Set(ITestClassStarting message)
	{
		Guard.ArgumentNotNull(message);
		Guard.NotNull(
			() => string.Format(
				CultureInfo.CurrentCulture,
				"{0} cannot be null when setting metadata for {1}",
				nameof(ITestClassStarting.TestClassUniqueID),
				typeof(ITestClassStarting).SafeName()
			),
			message.TestClassUniqueID
		);

		InternalSet(message.TestClassUniqueID, message);
	}

	/// <summary>
	/// Sets <see cref="ITestCollectionMetadata"/> into the cache.
	/// </summary>
	/// <param name="message">The message that contains the metadata.</param>
	public void Set(ITestCollectionStarting message)
	{
		Guard.ArgumentNotNull(message);
		Guard.NotNull(
			() => string.Format(
				CultureInfo.CurrentCulture,
				"{0} cannot be null when setting metadata for {1}",
				nameof(ITestCollectionStarting.TestCollectionUniqueID),
				typeof(ITestCollectionStarting).SafeName()
			),
			message.TestCollectionUniqueID
		);

		InternalSet(message.TestCollectionUniqueID, message);
	}

	/// <summary>
	/// Sets <see cref="ITestMetadata"/> into the cache.
	/// </summary>
	/// <param name="message">The message that contains the metadata.</param>
	public void Set(ITestStarting message)
	{
		Guard.ArgumentNotNull(message);
		Guard.NotNull(
			() => string.Format(
				CultureInfo.CurrentCulture,
				"{0} cannot be null when setting metadata for {1}",
				nameof(ITestStarting.TestUniqueID),
				typeof(ITestStarting).SafeName()
			),
			message.TestUniqueID
		);

		InternalSet(message.TestUniqueID, message);
	}

	/// <summary>
	/// Sets <see cref="ITestMethodMetadata"/> into the cache.
	/// </summary>
	/// <param name="message">The message that contains the metadata.</param>
	public void Set(ITestMethodStarting message)
	{
		Guard.ArgumentNotNull(message);
		Guard.NotNull(
			() => string.Format(
				CultureInfo.CurrentCulture,
				"{0} cannot be null when setting metadata for {1}",
				nameof(ITestMethodStarting.TestMethodUniqueID),
				typeof(ITestMethodStarting).SafeName()
			),
			message.TestMethodUniqueID
		);

		InternalSet(message.TestMethodUniqueID, message);
	}

	/// <summary>
	/// Attempts to retrieve <see cref="IAssemblyMetadata"/> from the cache (and optionally remove it).
	/// </summary>
	/// <param name="assemblyUniqueID">The unique ID of the assembly to retrieve.</param>
	/// <param name="remove">Set to <see langword="true"/> to remove the metadata after retrieval.</param>
	/// <returns>The cached metadata, if present; or <see langword="null"/> if there isn't any.</returns>
	public IAssemblyMetadata? TryGetAssemblyMetadata(
		string assemblyUniqueID,
		bool remove = false) =>
			InternalGetAndRemove(Guard.ArgumentNotNull(assemblyUniqueID), remove) as IAssemblyMetadata;

	/// <summary>
	/// Attempts to retrieve <see cref="IAssemblyMetadata"/> from the cache.
	/// </summary>
	/// <param name="message">The message that indicates which metadata to retrieve.</param>
	/// <returns>The cached metadata, if present; or <see langword="null"/> if there isn't any.</returns>
	public IAssemblyMetadata? TryGetAssemblyMetadata(ITestAssemblyMessage message) =>
		InternalGetAndRemove(Guard.ArgumentNotNull(message).AssemblyUniqueID, false) as IAssemblyMetadata;

	/// <summary>
	/// Attempts to retrieve <see cref="ITestCaseMetadata"/> from the cache (and optionally remove it).
	/// </summary>
	/// <param name="testCaseUniqueID">The unique ID of the test case to retrieve.</param>
	/// <param name="remove">Set to <see langword="true"/> to remove the metadata after retrieval.</param>
	/// <returns>The cached metadata, if present; or <see langword="null"/> if there isn't any.</returns>
	public ITestCaseMetadata? TryGetTestCaseMetadata(
		string testCaseUniqueID,
		bool remove = false) =>
			InternalGetAndRemove(Guard.ArgumentNotNull(testCaseUniqueID), remove) as ITestCaseMetadata;

	/// <summary>
	/// Attempts to retrieve <see cref="ITestCaseMetadata"/> from the cache.
	/// </summary>
	/// <param name="message">The message that indicates which metadata to retrieve.</param>
	/// <returns>The cached metadata, if present; or <see langword="null"/> if there isn't any.</returns>
	public ITestCaseMetadata? TryGetTestCaseMetadata(ITestCaseMessage message) =>
		InternalGetAndRemove(Guard.ArgumentNotNull(message).TestCaseUniqueID, false) as ITestCaseMetadata;

	/// <summary>
	/// Attempts to retrieve <see cref="ITestClassMetadata"/> from the cache (and optionally remove it).
	/// </summary>
	/// <param name="testClassUniqueID">The unique ID of the test class to retrieve.</param>
	/// <param name="remove">Set to <see langword="true"/> to remove the metadata after retrieval.</param>
	/// <returns>The cached metadata, if present; or <see langword="null"/> if there isn't any.</returns>
	public ITestClassMetadata? TryGetClassMetadata(
		string testClassUniqueID,
		bool remove = false) =>
			InternalGetAndRemove(Guard.ArgumentNotNull(testClassUniqueID), remove) as ITestClassMetadata;

	/// <summary>
	/// Attempts to retrieve <see cref="ITestClassMetadata"/> from the cache.
	/// </summary>
	/// <param name="message">The message that indicates which metadata to retrieve.</param>
	/// <returns>The cached metadata, if present; or <see langword="null"/> if there isn't any.</returns>
	public ITestClassMetadata? TryGetClassMetadata(ITestClassMessage message) =>
		InternalGetAndRemove(Guard.ArgumentNotNull(message).TestClassUniqueID, false) as ITestClassMetadata;

	/// <summary>
	/// Attempts to retrieve <see cref="ITestCollectionMetadata"/> from the cache (and optionally remove it).
	/// </summary>
	/// <param name="testCollectionUniqueID">The unique ID of the test collection to retrieve.</param>
	/// <param name="remove">Set to <see langword="true"/> to remove the metadata after retrieval.</param>
	/// <returns>The cached metadata, if present; or <see langword="null"/> if there isn't any.</returns>
	public ITestCollectionMetadata? TryGetCollectionMetadata(
		string testCollectionUniqueID,
		bool remove = false) =>
			InternalGetAndRemove(Guard.ArgumentNotNull(testCollectionUniqueID), remove) as ITestCollectionMetadata;

	/// <summary>
	/// Attempts to retrieve <see cref="ITestCollectionMetadata"/> from the cache.
	/// </summary>
	/// <param name="message">The message that indicates which metadata to retrieve.</param>
	/// <returns>The cached metadata, if present; or <see langword="null"/> if there isn't any.</returns>
	public ITestCollectionMetadata? TryGetCollectionMetadata(ITestCollectionMessage message) =>
		InternalGetAndRemove(Guard.ArgumentNotNull(message).TestCollectionUniqueID, false) as ITestCollectionMetadata;

	/// <summary>
	/// Attempts to retrieve <see cref="ITestMethodMetadata"/> from the cache (and optionally remove it).
	/// </summary>
	/// <param name="testMethodUniqueID">The unique ID of the test method to retrieve.</param>
	/// <param name="remove">Set to <see langword="true"/> to remove the metadata after retrieval.</param>
	/// <returns>The cached metadata, if present; or <see langword="null"/> if there isn't any.</returns>
	public ITestMethodMetadata? TryGetMethodMetadata(
		string testMethodUniqueID,
		bool remove = false) =>
			InternalGetAndRemove(Guard.ArgumentNotNull(testMethodUniqueID), remove) as ITestMethodMetadata;

	/// <summary>
	/// Attempts to retrieve <see cref="ITestMethodMetadata"/> from the cache.
	/// </summary>
	/// <param name="message">The message that indicates which metadata to retrieve.</param>
	/// <returns>The cached metadata, if present; or <see langword="null"/> if there isn't any.</returns>
	public ITestMethodMetadata? TryGetMethodMetadata(ITestMethodMessage message) =>
		InternalGetAndRemove(Guard.ArgumentNotNull(message).TestMethodUniqueID, false) as ITestMethodMetadata;

	/// <summary>
	/// Attempts to retrieve <see cref="ITestMetadata"/> from the cache (and optionally remove it).
	/// </summary>
	/// <param name="testUniqueID">The unique ID of the test to retrieve.</param>
	/// <param name="remove">Set to <see langword="true"/> to remove the metadata after retrieval.</param>
	/// <returns>The cached metadata, if present; or <see langword="null"/> if there isn't any.</returns>
	public ITestMetadata? TryGetTestMetadata(
		string testUniqueID,
		bool remove = false) =>
			InternalGetAndRemove(Guard.ArgumentNotNull(testUniqueID), remove) as ITestMetadata;

	/// <summary>
	/// Attempts to retrieve <see cref="ITestMetadata"/> from the cache.
	/// </summary>
	/// <param name="message">The message that indicates which metadata to retrieve.</param>
	/// <returns>The cached metadata, if present; or <see langword="null"/> if there isn't any.</returns>
	public ITestMetadata? TryGetTestMetadata(ITestMessage message) =>
		InternalGetAndRemove(Guard.ArgumentNotNull(message).TestUniqueID, false) as ITestMetadata;

	/// <summary>
	/// Attempts to retrieve <see cref="IAssemblyMetadata"/> from the cache, and if present,
	/// removes the metadata from the cache.
	/// </summary>
	/// <param name="message">The message that indicates which metadata to retrieve.</param>
	/// <returns>The cached metadata, if present; or <see langword="null"/> if there isn't any.</returns>
	public IAssemblyMetadata? TryRemove(ITestAssemblyFinished message) =>
		InternalGetAndRemove(Guard.ArgumentNotNull(message).AssemblyUniqueID, true) as IAssemblyMetadata;

	/// <summary>
	/// Attempts to retrieve <see cref="ITestCaseMetadata"/> from the cache, and if present,
	/// removes the metadata from the cache.
	/// </summary>
	/// <param name="message">The message that indicates which metadata to retrieve.</param>
	/// <returns>The cached metadata, if present; or <see langword="null"/> if there isn't any.</returns>
	public ITestCaseMetadata? TryRemove(ITestCaseFinished message) =>
		InternalGetAndRemove(Guard.ArgumentNotNull(message).TestCaseUniqueID, true) as ITestCaseMetadata;

	/// <summary>
	/// Attempts to retrieve <see cref="ITestClassMetadata"/> from the cache, and if present,
	/// removes the metadata from the cache.
	/// </summary>
	/// <param name="message">The message that indicates which metadata to retrieve.</param>
	/// <returns>The cached metadata, if present; or <see langword="null"/> if there isn't any.</returns>
	public ITestClassMetadata? TryRemove(ITestClassFinished message) =>
		InternalGetAndRemove(Guard.ArgumentNotNull(message).TestClassUniqueID, true) as ITestClassMetadata;

	/// <summary>
	/// Attempts to retrieve <see cref="ITestCollectionMetadata"/> from the cache, and if present,
	/// removes the metadata from the cache.
	/// </summary>
	/// <param name="message">The message that indicates which metadata to retrieve.</param>
	/// <returns>The cached metadata, if present; or <see langword="null"/> if there isn't any.</returns>
	public ITestCollectionMetadata? TryRemove(ITestCollectionFinished message) =>
		InternalGetAndRemove(Guard.ArgumentNotNull(message).TestCollectionUniqueID, true) as ITestCollectionMetadata;

	/// <summary>
	/// Attempts to retrieve <see cref="ITestMetadata"/> from the cache, and if present,
	/// removes the metadata from the cache.
	/// </summary>
	/// <param name="message">The message that indicates which metadata to retrieve.</param>
	/// <returns>The cached metadata, if present; or <see langword="null"/> if there isn't any.</returns>
	public ITestMetadata? TryRemove(ITestFinished message) =>
		InternalGetAndRemove(Guard.ArgumentNotNull(message).TestUniqueID, true) as ITestMetadata;

	/// <summary>
	/// Attempts to retrieve <see cref="ITestMethodMetadata"/> from the cache, and if present,
	/// removes the metadata from the cache.
	/// </summary>
	/// <param name="message">The message that indicates which metadata to retrieve.</param>
	/// <returns>The cached metadata, if present; or <see langword="null"/> if there isn't any.</returns>
	public ITestMethodMetadata? TryRemove(ITestMethodFinished message) =>
		InternalGetAndRemove(Guard.ArgumentNotNull(message).TestMethodUniqueID, true) as ITestMethodMetadata;

	// Helpers

	// We're using reference counts instead of throwing, in case we get two metadata with the same unique ID.
	// We don't attempt to reconcile the differences between the two sets of metadata, since this is an illegal
	// scenario (unique IDs must be unique), but we can at least stop throwing. We catch the most common version
	// of this in the front controllers by rejecting test cases with duplicate IDs. The next most likely case is
	// tests with non-unique IDs as a result of non-pre-enumerated theory data that's identical. This would result
	// in identical metadata, so the reference counting strategy will "just work" even though the user has done
	// something that's not supported.
	//
	// We use a struct rather than a class since it saves us on memory usage (no need for heap space, and no
	// need to waste the space and indirection for a pointer). This means re-allocating the struct for the edge
	// cases, which seems entirely reasonable.

	object? InternalGetAndRemove(
		string? uniqueID,
		bool remove)
	{
		if (uniqueID is not null)
			lock (cache)
			{
				if (cache.TryGetValue(uniqueID, out var refCount))
				{
					if (remove)
					{
						if (refCount.Count == 1)
							cache.Remove(uniqueID);
						else
							cache[uniqueID] = new(refCount.Count - 1, refCount.Value);
					}

					return refCount.Value;
				}
			}

		return null;
	}

	void InternalSet(
		string uniqueID,
		object metadata)
	{
		lock (cache)
		{
			if (cache.TryGetValue(uniqueID, out var refCount))
				cache[uniqueID] = new(refCount.Count + 1, refCount.Value);
			else
				cache[uniqueID] = new(1, metadata);
		}
	}

	readonly struct RefCount(
		int count,
		object value)
	{
		public readonly int Count => count;
		public readonly object Value => value;
	}
}
