using System;
using System.Collections.Generic;
using System.Globalization;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.Runner.Common;

/// <summary>
/// Caches message metadata for xUnit.net v3 messages. The metadata which is cached depends on
/// the message that is passed (for example, passing a <see cref="_TestAssemblyMessage"/> will store
/// and/or return <see cref="_IAssemblyMetadata"/>). Storage methods require the Starting versions
/// of messages, as these are the ones which contain the metadata.
/// </summary>
public class MessageMetadataCache
{
	readonly Dictionary<string, object> cache = [];

	/// <summary>
	/// Sets <see cref="_IAssemblyMetadata"/> into the cache.
	/// </summary>
	/// <param name="message">The message that contains the metadata.</param>
	public void Set(_TestAssemblyStarting message)
	{
		Guard.ArgumentNotNull(message);
		Guard.NotNull(
			() => string.Format(
				CultureInfo.CurrentCulture,
				"{0} cannot be null when setting metadata for {1}",
				nameof(_TestAssemblyStarting.AssemblyUniqueID),
				typeof(_TestAssemblyStarting).SafeName()
			),
			message.AssemblyUniqueID
		);

		InternalSet(message.AssemblyUniqueID, message);
	}

	/// <summary>
	/// Sets <see cref="_ITestCaseMetadata"/> into the cache.
	/// </summary>
	/// <param name="message">The message that contains the metadata.</param>
	public void Set(_TestCaseStarting message)
	{
		Guard.ArgumentNotNull(message);
		Guard.NotNull(
			() => string.Format(
				CultureInfo.CurrentCulture,
				"{0} cannot be null when setting metadata for {1}",
				nameof(_TestCaseStarting.TestCaseUniqueID),
				typeof(_TestCaseStarting).SafeName()
			),
			message.TestCaseUniqueID
		);

		InternalSet(message.TestCaseUniqueID, message);
	}

	/// <summary>
	/// Sets <see cref="_ITestClassMetadata"/> into the cache.
	/// </summary>
	/// <param name="message">The message that contains the metadata.</param>
	public void Set(_TestClassStarting message)
	{
		Guard.ArgumentNotNull(message);
		Guard.NotNull(
			() => string.Format(
				CultureInfo.CurrentCulture,
				"{0} cannot be null when setting metadata for {1}",
				nameof(_TestClassStarting.TestClassUniqueID),
				typeof(_TestClassStarting).SafeName()
			),
			message.TestClassUniqueID
		);

		InternalSet(message.TestClassUniqueID, message);
	}

	/// <summary>
	/// Sets <see cref="_ITestCollectionMetadata"/> into the cache.
	/// </summary>
	/// <param name="message">The message that contains the metadata.</param>
	public void Set(_TestCollectionStarting message)
	{
		Guard.ArgumentNotNull(message);
		Guard.NotNull(
			() => string.Format(
				CultureInfo.CurrentCulture,
				"{0} cannot be null when setting metadata for {1}",
				nameof(_TestCollectionStarting.TestCollectionUniqueID),
				typeof(_TestCollectionStarting).SafeName()
			),
			message.TestCollectionUniqueID
		);

		InternalSet(message.TestCollectionUniqueID, message);
	}

	/// <summary>
	/// Sets <see cref="_ITestMetadata"/> into the cache.
	/// </summary>
	/// <param name="message">The message that contains the metadata.</param>
	public void Set(_TestStarting message)
	{
		Guard.ArgumentNotNull(message);
		Guard.NotNull(
			() => string.Format(
				CultureInfo.CurrentCulture,
				"{0} cannot be null when setting metadata for {1}",
				nameof(_TestStarting.TestUniqueID),
				typeof(_TestStarting).SafeName()
			),
			message.TestUniqueID
		);

		InternalSet(message.TestUniqueID, message);
	}

