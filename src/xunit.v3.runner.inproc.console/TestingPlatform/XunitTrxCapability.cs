using Microsoft.Testing.Extensions.TrxReport.Abstractions;

namespace Xunit.Runner.InProc.SystemConsole.TestingPlatform;

/// <summary>
/// Implementation of <see cref="ITrxReportCapability"/> to supplement TRX reporting
/// with xUnit.net v3 information.
/// </summary>
/// <remarks>
/// This class is an implementation detail for Microsoft.Testing.Platform that is public for testing purposes.
/// Use this class at your own risk, as breaking changes may occur as needed.
/// </remarks>
public sealed class XunitTrxCapability(bool trxEnabled = false) :
	ITrxReportCapability
{
	/// <summary>
	/// Gets a flag to indicate whether TRX reporting is enabled
	/// </summary>
	public bool IsTrxEnabled { get; private set; } = trxEnabled;

	/// <inheritdoc/>
	public bool IsSupported =>
		true;

	/// <inheritdoc/>
	public void Enable() =>
		IsTrxEnabled = true;
}
