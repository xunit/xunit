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
/// Default implementation of <see cref="ITestAssemblyCleanupFailure"/>.
/// </summary>
[JsonTypeID("test-assembly-cleanup-failure")]
sealed partial class TestAssemblyCleanupFailure : TestAssemblyMessage, ITestAssemblyCleanupFailure
{
	/// <summary>
	/// Creates a new <see cref="ITestAssemblyCleanupFailure"/> constructed from an <see cref="Exception"/> object.
	/// </summary>
	/// <param name="ex">The exception to use</param>
	/// <param name="assemblyUniqueID">The unique ID of the assembly</param>
	public static ITestAssemblyCleanupFailure FromException(
		Exception ex,
		string assemblyUniqueID)
	{
		Guard.ArgumentNotNull(ex);
		Guard.ArgumentNotNull(assemblyUniqueID);

		var errorMetadata = ExceptionUtility.ExtractMetadata(ex);

		return new TestAssemblyCleanupFailure
		{
			AssemblyUniqueID = assemblyUniqueID,
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
