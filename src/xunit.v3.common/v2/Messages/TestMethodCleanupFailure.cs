using System;
using System.Collections.Generic;
using Xunit.Abstractions;
using Xunit.Internal;

namespace Xunit.Runner.v2
{
	/// <summary>
	/// Default implementation of <see cref="ITestMethodCleanupFailure"/>.
	/// </summary>
	public class TestMethodCleanupFailure : TestMethodMessage, ITestMethodCleanupFailure
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="TestMethodCleanupFailure"/> class.
		/// </summary>
		public TestMethodCleanupFailure(
			IEnumerable<ITestCase> testCases,
			ITestMethod testMethod,
			string?[] exceptionTypes,
			string[] messages,
			string?[] stackTraces,
			int[] exceptionParentIndices)
				: base(testCases, testMethod)
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
		/// Initializes a new instance of the <see cref="TestMethodCleanupFailure"/> class.
		/// </summary>
		public TestMethodCleanupFailure(
			IEnumerable<ITestCase> testCases,
			ITestMethod testMethod,
			Exception ex)
				: base(testCases, testMethod)
		{
			Guard.ArgumentNotNull(nameof(ex), ex);

			var failureInfo = ExceptionUtility.ConvertExceptionToFailureInformation(ex);
			ExceptionTypes = failureInfo.ExceptionTypes;
			Messages = failureInfo.Messages;
			StackTraces = failureInfo.StackTraces;
			ExceptionParentIndices = failureInfo.ExceptionParentIndices;
		}

		/// <inheritdoc/>
		public string?[] ExceptionTypes { get; }

		/// <inheritdoc/>
		public string[] Messages { get; }

		/// <inheritdoc/>
		public string?[] StackTraces { get; }

		/// <inheritdoc/>
		public int[] ExceptionParentIndices { get; }
	}
}
