using System;
using System.Collections.Generic;
using System.Globalization;
using Xunit.Internal;

namespace Xunit.v3;

/// <summary>
/// Caches message metadata. The metadata which is cached depends on the message that is passed
/// (for example, passing a <see cref="_TestAssemblyMessage"/> will store and/or return
/// <see cref="_IAssemblyMetadata"/>). Storage methods require the Starting versions of messages,
/// as these are the ones which contain the metadata.
/// </summary>
public class MessageMetadataCache
{
	readonly Dictionary<string, object> cache = new();

	/// <summary>
	/// Sets <see cref="_IAssemblyMetadata"/> into the cache.
	/// </summary>
	/// <param name="message">The message that contains the metadata.</param>
	public void Set(_TestAssemblyStarting message)
	{
		Guard.ArgumentNotNull(message);
		Guard.NotNull(() => string.Format(CultureInfo.CurrentCulture, "{0} cannot be null when setting metadata for {1}", nameof(_TestAssemblyStarting.AssemblyUniqueID), typeof(_TestAssemblyStarting).FullName), message.AssemblyUniqueID);

		InternalSet(message.AssemblyUniqueID, message);
	}

	/// <summary>
	/// Sets <see cref="_ITestCaseMetadata"/> into the cache.
	/// </summary>
	/// <param name="message">The message that contains the metadata.</param>
	public void Set(_TestCaseStarting message)
	{
		Guard.ArgumentNotNull(message);
		Guard.NotNull(() => string.Format(CultureInfo.CurrentCulture, "{0} cannot be null when setting metadata for {1}", nameof(_TestCaseStarting.TestCaseUniqueID), typeof(_TestCaseStarting).FullName), message.TestCaseUniqueID);

		InternalSet(message.TestCaseUniqueID, message);
	}

	/// <summary>
	/// Sets <see cref="_ITestClassMetadata"/> into the cache.
	/// </summary>
	/// <param name="message">The message that contains the metadata.</param>
	public void Set(_TestClassStarting message)
	{
		Guard.ArgumentNotNull(message);
		Guard.NotNull(() => string.Format(CultureInfo.CurrentCulture, "{0} cannot be null when setting metadata for {1}", nameof(_TestClassStarting.TestClassUniqueID), typeof(_TestClassStarting).FullName), message.TestClassUniqueID);

		InternalSet(message.TestClassUniqueID, message);
	}

	/// <summary>
	/// Sets <see cref="_ITestCollectionMetadata"/> into the cache.
	/// </summary>
	/// <param name="message">The message that contains the metadata.</param>
	public void Set(_TestCollectionStarting message)
	{
		Guard.ArgumentNotNull(message);
		Guard.NotNull(() => string.Format(CultureInfo.CurrentCulture, "{0} cannot be null when setting metadata for {1}", nameof(_TestCollectionStarting.TestCollectionUniqueID), typeof(_TestCollectionStarting).FullName), message.TestCollectionUniqueID);

		InternalSet(message.TestCollectionUniqueID, message);
	}

	/// <summary>
	/// Sets <see cref="_ITestMetadata"/> into the cache.
	/// </summary>
	/// <param name="message">The message that contains the metadata.</param>
	public void Set(_TestStarting message)
	{
		Guard.ArgumentNotNull(message);
		Guard.NotNull(() => string.Format(CultureInfo.CurrentCulture, "{0} cannot be null when setting metadata for {1}", nameof(_TestStarting.TestUniqueID), typeof(_TestStarting).FullName), message.TestUniqueID);

		InternalSet(message.TestUniqueID, message);
	}

	/// <summary>
	/// Sets <see cref="_ITestMethodMetadata"/> into the cache.
	/// </summary>
	/// <param name="message">The message that contains the metadata.</param>
	public void Set(_TestMethodStarting message)
	{
		Guard.ArgumentNotNull(message);
		Guard.NotNull(() => string.Format(CultureInfo.CurrentCulture, "{0} cannot be null when setting metadata for {1}", nameof(_TestMethodStarting.TestMethodUniqueID), typeof(_TestMethodStarting).FullName), message.TestMethodUniqueID);

		InternalSet(message.TestMethodUniqueID, message);
	}

