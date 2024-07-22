using System;
using System.Globalization;
using Xunit.Internal;
using Xunit.Sdk;

#if XUNIT_RUNNER_COMMON
namespace Xunit.Runner.Common;
#else
namespace Xunit.v3;
#endif

/// <summary>
/// Default implementation of <see cref="ITestCollectionCleanupFailure"/>.
/// </summary>
[JsonTypeID("test-collection-cleanup-failure")]
sealed partial class TestCollectionCleanupFailure : TestCollectionMessage, ITestCollectionCleanupFailure
{
	/// <summary>
	/// Creates a new <see cref="ITestCollectionCleanupFailure"/> constructed from an <see cref="Exception"/> object.
	/// </summary>
	/// <param name="ex">The exception to use</param>
	/// <param name="assemblyUniqueID">The unique ID of the assembly</param>
	/// <param name="testCollectionUniqueID">The unique ID of the test collectioon</param>
	public static ITestCollectionCleanupFailure FromException(
		Exception ex,
		string assemblyUniqueID,
		string testCollectionUniqueID)
	{
		Guard.ArgumentNotNull(ex);
		Guard.ArgumentNotNull(assemblyUniqueID);
		Guard.ArgumentNotNull(testCollectionUniqueID);

		var errorMetadata = ExceptionUtility.ExtractMetadata(ex);

		return new TestCollectionCleanupFailure
		{
			AssemblyUniqueID = assemblyUniqueID,
			TestCollectionUniqueID = testCollectionUniqueID,
			ExceptionTypes = errorMetadata.ExceptionTypes,
			Messages = errorMetadata.Messages,
			StackTraces = errorMetadata.StackTraces,
			ExceptionParentIndices = errorMetadata.ExceptionParentIndices,
		};
	}

	/// <inheritdoc/>
	protected override void Serialize(JsonObjectSerializer serializer)
	{
		Guard.ArgumentNotNull(serializer);

		base.Serialize(serializer);

		serializer.SerializeIntArray(nameof(ExceptionParentIndices), ExceptionParentIndices);
		serializer.SerializeStringArray(nameof(ExceptionTypes), ExceptionTypes);
		serializer.SerializeStringArray(nameof(Messages), Messages);
		serializer.SerializeStringArray(nameof(StackTraces), StackTraces);
	}

	/// <inheritdoc/>
	public override string ToString() =>
		string.Format(CultureInfo.CurrentCulture, "{0} types={1} messages={2}", base.ToString(), ToDisplayString(ExceptionTypes), ToDisplayString(Messages));
}