	/// <summary>
	/// Sets <see cref="_ITestMethodMetadata"/> into the cache.
	/// </summary>
	/// <param name="message">The message that contains the metadata.</param>
	public void Set(_TestMethodStarting message)
	{
		Guard.ArgumentNotNull(message);
		Guard.NotNull(
			() => string.Format(
				CultureInfo.CurrentCulture,
				"{0} cannot be null when setting metadata for {1}",
				nameof(_TestMethodStarting.TestMethodUniqueID),
				typeof(_TestMethodStarting).SafeName()
			),
			message.TestMethodUniqueID
		);

		InternalSet(message.TestMethodUniqueID, message);
	}

	/// <summary>
	/// Attempts to retrieve <see cref="_IAssemblyMetadata"/> from the cache (and optionally remove it).
	/// </summary>
	/// <param name="assemblyUniqueID">The unique ID of the assembly to retrieve.</param>
	/// <param name="remove">Set to <c>true</c> to remove the metadata after retrieval.</param>
	/// <returns>The cached metadata, if present; or <c>null</c> if there isn't any.</returns>
	public _IAssemblyMetadata? TryGetAssemblyMetadata(
		string assemblyUniqueID,
		bool remove = false) =>
			InternalGetAndRemove(Guard.ArgumentNotNull(assemblyUniqueID), remove) as _IAssemblyMetadata;

	/// <summary>
	/// Attempts to retrieve <see cref="_IAssemblyMetadata"/> from the cache.
	/// </summary>
	/// <param name="message">The message that indicates which metadata to retrieve.</param>
	/// <returns>The cached metadata, if present; or <c>null</c> if there isn't any.</returns>
	public _IAssemblyMetadata? TryGetAssemblyMetadata(_TestAssemblyMessage message) =>
		InternalGetAndRemove(Guard.ArgumentNotNull(message).AssemblyUniqueID, false) as _IAssemblyMetadata;

	/// <summary>
	/// Attempts to retrieve <see cref="_ITestCaseMetadata"/> from the cache (and optionally remove it).
	/// </summary>
	/// <param name="testCaseUniqueID">The unique ID of the test case to retrieve.</param>
	/// <param name="remove">Set to <c>true</c> to remove the metadata after retrieval.</param>
	/// <returns>The cached metadata, if present; or <c>null</c> if there isn't any.</returns>
	public _ITestCaseMetadata? TryGetTestCaseMetadata(
		string testCaseUniqueID,
		bool remove = false) =>
			InternalGetAndRemove(Guard.ArgumentNotNull(testCaseUniqueID), remove) as _ITestCaseMetadata;

	/// <summary>
	/// Attempts to retrieve <see cref="_ITestCaseMetadata"/> from the cache.
	/// </summary>
	/// <param name="message">The message that indicates which metadata to retrieve.</param>
	/// <returns>The cached metadata, if present; or <c>null</c> if there isn't any.</returns>
	public _ITestCaseMetadata? TryGetTestCaseMetadata(_TestCaseMessage message) =>
		InternalGetAndRemove(Guard.ArgumentNotNull(message).TestCaseUniqueID, false) as _ITestCaseMetadata;

	/// <summary>
	/// Attempts to retrieve <see cref="_ITestClassMetadata"/> from the cache (and optionally remove it).
	/// </summary>
	/// <param name="testClassUniqueID">The unique ID of the test class to retrieve.</param>
	/// <param name="remove">Set to <c>true</c> to remove the metadata after retrieval.</param>
	/// <returns>The cached metadata, if present; or <c>null</c> if there isn't any.</returns>
	public _ITestClassMetadata? TryGetClassMetadata(
		string testClassUniqueID,
		bool remove = false) =>
			InternalGetAndRemove(Guard.ArgumentNotNull(testClassUniqueID), remove) as _ITestClassMetadata;

	/// <summary>
	/// Attempts to retrieve <see cref="_ITestClassMetadata"/> from the cache.
	/// </summary>
	/// <param name="message">The message that indicates which metadata to retrieve.</param>
	/// <returns>The cached metadata, if present; or <c>null</c> if there isn't any.</returns>
	public _ITestClassMetadata? TryGetClassMetadata(_TestClassMessage message) =>
		InternalGetAndRemove(Guard.ArgumentNotNull(message).TestClassUniqueID, false) as _ITestClassMetadata;

