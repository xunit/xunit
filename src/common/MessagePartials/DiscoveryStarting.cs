using System.Globalization;
using Xunit.Internal;
using Xunit.Sdk;

#if XUNIT_RUNNER_COMMON
namespace Xunit.Runner.Common;
#else
namespace Xunit.v3;
#endif

/// <summary>
/// Default implementation of <see cref="IDiscoveryStarting"/>.
/// </summary>
[JsonTypeID("discovery-starting")]
sealed partial class DiscoveryStarting : TestAssemblyMessage, IDiscoveryStarting
{
	/// <inheritdoc/>
	protected override void Serialize(JsonObjectSerializer serializer)
	{
		Guard.ArgumentNotNull(serializer);

		base.Serialize(serializer);

		serializer.Serialize(nameof(AssemblyName), AssemblyName);
		serializer.Serialize(nameof(AssemblyPath), AssemblyPath);
		serializer.Serialize(nameof(ConfigFilePath), ConfigFilePath);
	}

	/// <inheritdoc/>
	public override string ToString() =>
		string.Format(CultureInfo.CurrentCulture, "{0} name={1} path={2} config={3}", base.ToString(), AssemblyName.Quoted(), AssemblyPath.Quoted(), ConfigFilePath.Quoted());
}
