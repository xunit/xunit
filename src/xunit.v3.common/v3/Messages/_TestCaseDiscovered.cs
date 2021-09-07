﻿using System;
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
		string? testClassName;
		string? testClassNameWithNamespace;
		IReadOnlyDictionary<string, IReadOnlyList<string>> traits = new Dictionary<string, IReadOnlyList<string>>();

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

		/// <inheritdoc/>
		[NotNullIfNotNull(nameof(TestMethodName))]
		public string? TestClassName
		{
			get
			{
				if (testClassName == null && TestMethodName != null)
					throw new InvalidOperationException($"Illegal null {nameof(TestClassName)} on an instance of '{GetType().FullName}' when {nameof(TestMethodName)} is not null");

				return testClassName;
			}
			set => testClassName = value;
		}

		/// <inheritdoc/>
		public string? TestClassNamespace { get; set; }

		/// <inheritdoc/>
		[NotNullIfNotNull(nameof(TestClassName))]
		public string? TestClassNameWithNamespace
		{
			get
			{
				if (testClassNameWithNamespace == null && testClassName != null)
					throw new InvalidOperationException($"Illegal null {nameof(TestClassNameWithNamespace)} on an instance of '{GetType().FullName}' when {nameof(TestClassName)} is not null");

				return testClassNameWithNamespace;
			}
			set => testClassNameWithNamespace = value;
		}

		/// <inheritdoc/>
		public string? TestMethodName { get; set; }

		/// <inheritdoc/>
		public IReadOnlyDictionary<string, IReadOnlyList<string>> Traits
		{
			get => traits;
			set => traits = value ?? new Dictionary<string, IReadOnlyList<string>>();
		}

		/// <inheritdoc/>
		public override string ToString() =>
			$"{base.ToString()} name={testCaseDisplayName.Quoted()}";
	}
}
