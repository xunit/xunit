using System;
using System.Collections.Generic;

#if XUNIT_FRAMEWORK
namespace Xunit.v3
#else
namespace Xunit.Runner.v3
#endif
{
	/// <summary />
	public class _TestCaseMessage : _TestMethodMessage
	{
		string? testCaseDisplayName;
		string? testCaseId;
		Dictionary<string, string[]> traits = new Dictionary<string, string[]>();

		/// <summary />
		public string? SkipReason { get; set; }

		/// <summary />
		public string? SourceFilePath { get; set; }

		/// <summary />
		public int? SourceLineNumber { get; set; }

		/// <summary />
		public string TestCaseDisplayName
		{
			get => testCaseDisplayName ?? throw new InvalidOperationException($"Attempted to get {nameof(TestCaseDisplayName)} on an uninitialized '{GetType().FullName}' object");
			set => testCaseDisplayName = Guard.ArgumentNotNullOrEmpty(nameof(TestCaseDisplayName), value);
		}

		/// <summary />
		public string TestCaseId
		{
			get => testCaseId ?? throw new InvalidOperationException($"Attempted to get {nameof(TestCaseId)} on an uninitialized '{GetType().FullName}' object");
			set => testCaseId = Guard.ArgumentNotNullOrEmpty(nameof(TestCaseId), value);
		}

		/// <summary />
		public Dictionary<string, string[]> Traits
		{
			get => traits;
			set => traits = value ?? new Dictionary<string, string[]>();
		}
	}
}