	/// <summary>
	/// Attempts to retrieve <see cref="_IAssemblyMetadata"/> from the cache.
	/// </summary>
	/// <param name="assemblyUniqueID">The unique ID of the assembly to find.</param>
	/// <returns>The cached metadata, if present; or <c>null</c> if there isn't any.</returns>
	public _IAssemblyMetadata? TryGetAssemblyMetadata(string assemblyUniqueID)
	{
		Guard.ArgumentNotNullOrEmpty(assemblyUniqueID);

		return (_IAssemblyMetadata?)InternalGetAndRemove(assemblyUniqueID, false);
	}

	/// <summary>
	/// Attempts to retrieve <see cref="_IAssemblyMetadata"/> from the cache.
	/// </summary>
	/// <param name="message">The message that indicates which metadata to retrieve.</param>
	/// <returns>The cached metadata, if present; or <c>null</c> if there isn't any.</returns>
	public _IAssemblyMetadata? TryGetAssemblyMetadata(_TestAssemblyMessage message) =>
		TryGetAssemblyMetadata(Guard.ArgumentNotNull(message).AssemblyUniqueID);

	/// <summary>
	/// Attempts to retrieve <see cref="_ITestCaseMetadata"/> from the cache.
	/// </summary>
	/// <param name="message">The message that indicates which metadata to retrieve.</param>
	/// <returns>The cached metadata, if present; or <c>null</c> if there isn't any.</returns>
	public _ITestCaseMetadata? TryGetTestCaseMetadata(_TestCaseMessage message)
	{
		Guard.ArgumentNotNull(message);

		return (_ITestCaseMetadata?)InternalGetAndRemove(message.TestCaseUniqueID, false);
	}

	/// <summary>
	/// Attempts to retrieve <see cref="_ITestClassMetadata"/> from the cache.
	/// </summary>
	/// <param name="message">The message that indicates which metadata to retrieve.</param>
	/// <returns>The cached metadata, if present; or <c>null</c> if there isn't any.</returns>
	public _ITestClassMetadata? TryGetClassMetadata(_TestClassMessage message)
	{
		Guard.ArgumentNotNull(message);

		return (_ITestClassMetadata?)InternalGetAndRemove(message.TestClassUniqueID, false);
	}

	/// <summary>
	/// Attempts to retrieve <see cref="_ITestCollectionMetadata"/> from the cache.
	/// </summary>
	/// <param name="message">The message that indicates which metadata to retrieve.</param>
	/// <returns>The cached metadata, if present; or <c>null</c> if there isn't any.</returns>
	public _ITestCollectionMetadata? TryGetCollectionMetadata(_TestCollectionMessage message)
	{
		Guard.ArgumentNotNull(message);

		return (_ITestCollectionMetadata?)InternalGetAndRemove(message.TestCollectionUniqueID, false);
	}

	/// <summary>
	/// Attempts to retrieve <see cref="_ITestMethodMetadata"/> from the cache.
	/// </summary>
	/// <param name="message">The message that indicates which metadata to retrieve.</param>
	/// <returns>The cached metadata, if present; or <c>null</c> if there isn't any.</returns>
	public _ITestMethodMetadata? TryGetMethodMetadata(_TestMethodMessage message)
	{
		Guard.ArgumentNotNull(message);

		return (_ITestMethodMetadata?)InternalGetAndRemove(message.TestMethodUniqueID, false);
	}

