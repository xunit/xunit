using Xunit.Abstractions;
using Xunit.Internal;
using Xunit.Sdk;
using Xunit.v3;

namespace Xunit.Runner.v2
{
	/// <summary>
	/// Provides a class which wraps <see cref="ITestMethod"/> instances to implement <see cref="_ITestMethod"/>.
	/// </summary>
	public class Xunit2TestMethod : _ITestMethod
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Xunit2TestMethod"/> class.
		/// </summary>
		/// <param name="v2TestMethod">The v2 test method to wrap.</param>
		public Xunit2TestMethod(ITestMethod v2TestMethod)
		{
			V2TestMethod = Guard.ArgumentNotNull(nameof(v2TestMethod), v2TestMethod);

			Method = new Xunit3MethodInfo(V2TestMethod.Method);
			TestClass = new Xunit2TestClass(V2TestMethod.TestClass);
			UniqueID = UniqueIDGenerator.ForTestMethod(TestClass.UniqueID, V2TestMethod.Method.Name);
		}

		/// <inheritdoc/>
		public _IMethodInfo Method { get; }

		/// <inheritdoc/>
		public _ITestClass TestClass { get; }

		/// <inheritdoc/>
		public string UniqueID { get; }

		/// <summary>
		/// Gets the underlying xUnit.net v2 <see cref="ITestMethod"/> that this class is wrapping.
		/// </summary>
		public ITestMethod V2TestMethod { get; }
	}
}
