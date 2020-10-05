using System.Collections.Generic;
using Xunit.Abstractions;

namespace Xunit.Runner.v2
{
	/// <summary>
	/// Default implementation of <see cref="ITestMethodMessage"/>.
	/// </summary>
	public class TestMethodMessage : TestClassMessage, ITestMethodMessage
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="TestMethodMessage"/> class.
		/// </summary>
		public TestMethodMessage(
			IEnumerable<ITestCase> testCases,
			ITestMethod testMethod)
				: base(testCases, testMethod.TestClass)
		{
			TestMethod = testMethod;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="TestMethodMessage"/> class.
		/// </summary>
		internal TestMethodMessage(
			ITestCase testCase,
			ITestMethod testMethod)
				: base(testCase, testMethod.TestClass)
		{
			TestMethod = testMethod;
		}

		/// <inheritdoc/>
		public ITestMethod TestMethod { get; }
	}
}
