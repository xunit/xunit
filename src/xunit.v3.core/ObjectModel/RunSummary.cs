using System.Text;

namespace Xunit.v3;

/// <summary>
/// Represents the statistical summary from a run of one or more tests.
/// </summary>
public struct RunSummary
{
	// Kept as long milliseconds so we can use Interlocked for thread safety
	long longTime;

	/// <summary>
	/// The total number of tests run.
	/// </summary>
	public int Total;

	/// <summary>
	/// The number of failed tests.
	/// </summary>
	public int Failed;

	/// <summary>
	/// The number of skipped tests.
	/// </summary>
	public int Skipped;

	/// <summary>
	/// The number of tests that were not run.
	/// </summary>
	public int NotRun;

	/// <summary>
	/// The total time taken to run the tests, in seconds.
	/// </summary>
	public decimal Time
	{
		get => longTime / 1000m;
		set => longTime = (long)(value * 1000);
	}

	/// <summary>
	/// Adds a run summary's totals into this run summary.
	/// </summary>
	/// <param name="other">The run summary to be added.</param>
	public void Aggregate(RunSummary other)
	{
		Interlocked.Add(ref Total, other.Total);
		Interlocked.Add(ref Failed, other.Failed);
		Interlocked.Add(ref Skipped, other.Skipped);
		Interlocked.Add(ref NotRun, other.NotRun);
		Interlocked.Add(ref longTime, other.longTime);
	}

	/// <inheritdoc/>
	public override string ToString()
	{
		var result = new StringBuilder();

		result.AppendFormat(CultureInfo.CurrentCulture, "{{ Total = {0}", Total);

		if (Failed != 0)
			result.AppendFormat(CultureInfo.CurrentCulture, ", Failed = {0}", Failed);
		if (Skipped != 0)
			result.AppendFormat(CultureInfo.CurrentCulture, ", Skipped = {0}", Skipped);
		if (NotRun != 0)
			result.AppendFormat(CultureInfo.CurrentCulture, ", NotRun = {0}", NotRun);
		if (Time != 0m)
			result.AppendFormat(CultureInfo.CurrentCulture, ", Time = {0}", Time);

		result.Append(" }");
		return result.ToString();
	}
}
