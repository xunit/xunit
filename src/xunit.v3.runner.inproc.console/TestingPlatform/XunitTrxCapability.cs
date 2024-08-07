using Microsoft.Testing.Extensions.TrxReport.Abstractions;

namespace Xunit.Runner.InProc.SystemConsole.TestingPlatform;

/// <summary>
/// Implementation of <see cref="ITrxReportCapability"/> to supplement TRX reporting
/// with xUnit.net v3 information.
/// </summary>
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
