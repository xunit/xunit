using System;
using System.Collections.Generic;
using Xunit.Internal;

namespace Xunit.v3;

public partial class DiscoveryComplete
{
	DateTimeOffset? finishTime;
	int? testCasesToRun;

	/// <inheritdoc/>
	public required DateTimeOffset FinishTime
	{
		get => this.ValidateNullablePropertyValue(finishTime, nameof(FinishTime));
		set => finishTime = value;
	}

	/// <inheritdoc/>
	public required int TestCasesToRun
	{
		get => this.ValidateNullablePropertyValue(testCasesToRun, nameof(TestCasesToRun));
		set => testCasesToRun = value;
	}

	/// <inheritdoc/>
	protected override void ValidateObjectState(HashSet<string> invalidProperties)
	{
		base.ValidateObjectState(invalidProperties);

		ValidatePropertyIsNotNull(finishTime, nameof(FinishTime), invalidProperties);
		ValidatePropertyIsNotNull(testCasesToRun, nameof(TestCasesToRun), invalidProperties);
	}
}
