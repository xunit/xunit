#nullable enable

#pragma warning disable IDE0290 // Use primary constructor

using System;

namespace Xunit.Generators
{
	/// <summary>
	/// Represents a theory data row factory, used by <see cref="DataAttributeGeneratorResult"/>.
	/// </summary>
	public class TheoryDataRowFactory : IEquatable<TheoryDataRowFactory>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="TheoryDataRowFactory"/> class.
		/// </summary>
		/// <param name="factory">The factory source code</param>
		/// <param name="disableDiscoveryEnumeration">A flag which indicates if discovery enumeration should be skipped</param>
		public TheoryDataRowFactory(
			string factory,
			bool disableDiscoveryEnumeration)
		{
			Factory = factory ?? throw new ArgumentNullException(nameof(factory));
			DisableDiscoveryEnumeration = disableDiscoveryEnumeration;
		}

		/// <summary>
		/// Gets a flag which indicates if discovery enumeration should be skipped
		/// </summary>
		public bool DisableDiscoveryEnumeration { get; }

		/// <summary>
		/// Gets the factory source code
		/// </summary>
		public string Factory { get; }

		/// <inheritdoc/>
		public override bool Equals(object? obj) =>
			Equals(obj as TheoryDataRowFactory);

		/// <inheritdoc/>
		public bool Equals(TheoryDataRowFactory? other) =>
			other != null &&
			ComparerHelper.Equal(DisableDiscoveryEnumeration, other.DisableDiscoveryEnumeration) &&
			ComparerHelper.Equal(Factory, other.Factory);

		/// <inheritdoc/>
		public override int GetHashCode() =>
			HashCodeHelper.Start()
				.With(DisableDiscoveryEnumeration)
				.With(Factory);
	}
}
