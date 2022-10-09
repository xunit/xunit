using Xunit.Abstractions;
using Xunit.Internal;
using Xunit.v3;

namespace Xunit.Runner.v2;

/// <summary>
/// Provides a class which wraps <see cref="_IParameterInfo"/> instances to implement <see cref="IParameterInfo"/>.
/// </summary>
public class Xunit2ParameterInfo : LongLivedMarshalByRefObject, IParameterInfo
{
	/// <summary>
	/// Initializes a new instance of the <see cref="Xunit2ParameterInfo"/> class.
	/// </summary>
	/// <param name="v3ParameterInfo">The v3 parameter info to wrap.</param>
	public Xunit2ParameterInfo(_IParameterInfo v3ParameterInfo)
	{
		V3ParameterInfo = Guard.ArgumentNotNull(v3ParameterInfo);

		ParameterType = new Xunit2TypeInfo(V3ParameterInfo.ParameterType);
	}

	/// <inheritdoc/>
	public string Name => V3ParameterInfo.Name;

	/// <inheritdoc/>
	public ITypeInfo ParameterType { get; }

	/// <summary>
	/// Gets the underlying xUnit.net v3 <see cref="_IParameterInfo"/> that this class is wrapping.
	/// </summary>
	public _IParameterInfo V3ParameterInfo { get; }
}
