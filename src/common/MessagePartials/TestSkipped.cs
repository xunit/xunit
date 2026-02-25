using Xunit.Sdk;

#if XUNIT_RUNNER_COMMON
namespace Xunit.Runner.Common;
#else
namespace Xunit.v3;
#endif

/// <summary>
/// Default implementation of <see cref="ITestSkipped"/>.
/// </summary>
[JsonTypeID(TypeID)]
sealed partial class TestSkipped : TestResultMessage, ITestSkipped
{
	internal const string TypeID = "test-skipped";
	/// <inheritdoc/>
	protected override void Serialize(JsonObjectSerializer serializer)
	{
		Guard.ArgumentNotNull(serializer);

		base.Serialize(serializer);

		serializer.Serialize(nameof(Reason), Reason);
	}

	/// <inheritdoc/>
	public override string ToString() =>
		string.Format(CultureInfo.CurrentCulture, "{0} reason={1}", base.ToString(), Reason.Quoted());
}