	/// <summary>
	/// Attempts to retrieve <see cref="_ITestCollectionMetadata"/> from the cache (and optionally remove it).
	/// </summary>
	/// <param name="testCollectionUniqueID">The unique ID of the test collection to retrieve.</param>
	/// <param name="remove">Set to <c>true</c> to remove the metadata after retrieval.</param>
	/// <returns>The cached metadata, if present; or <c>null</c> if there isn't any.</returns>
	public _ITestCollectionMetadata? TryGetCollectionMetadata(
		string testCollectionUniqueID,
		bool remove = false) =>
			InternalGetAndRemove(Guard.ArgumentNotNull(testCollectionUniqueID), remove) as _ITestCollectionMetadata;

	/// <summary>
	/// Attempts to retrieve <see cref="_ITestCollectionMetadata"/> from the cache.
	/// </summary>
	/// <param name="message">The message that indicates which metadata to retrieve.</param>
	/// <returns>The cached metadata, if present; or <c>null</c> if there isn't any.</returns>
	public _ITestCollectionMetadata? TryGetCollectionMetadata(_TestCollectionMessage message) =>
		InternalGetAndRemove(Guard.ArgumentNotNull(message).TestCollectionUniqueID, false) as _ITestCollectionMetadata;

	/// <summary>
	/// Attempts to retrieve <see cref="_ITestMethodMetadata"/> from the cache (and optionally remove it).
	/// </summary>
	/// <param name="testMethodUniqueID">The unique ID of the test method to retrieve.</param>
	/// <param name="remove">Set to <c>true</c> to remove the metadata after retrieval.</param>
	/// <returns>The cached metadata, if present; or <c>null</c> if there isn't any.</returns>
	public _ITestMethodMetadata? TryGetMethodMetadata(
		string testMethodUniqueID,
		bool remove = false) =>
			InternalGetAndRemove(Guard.ArgumentNotNull(testMethodUniqueID), remove) as _ITestMethodMetadata;

	/// <summary>
	/// Attempts to retrieve <see cref="_ITestMethodMetadata"/> from the cache.
	/// </summary>
	/// <param name="message">The message that indicates which metadata to retrieve.</param>
	/// <returns>The cached metadata, if present; or <c>null</c> if there isn't any.</returns>
	public _ITestMethodMetadata? TryGetMethodMetadata(_TestMethodMessage message) =>
		InternalGetAndRemove(Guard.ArgumentNotNull(message).TestMethodUniqueID, false) as _ITestMethodMetadata;

	/// <summary>
	/// Attempts to retrieve <see cref="_ITestMetadata"/> from the cache (and optionally remove it).
	/// </summary>
	/// <param name="testUniqueID">The unique ID of the test to retrieve.</param>
	/// <param name="remove">Set to <c>true</c> to remove the metadata after retrieval.</param>
	/// <returns>The cached metadata, if present; or <c>null</c> if there isn't any.</returns>
	public _ITestMetadata? TryGetTestMetadata(
		string testUniqueID,
		bool remove = false) =>
			InternalGetAndRemove(Guard.ArgumentNotNull(testUniqueID), remove) as _ITestMetadata;

	/// <summary>
	/// Attempts to retrieve <see cref="_ITestMetadata"/> from the cache.
	/// </summary>
	/// <param name="message">The message that indicates which metadata to retrieve.</param>
	/// <returns>The cached metadata, if present; or <c>null</c> if there isn't any.</returns>
	public _ITestMetadata? TryGetTestMetadata(_TestMessage message) =>
		InternalGetAndRemove(Guard.ArgumentNotNull(message).TestUniqueID, false) as _ITestMetadata;

