using System.Collections.Generic;
using Xunit.Internal;

namespace Xunit.v3;

public partial class DiscoveryComplete
{
	int? testCasesToRun;

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

		ValidatePropertyIsNotNull(testCasesToRun, nameof(TestCasesToRun), invalidProperties);
	}
}
