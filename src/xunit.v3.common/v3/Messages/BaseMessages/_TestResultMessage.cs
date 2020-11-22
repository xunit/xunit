using System;
using Xunit.Internal;

namespace Xunit.v3
{
	/// <summary>
	/// This is the base message for all individual test results (e.g., tests which
	/// pass, fail, or are skipped).
	/// </summary>
	public class _TestResultMessage : _TestMessage, _IExecutionMetadata
	{
		decimal? executionTime;
		string? output;

		/// <inheritdoc/>
		public decimal ExecutionTime
		{
			get => executionTime ?? throw new InvalidOperationException($"Attempted to get {nameof(ExecutionTime)} on an uninitialized '{GetType().FullName}' object");
			set => executionTime = value;
		}

		/// <inheritdoc/>
		public string Output
		{
			get => output ?? throw new InvalidOperationException($"Attempted to get {nameof(Output)} on an uninitialized '{GetType().FullName}' object");
			set => output = Guard.ArgumentNotNull(nameof(Output), value);
		}
	}
}
