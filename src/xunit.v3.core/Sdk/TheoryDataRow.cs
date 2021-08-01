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

		/// <inheritdoc/>
		public object?[] GetData() => data;
	}
}