	/// <summary>
	/// Attempts to retrieve <see cref="_ITestMetadata"/> from the cache.
	/// </summary>
	/// <param name="message">The message that indicates which metadata to retrieve.</param>
	/// <returns>The cached metadata, if present; or <c>null</c> if there isn't any.</returns>
	public _ITestMetadata? TryGetTestMetadata(_TestMessage message)
	{
		Guard.ArgumentNotNull(message);

		return (_ITestMetadata?)InternalGetAndRemove(message.TestUniqueID, false);
	}

	/// <summary>
	/// Attempts to retrieve <see cref="_IAssemblyMetadata"/> from the cache, and if present,
	/// removes the metadata from the cache.
	/// </summary>
	/// <param name="message">The message that indicates which metadata to retrieve.</param>
	/// <returns>The cached metadata, if present; or <c>null</c> if there isn't any.</returns>
	public _IAssemblyMetadata? TryRemove(_TestAssemblyFinished message)
	{
		Guard.ArgumentNotNull(message);

		return (_IAssemblyMetadata?)InternalGetAndRemove(message.AssemblyUniqueID, true);
	}

	/// <summary>
	/// Attempts to retrieve <see cref="_ITestCaseMetadata"/> from the cache, and if present,
	/// removes the metadata from the cache.
	/// </summary>
	/// <param name="message">The message that indicates which metadata to retrieve.</param>
	/// <returns>The cached metadata, if present; or <c>null</c> if there isn't any.</returns>
	public _ITestCaseMetadata? TryRemove(_TestCaseFinished message)
	{
		Guard.ArgumentNotNull(message);

		return (_ITestCaseMetadata?)InternalGetAndRemove(message.TestCaseUniqueID, true);
	}

	/// <summary>
	/// Attempts to retrieve <see cref="_ITestClassMetadata"/> from the cache, and if present,
	/// removes the metadata from the cache.
	/// </summary>
	/// <param name="message">The message that indicates which metadata to retrieve.</param>
	/// <returns>The cached metadata, if present; or <c>null</c> if there isn't any.</returns>
	public _ITestClassMetadata? TryRemove(_TestClassFinished message)
	{
		Guard.ArgumentNotNull(message);

		return (_ITestClassMetadata?)InternalGetAndRemove(message.TestClassUniqueID, true);
	}

	/// <summary>
	/// Attempts to retrieve <see cref="_ITestCollectionMetadata"/> from the cache, and if present,
	/// removes the metadata from the cache.
	/// </summary>
	/// <param name="message">The message that indicates which metadata to retrieve.</param>
	/// <returns>The cached metadata, if present; or <c>null</c> if there isn't any.</returns>
	public _ITestCollectionMetadata? TryRemove(_TestCollectionFinished message)
	{
		Guard.ArgumentNotNull(message);

		return (_ITestCollectionMetadata?)InternalGetAndRemove(message.TestCollectionUniqueID, true);
	}

	/// <summary>
	/// Attempts to retrieve <see cref="_ITestMetadata"/> from the cache, and if present,
	/// removes the metadata from the cache.
	/// </summary>
	/// <param name="message">The message that indicates which metadata to retrieve.</param>
	/// <returns>The cached metadata, if present; or <c>null</c> if there isn't any.</returns>
	public _ITestMetadata? TryRemove(_TestFinished message)
	{
		Guard.ArgumentNotNull(message);

		return (_ITestMetadata?)InternalGetAndRemove(message.TestUniqueID, true);
	}

	/// <summary>
	/// Attempts to retrieve <see cref="_ITestMethodMetadata"/> from the cache, and if present,
	/// removes the metadata from the cache.
	/// </summary>
	/// <param name="message">The message that indicates which metadata to retrieve.</param>
	/// <returns>The cached metadata, if present; or <c>null</c> if there isn't any.</returns>
	public _ITestMethodMetadata? TryRemove(_TestMethodFinished message)
	{
		Guard.ArgumentNotNull(message);

		return (_ITestMethodMetadata?)InternalGetAndRemove(message.TestMethodUniqueID, true);
	}

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
