using Xunit.Abstractions;
using Xunit.Internal;
using Xunit.v3;

namespace Xunit.Runner.v2
{
	/// <summary>
	/// An implementation of xUnit.net v2's <see cref="ITestFrameworkDiscoveryOptions"/> and
	/// <see cref="ITestFrameworkExecutionOptions"/>, which delegates calls to an xUnit.net v3
	/// implementation of <see cref="_ITestFrameworkOptions"/>.
	/// </summary>
	public class Xunit2Options : LongLivedMarshalByRefObject, ITestFrameworkDiscoveryOptions, ITestFrameworkExecutionOptions
	{
		private readonly _ITestFrameworkOptions v3Options;

		/// <summary>
		/// Initializes a new instance of the <see cref="Xunit2Options"/> class.
		/// </summary>
		/// <param name="v3Options">The v3 options object to delegate all the calls to.</param>
		public Xunit2Options(_ITestFrameworkOptions v3Options)
		{
			this.v3Options = Guard.ArgumentNotNull(nameof(v3Options), v3Options);
		}

		/// <inheritdoc/>
		public TValue? GetValue<TValue>(string name) =>
			v3Options.GetValue<TValue>(name);

		/// <inheritdoc/>
		public void SetValue<TValue>(
			string name,
			TValue value) =>
				v3Options.SetValue<TValue>(name, value);
	}
}
