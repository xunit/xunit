using System;
using System.ComponentModel;
using System.Globalization;
using Xunit.Internal;
using Xunit.Sdk;

#if XUNIT_RUNNER_COMMON
namespace Xunit.Runner.Common;
#else
namespace Xunit.v3;
#endif

/// <summary>
/// Default implementation of <see cref="IErrorMessage"/>.
/// </summary>
[JsonTypeID("error")]
sealed partial class ErrorMessage : MessageSinkMessage, IErrorMessage
{
	/// <summary>
	/// Please use <see cref="FromException(Exception, string?)"/>.
	/// This overload will be removed in the next major version.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Please use the factory function which accepts assemblyUniqueID. This overload will be removed in the next major version.")]
	public static IErrorMessage FromException(Exception ex) =>
		FromException(ex, null);

	/// <summary>
	/// Creates a new <see cref="IErrorMessage"/> constructed from an <see cref="Exception"/> object.
	/// </summary>
	/// <param name="ex">The exception to use</param>
	/// <param name="assemblyUniqueID">The optional assembly unique ID, if this error belongs to an assembly.</param>
	public static IErrorMessage FromException(
		Exception ex,
		string? assemblyUniqueID)
	{
		Guard.ArgumentNotNull(ex);

		var errorMetadata = ExceptionUtility.ExtractMetadata(ex);

		return new ErrorMessage
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

		serializer.Serialize(nameof(AssemblyUniqueID), AssemblyUniqueID);
		serializer.SerializeIntArray(nameof(ExceptionParentIndices), ExceptionParentIndices);
		serializer.SerializeStringArray(nameof(ExceptionTypes), ExceptionTypes);
		serializer.SerializeStringArray(nameof(Messages), Messages);
		serializer.SerializeStringArray(nameof(StackTraces), StackTraces);
	}

	/// <inheritdoc/>
	public override string ToString() =>
		string.Format(CultureInfo.CurrentCulture, "{0} types={1} messages={2}", base.ToString(), ToDisplayString(ExceptionTypes), ToDisplayString(Messages));
}
