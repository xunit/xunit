using Xunit.Abstractions;
using Xunit.Internal;
using Xunit.Sdk;
using Xunit.v3;

namespace Xunit.Runner.v2
{
	/// <summary>
	/// Provides a class which wraps <see cref="ITestCollection"/> instances to implement <see cref="_ITestCollection"/>.
	/// </summary>
	public class Xunit2TestCollection : _ITestCollection
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Xunit2TestCollection"/> class.
		/// </summary>
		/// <param name="v2TestCollection">The v2 test collection to wrap.</param>
		public Xunit2TestCollection(ITestCollection v2TestCollection)
		{
			V2TestCollection = Guard.ArgumentNotNull(nameof(v2TestCollection), v2TestCollection);

			CollectionDefinition = V2TestCollection.CollectionDefinition == null ? null : new Xunit3TypeInfo(V2TestCollection.CollectionDefinition);
			TestAssembly = new Xunit2TestAssembly(V2TestCollection.TestAssembly);
			UniqueID = UniqueIDGenerator.ForTestCollection(TestAssembly.UniqueID, V2TestCollection.DisplayName, V2TestCollection.CollectionDefinition?.Name);
		}

		/// <inheritdoc/>
		public _ITypeInfo? CollectionDefinition { get; }

		/// <inheritdoc/>
		public string DisplayName => V2TestCollection.DisplayName;

		/// <inheritdoc/>
		public _ITestAssembly TestAssembly { get; }

		/// <inheritdoc/>
		public string UniqueID { get; }

		/// <summary>
		/// Gets the underlying xUnit.net v2 <see cref="ITestCollection"/> that this class is wrapping.
		/// </summary>
		public ITestCollection V2TestCollection { get; }
	}
}
