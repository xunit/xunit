using System.Globalization;
using Xunit.Internal;
using Xunit.Sdk;

#if XUNIT_RUNNER_COMMON
namespace Xunit.Runner.Common;
#else
namespace Xunit.v3;
#endif

/// <summary>
/// Default implementation of <see cref="IDiscoveryComplete"/>.
/// </summary>
[JsonTypeID("discovery-complete")]
sealed partial class DiscoveryComplete : TestAssemblyMessage, IDiscoveryComplete
{
	/// <inheritdoc/>
	protected override void Serialize(JsonObjectSerializer serializer)
	{
		Guard.ArgumentNotNull(serializer);

		base.Serialize(serializer);

		serializer.Serialize(nameof(TestCasesToRun), TestCasesToRun);
	}

	/// <inheritdoc/>
	public override string ToString() =>
		string.Format(CultureInfo.CurrentCulture, "{0} testCasesToRun={1}", base.ToString(), TestCasesToRun);
}
