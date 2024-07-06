using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.Runner.v2;

/// <summary>
/// An implementation of xUnit.net v2's <see cref="Abstractions.ITestFrameworkDiscoveryOptions"/> and
/// <see cref="Abstractions.ITestFrameworkExecutionOptions"/>, which delegates calls to an xUnit.net v3
/// implementation of <see cref="ITestFrameworkOptions"/>.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="Xunit2Options"/> class.
/// </remarks>
/// <param name="v3Options">The v3 options object to delegate all the calls to.</param>
public class Xunit2Options(ITestFrameworkOptions v3Options) :
	LongLivedMarshalByRefObject, Abstractions.ITestFrameworkDiscoveryOptions, Abstractions.ITestFrameworkExecutionOptions
{
	private readonly ITestFrameworkOptions v3Options = Guard.ArgumentNotNull(v3Options);

	/// <inheritdoc/>
	public TValue? GetValue<TValue>(string name) =>
		v3Options.GetValue<TValue>(name);

	/// <inheritdoc/>
	public void SetValue<TValue>(
		string name,
		TValue value) =>
			v3Options.SetValue(name, value);
}
