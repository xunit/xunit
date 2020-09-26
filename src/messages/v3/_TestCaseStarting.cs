using System;
using System.Collections.Generic;

#if XUNIT_FRAMEWORK
namespace Xunit.v3
#else
namespace Xunit.Runner.v3
#endif
{
	/// <summary>
	/// This message indicates that a test case is about to start executing.
	/// </summary>
	public class _TestCaseStarting : _TestCaseMessage, _ITestCaseMetadata
	{
		string? testCaseDisplayName;
		Dictionary<string, string[]> traits = new Dictionary<string, string[]>();

		/// <inheritdoc/>
		public string? SkipReason { get; set; }

		/// <inheritdoc/>
		public string? SourceFilePath { get; set; }

		/// <inheritdoc/>
		public int? SourceLineNumber { get; set; }

		/// <inheritdoc/>
		public string TestCaseDisplayName
		{
			get => testCaseDisplayName ?? throw new InvalidOperationException($"Attempted to get {nameof(TestCaseDisplayName)} on an uninitialized '{GetType().FullName}' object");
			set => testCaseDisplayName = Guard.ArgumentNotNullOrEmpty(nameof(TestCaseDisplayName), value);
		}

		/// <inheritdoc/>
		public Dictionary<string, string[]> Traits
		{
			get => traits;
			set => traits = value ?? new Dictionary<string, string[]>();
		}

		IReadOnlyDictionary<string, string[]> _ITestCaseMetadata.Traits => traits;
	}
}