	/// <summary>
	/// Attempts to retrieve <see cref="_IAssemblyMetadata"/> from the cache, and if present,
	/// removes the metadata from the cache.
	/// </summary>
	/// <param name="message">The message that indicates which metadata to retrieve.</param>
	/// <returns>The cached metadata, if present; or <c>null</c> if there isn't any.</returns>
	public _IAssemblyMetadata? TryRemove(_TestAssemblyFinished message) =>
		InternalGetAndRemove(Guard.ArgumentNotNull(message).AssemblyUniqueID, true) as _IAssemblyMetadata;

	/// <summary>
	/// Attempts to retrieve <see cref="_ITestCaseMetadata"/> from the cache, and if present,
	/// removes the metadata from the cache.
	/// </summary>
	/// <param name="message">The message that indicates which metadata to retrieve.</param>
	/// <returns>The cached metadata, if present; or <c>null</c> if there isn't any.</returns>
	public _ITestCaseMetadata? TryRemove(_TestCaseFinished message) =>
		InternalGetAndRemove(Guard.ArgumentNotNull(message).TestCaseUniqueID, true) as _ITestCaseMetadata;

	/// <summary>
	/// Attempts to retrieve <see cref="_ITestClassMetadata"/> from the cache, and if present,
	/// removes the metadata from the cache.
	/// </summary>
	/// <param name="message">The message that indicates which metadata to retrieve.</param>
	/// <returns>The cached metadata, if present; or <c>null</c> if there isn't any.</returns>
	public _ITestClassMetadata? TryRemove(_TestClassFinished message) =>
		InternalGetAndRemove(Guard.ArgumentNotNull(message).TestClassUniqueID, true) as _ITestClassMetadata;

	/// <summary>
	/// Attempts to retrieve <see cref="_ITestCollectionMetadata"/> from the cache, and if present,
	/// removes the metadata from the cache.
	/// </summary>
	/// <param name="message">The message that indicates which metadata to retrieve.</param>
	/// <returns>The cached metadata, if present; or <c>null</c> if there isn't any.</returns>
	public _ITestCollectionMetadata? TryRemove(_TestCollectionFinished message) =>
		InternalGetAndRemove(Guard.ArgumentNotNull(message).TestCollectionUniqueID, true) as _ITestCollectionMetadata;

	/// <summary>
	/// Attempts to retrieve <see cref="_ITestMetadata"/> from the cache, and if present,
	/// removes the metadata from the cache.
	/// </summary>
	/// <param name="message">The message that indicates which metadata to retrieve.</param>
	/// <returns>The cached metadata, if present; or <c>null</c> if there isn't any.</returns>
	public _ITestMetadata? TryRemove(_TestFinished message) =>
		InternalGetAndRemove(Guard.ArgumentNotNull(message).TestUniqueID, true) as _ITestMetadata;

	/// <summary>
	/// Attempts to retrieve <see cref="_ITestMethodMetadata"/> from the cache, and if present,
	/// removes the metadata from the cache.
	/// </summary>
	/// <param name="message">The message that indicates which metadata to retrieve.</param>
	/// <returns>The cached metadata, if present; or <c>null</c> if there isn't any.</returns>
	public _ITestMethodMetadata? TryRemove(_TestMethodFinished message) =>
		InternalGetAndRemove(Guard.ArgumentNotNull(message).TestMethodUniqueID, true) as _ITestMethodMetadata;

	// Helpers

	object? InternalGetAndRemove(
		string? uniqueID,
		bool remove)
	{
		if (uniqueID is null)
			return null;

		lock (cache)
		{
			if (cache.TryGetValue(uniqueID, out var metadata) && remove)
				cache.Remove(uniqueID);

			return metadata;
		}
	}

	void InternalSet(
		string uniqueID,
		object metadata)
	{
		lock (cache)
		{
#pragma warning disable CA1854 // This isn't getting a value, it's setting one
			if (cache.ContainsKey(uniqueID))
				throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "Key '{0}' already exists in the message metadata cache.{1}Old item: {2}{3}New item: {4}", uniqueID, Environment.NewLine, cache[uniqueID], Environment.NewLine, metadata));
#pragma warning restore CA1854

			cache.Add(uniqueID, metadata);
		}
	}
}
