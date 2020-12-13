using System;
using Xunit.Abstractions;
using Xunit.Internal;
using Xunit.v3;

namespace Xunit.Runner.v2
{
	/// <summary>
	/// Provides a class which wraps <see cref="ITestMethod"/> instances to implement <see cref="_ITestMethod"/>.
	/// </summary>
	public class Xunit2TestMethod : _ITestMethod
	{
		Lazy<_ITestClass> testClass;

		/// <summary>
		/// Initializes a new instance of the <see cref="Xunit2TestMethod"/> class.
		/// </summary>
		/// <param name="v2TestMethod">The v2 test method to wrap.</param>
		public Xunit2TestMethod(ITestMethod v2TestMethod)
		{
			V2TestMethod = Guard.ArgumentNotNull(nameof(v2TestMethod), v2TestMethod);

			testClass = new Lazy<_ITestClass>(() => new Xunit2TestClass(V2TestMethod.TestClass));
		}

		/// <inheritdoc/>
		public IMethodInfo Method => V2TestMethod.Method;

		/// <inheritdoc/>
		public _ITestClass TestClass => testClass.Value;

		/// <summary>
		/// Gets the underlying xUnit.net v2 <see cref="ITestMethod"/> that this class is wrapping.
		/// </summary>
		public ITestMethod V2TestMethod { get; }
	}
}
