using System;
using System.Collections.Generic;
using Xunit.Abstractions;
using Xunit.Internal;

namespace Xunit.Runner.v2
{
	/// <summary>
	/// Default implementation of <see cref="TestAssemblyCleanupFailure"/>.
	/// </summary>
	public class TestAssemblyCleanupFailure : TestAssemblyMessage, ITestAssemblyCleanupFailure
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="TestAssemblyCleanupFailure"/> class.
		/// </summary>
		public TestAssemblyCleanupFailure(
			IEnumerable<ITestCase> testCases,
			ITestAssembly testAssembly,
			string?[] exceptionTypes,
			string[] messages,
			string?[] stackTraces,
			int[] exceptionParentIndices)
				: base(testCases, testAssembly)
		{
			Guard.ArgumentNotNull(nameof(exceptionTypes), exceptionTypes);
			Guard.ArgumentNotNull(nameof(messages), messages);
			Guard.ArgumentNotNull(nameof(stackTraces), stackTraces);
			Guard.ArgumentNotNull(nameof(exceptionParentIndices), exceptionParentIndices);

			StackTraces = stackTraces;
			Messages = messages;
			ExceptionTypes = exceptionTypes;
			ExceptionParentIndices = exceptionParentIndices;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="TestAssemblyCleanupFailure"/> class.
		/// </summary>
		public TestAssemblyCleanupFailure(
			IEnumerable<ITestCase> testCases,
			ITestAssembly testAssembly,
			Exception ex)
				: base(testCases, testAssembly)
		{
			Guard.ArgumentNotNull(nameof(ex), ex);

			var failureInfo = ExceptionUtility.ConvertExceptionToFailureInformation(ex);
			ExceptionTypes = failureInfo.ExceptionTypes;
			Messages = failureInfo.Messages;
			StackTraces = failureInfo.StackTraces;
			ExceptionParentIndices = failureInfo.ExceptionParentIndices;
		}

		/// <inheritdoc/>
		public int[] ExceptionParentIndices { get; }

		/// <inheritdoc/>
		public string?[] ExceptionTypes { get; }

		/// <inheritdoc/>
		public string[] Messages { get; }

		/// <inheritdoc/>
		public string?[] StackTraces { get; }
	}
}
