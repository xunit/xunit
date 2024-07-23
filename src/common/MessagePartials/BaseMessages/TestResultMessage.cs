using System.Globalization;
using Xunit.Internal;
using Xunit.Sdk;

#if XUNIT_RUNNER_COMMON
namespace Xunit.Runner.Common;
#else
namespace Xunit.v3;
#endif

/// <summary>
/// Default implementation of <see cref="ITestResultMessage"/>.
/// </summary>
abstract partial class TestResultMessage : TestMessage, ITestResultMessage
{
	/// <inheritdoc/>
	protected override void Serialize(JsonObjectSerializer serializer)
	{
		Guard.ArgumentNotNull(serializer);

		base.Serialize(serializer);

		serializer.Serialize(nameof(ExecutionTime), ExecutionTime);
		serializer.Serialize(nameof(FinishTime), FinishTime);
		serializer.Serialize(nameof(Output), Output, includeEmptyValues: false);
		serializer.SerializeStringArray(nameof(Warnings), Warnings);
	}

	/// <inheritdoc/>
	public override string ToString() =>
		string.Format(CultureInfo.CurrentCulture, "{0} time={1}", base.ToString(), ExecutionTime);
}
