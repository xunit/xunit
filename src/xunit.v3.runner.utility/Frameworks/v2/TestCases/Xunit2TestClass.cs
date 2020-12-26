using Xunit.Abstractions;
using Xunit.Internal;
using Xunit.Sdk;
using Xunit.v3;

namespace Xunit.Runner.v2
{
	/// <summary>
	/// Provides a class which wraps <see cref="ITestClass"/> instances to implement <see cref="_ITestClass"/>.
	/// </summary>
	public class Xunit2TestClass : _ITestClass
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Xunit2TestClass"/> class.
		/// </summary>
		/// <param name="v2TestClass">The v2 test class to wrap.</param>
		public Xunit2TestClass(ITestClass v2TestClass)
		{
			V2TestClass = Guard.ArgumentNotNull(nameof(v2TestClass), v2TestClass);

			Class = new Xunit3TypeInfo(V2TestClass.Class);
			TestCollection = new Xunit2TestCollection(V2TestClass.TestCollection);
			UniqueID = UniqueIDGenerator.ForTestClass(TestCollection.UniqueID, v2TestClass.Class.Name);
		}

		/// <inheritdoc/>
		public _ITypeInfo Class { get; }

		/// <inheritdoc/>
		public _ITestCollection TestCollection { get; }

		/// <inheritdoc/>
		public string UniqueID { get; }

		/// <summary>
		/// Gets the underlying xUnit.net v2 <see cref="ITestClass"/> that this class is wrapping.
		/// </summary>
		public ITestClass V2TestClass { get; }
	}
}
