namespace Xunit.Runner.Common;

/// <summary>
/// Indicates the current level of app domain support that's in effect,
/// for use by runner reporters.
/// </summary>
public enum AppDomainOption
{
	/// <summary>
	/// App domains are not supported by the current platform
	/// </summary>
	NotAvailable,

	/// <summary>
	/// App domains are supported, but currently disabled
	/// </summary>
	Disabled,

	/// <summary>
	/// App domains are supported and currently enabled
	/// </summary>
	Enabled,
}
