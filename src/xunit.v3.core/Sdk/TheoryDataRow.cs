using System.Collections.Generic;
using Xunit.Internal;

namespace Xunit.Sdk
{
	/// <summary>
	/// Default implementation of <see cref="ITheoryDataRow"/> which accepts pre-enumerated data.
	/// </summary>
	public class TheoryDataRow : ITheoryDataRow
	{
		object?[] data;

		/// <summary>
		/// Initializes a new instance of the <see cref="TheoryDataRow"/> class, encapsulating the
		/// provided theory data.
		/// </summary>
		/// <param name="data">The data for the theory row</param>
		public TheoryDataRow(params object?[] data)
		{
			this.data = Guard.ArgumentNotNull(nameof(data), data);
		}

		/// <inheritdoc/>
		public string? Skip { get; set; }

		/// <summary>
		/// Gets or sets the traits for the theory data row.
		/// </summary>
		public Dictionary<string, List<string>> Traits { get; set; } = new();

		/// <summary>
		/// Adds a single trait name/value pair to the theory data row.
		/// </summary>
		/// <param name="name">The name of the trait</param>
		/// <param name="value">The value of the trait</param>
		public TheoryDataRow WithTrait(string name, string value)
		{
			var list = Traits.GetOrAdd(name, () => new());
			list.Add(value);
			return this;
		}

		/// <inheritdoc/>
		public object?[] GetData() => data;
	}
}
