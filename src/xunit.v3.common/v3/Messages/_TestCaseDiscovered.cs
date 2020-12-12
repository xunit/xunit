using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Xunit.Internal;

namespace Xunit.v3
{
	/// <summary>
	/// This message indicates that a test case had been found during the discovery process.
	/// </summary>
	public class _TestCaseDiscovered : _TestCaseMessage, _ITestCaseMetadata
	{
		_ITestCase? testCase;
		string? testCaseDisplayName;
		Dictionary<string, List<string>> traits = new Dictionary<string, List<string>>();

		/// <summary>
		/// Gets the serialized value of the test case, which allows it to be transferred across
		/// process boundaries. Will only be available if <see cref="TestOptionsNames.Discovery.IncludeSerialization"/>
		/// is present inside the discovery options when the test case was discovered.
		/// </summary>
		public string? Serialization { get; set; }

		/// <inheritdoc/>
		public string? SkipReason { get; set; }

		/// <inheritdoc/>
		public string? SourceFilePath { get; set; }

		/// <inheritdoc/>
		public int? SourceLineNumber { get; set; }

		/// <summary>
		/// Gets the test case. This cannot cross process boundaries, so runners which
		/// need long-term or cross-process test cases should instead request for serialization
		/// during discovery (via <see cref="M:TestFrameworkOptionsReadWriteExtensions.SetIncludeSerialization"/>),
		/// and stash the <see cref="Serialization"/> to later pass to
		/// <see cref="_ITestFrameworkExecutor.RunTests(IEnumerable{string}, _IMessageSink, _ITestFrameworkExecutionOptions)"/>.
		/// </summary>
		[JsonIgnore]
		public _ITestCase TestCase
		{
			get => testCase ?? throw new InvalidOperationException($"Attempted to get {nameof(TestCase)} on an uninitialized '{GetType().FullName}' object");
			set => testCase = Guard.ArgumentNotNull(nameof(TestCase), value);
		}

		/// <inheritdoc/>
		public string TestCaseDisplayName
		{
			get => testCaseDisplayName ?? throw new InvalidOperationException($"Attempted to get {nameof(TestCaseDisplayName)} on an uninitialized '{GetType().FullName}' object");
			set => testCaseDisplayName = Guard.ArgumentNotNullOrEmpty(nameof(TestCaseDisplayName), value);
		}

		/// <inheritdoc/>
		public Dictionary<string, List<string>> Traits
		{
			get => traits;
			set => traits = value ?? new Dictionary<string, List<string>>();
		}

		IReadOnlyDictionary<string, IReadOnlyList<string>> _ITestCaseMetadata.Traits => traits.ToReadOnly();

		/// <inheritdoc/>
		public override string ToString() =>
			$"{base.ToString()} name={testCaseDisplayName.Quoted()}";
	}
}
