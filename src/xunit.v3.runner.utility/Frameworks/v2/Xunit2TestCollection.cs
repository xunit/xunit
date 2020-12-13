using System;
using Xunit.Abstractions;
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
			V2TestCollection = v2TestCollection;
		}

		/// <inheritdoc/>
		public ITypeInfo? CollectionDefinition => V2TestCollection.CollectionDefinition;

		/// <inheritdoc/>
		public string DisplayName => V2TestCollection.DisplayName;

		/// <inheritdoc/>
		public ITestAssembly TestAssembly => V2TestCollection.TestAssembly;

		/// <inheritdoc/>
		public Guid UniqueID => V2TestCollection.UniqueID;

		/// <summary>
		/// Gets the underlying xUnit.net v2 <see cref="ITestCollection"/> that this class is wrapping.
		/// </summary>
		public ITestCollection V2TestCollection { get; }
	}
}
