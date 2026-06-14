using Xunit.Sdk;

#if XUNIT_RUNNER_COMMON
namespace Xunit.Runner.Common;
#else
namespace Xunit.v3;
#endif

/// <summary>
/// Default implementation of <see cref="ITestClassFinished"/>
/// </summary>
[JsonTypeID(TypeID)]
sealed partial class TestClassFinished : TestClassMessage, ITestClassFinished
{
	internal const string TypeID = "test-class-finished";

	/// <inheritdoc/>
	protected override void Serialize(JsonObjectSerializer serializer)
	{
		Guard.ArgumentNotNull(serializer);

		base.Serialize(serializer);

		serializer.Serialize(nameof(ExecutionTime), ExecutionTime);
		serializer.Serialize(nameof(FinishTime), FinishTime);
		serializer.Serialize(nameof(TestsFailed), TestsFailed);
		serializer.Serialize(nameof(TestsNotRun), TestsNotRun);
		serializer.Serialize(nameof(TestsSkipped), TestsSkipped);
		serializer.Serialize(nameof(TestsTotal), TestsTotal);
	}

	/// <inheritdoc/>
	public override string ToString() =>
		string.Format(
			CultureInfo.CurrentCulture,
			"{0} total={1}{2}{3}{4}{5}",
			base.ToString(),
			TestsTotal,
			TestsFailed != 0 ? $" failed={TestsFailed}" : string.Empty,
			TestsSkipped != 0 ? $" skipped={TestsSkipped}" : string.Empty,
			TestsNotRun != 0 ? $" notRun={TestsNotRun}" : string.Empty,
			ExecutionTime != 0 ? $" time={ExecutionTime}" : string.Empty
		);
}
