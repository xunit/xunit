using System.Collections.Generic;
using Xunit.Abstractions;

namespace Xunit.Runner.v2
{
	/// <summary>
	/// Default implementation of <see cref="ITestClassMessage"/>.
	/// </summary>
	public class TestClassMessage : TestCollectionMessage, ITestClassMessage
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="TestClassMessage"/> class.
		/// </summary>
		public TestClassMessage(
			IEnumerable<ITestCase> testCases,
			ITestClass testClass)
				: base(testCases, testClass.TestCollection)
		{
			TestClass = testClass;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="TestClassMessage"/> class.
		/// </summary>
		internal TestClassMessage(
			ITestCase testCase,
			ITestClass testClass)
				: base(testCase, testClass.TestCollection)
		{
			TestClass = testClass;
		}

		/// <inheritdoc/>
		public ITestClass TestClass { get; }
	}
}
