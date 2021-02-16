using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Xunit.Internal;

namespace Xunit.v3
{
	/// <summary>
	/// This message indicates that a test case had been found during the discovery process.
	/// </summary>
	public class _TestCaseDiscovered : _TestCaseMessage, _ITestCaseMetadata
	{
		string? serialization;
		string? testCaseDisplayName;
		Dictionary<string, List<string>> traits = new Dictionary<string, List<string>>();

		/// <summary>
		/// Gets the serialized value of the test case, which allows it to be transferred across
		/// process boundaries.
		/// </summary>
		public string Serialization
		{
			get => serialization ?? throw new InvalidOperationException($"Attempted to get {nameof(Serialization)} on an uninitialized '{GetType().FullName}' object");
			set => serialization = Guard.ArgumentNotNull(nameof(Serialization), value);
		}

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

		/// <summary>
		/// Gets the name of the class where the test is defined. If the test did not originiate
		/// in a class, will return <c>null</c>.
		/// </summary>
		[NotNullIfNotNull(nameof(TestMethod))]
		public string? TestClass { get; set; }

		/// <summary>
		/// Gets the fully qualified type name (without assembly) of the class where the test is defined.
		/// If the test did not originiate in a class, will return <c>null</c>.
		/// </summary>
		[NotNullIfNotNull(nameof(TestClass))]
		public string? TestClassWithNamespace { get; set; }

		/// <summary>
		/// Gets the method name where the test is defined, in the <see cref="TestClass"/> class.
		/// If the test did not originiate in a method, will return <c>null</c>.
		/// </summary>
		public string? TestMethod { get; set; }

		/// <summary>
		/// Gets the namespace of the class where the test is defined. If the test did not
		/// originate in a class, or the class it originated in does not reside in a namespace,
		/// will return <c>null</c>.
		/// </summary>
		public string? TestNamespace { get; set; }

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
