#pragma warning disable xUnit3000 // This class does not have direct access to v2 xunit.runner.utility, so it can't derive from v2's LLMBRO

using System;
using Xunit.Abstractions;
using Xunit.Internal;

namespace Xunit.Runner.v2;

/// <summary>
/// Default implementation of <see cref="ISourceInformation"/>.
/// </summary>
public class Xunit2SourceInformation : MarshalByRefObject, ISourceInformation
{
	/// <inheritdoc/>
	public string? FileName { get; set; }

	/// <inheritdoc/>
	public int? LineNumber { get; set; }

	/// <inheritdoc/>
	public void Serialize(IXunitSerializationInfo info)
	{
		Guard.ArgumentNotNull(info);

		info.AddValue("FileName", FileName);
		info.AddValue("LineNumber", LineNumber);
	}

	/// <inheritdoc/>
	public void Deserialize(IXunitSerializationInfo info)
	{
		Guard.ArgumentNotNull(info);

		FileName = info.GetValue<string>("FileName");
		LineNumber = info.GetValue<int?>("LineNumber");
	}

#if NETFRAMEWORK
	/// <inheritdoc/>
	[System.Security.SecurityCritical]
	public sealed override object InitializeLifetimeService() => null!;
#endif
}
