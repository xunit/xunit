using Xunit.Internal;
using Xunit.Sdk;
using Xunit.v3;

namespace Xunit.Runner.v1
{
	/// <summary>
	/// Implementation of <see cref="_ITestCollection"/> for xUnit.net v1 test collections.
	/// </summary>
	public class Xunit1TestCollection : _ITestCollection
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Xunit1TestCollection"/> class.
		/// </summary>
		/// <param name="testAssembly">The test assembly this test collection belongs to.</param>
		public Xunit1TestCollection(Xunit1TestAssembly testAssembly)
		{
			TestAssembly = Guard.ArgumentNotNull(nameof(testAssembly), testAssembly);
			UniqueID = UniqueIDGenerator.ForTestCollection(TestAssembly.UniqueID, DisplayName, null);
		}

		/// <inheritdoc/>
		public _ITypeInfo? CollectionDefinition => null;

		/// <inheritdoc/>
		public string DisplayName => $"xUnit.net v1 Tests for {TestAssembly.Assembly.AssemblyPath}";

		/// <inheritdoc/>
		public Xunit1TestAssembly TestAssembly { get; }

		/// <inheritdoc/>
		public string UniqueID { get; }

		_ITestAssembly _ITestCollection.TestAssembly => TestAssembly;
	}
}
