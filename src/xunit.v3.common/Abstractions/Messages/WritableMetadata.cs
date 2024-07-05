namespace Xunit.Internal;

// The interfaces in this file need to be kept up to date with the read-only counterparts.

/// <summary>
/// INTERNAL INTERFACE. DO NOT USE.
/// </summary>
public interface IWritableExecutionSummaryMetadata
{
	/// <summary/>
	decimal ExecutionTime { get; set; }
	/// <summary/>
	int TestsFailed { get; set; }
	/// <summary/>
	int TestsNotRun { get; set; }
	/// <summary/>
	int TestsSkipped { get; set; }
	/// <summary/>
	int TestsTotal { get; set; }
}

/// <summary>
/// INTERNAL INTERFACE. DO NOT USE.
/// </summary>
public interface IWritableErrorMetadata
{
	/// <summary/>
	int[] ExceptionParentIndices { get; set; }
	/// <summary/>
	string?[] ExceptionTypes { get; set; }
	/// <summary/>
	string[] Messages { get; set; }
	/// <summary/>
	string?[] StackTraces { get; set; }
}
