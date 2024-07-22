using System.Globalization;
using Xunit.Internal;
using Xunit.Sdk;

#if XUNIT_RUNNER_COMMON
namespace Xunit.Runner.Common;
#else
namespace Xunit.v3;
#endif

/// <summary>
/// Default implementation of <see cref="ITestStarting"/>.
/// </summary>
[JsonTypeID("test-starting")]
sealed partial class TestStarting : TestMessage, ITestStarting
{
	string ITestMetadata.UniqueID =>
		TestUniqueID;

	/// <inheritdoc/>
	protected override void Serialize(JsonObjectSerializer serializer)
	{
		Guard.ArgumentNotNull(serializer);

		base.Serialize(serializer);

		serializer.Serialize(nameof(Explicit), Explicit);
		serializer.Serialize(nameof(StartTime), StartTime);
		serializer.Serialize(nameof(TestDisplayName), TestDisplayName);
		serializer.Serialize(nameof(Timeout), Timeout);
		serializer.SerializeTraits(nameof(Traits), Traits);
	}

	/// <inheritdoc/>
	public override string ToString() =>
		string.Format(CultureInfo.CurrentCulture, "{0} name={1}", base.ToString(), TestDisplayName.Quoted());
}
