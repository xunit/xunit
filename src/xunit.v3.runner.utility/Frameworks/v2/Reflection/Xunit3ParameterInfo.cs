using Xunit.Abstractions;
using Xunit.Internal;
using Xunit.v3;

namespace Xunit.Runner.v2;

/// <summary>
/// Provides a class which wraps <see cref="IParameterInfo"/> instances to implement <see cref="_IParameterInfo"/>.
/// </summary>
public class Xunit3ParameterInfo : _IParameterInfo
{
	/// <summary>
	/// Initializes a new instance of the <see cref="Xunit3ParameterInfo"/> class.
	/// </summary>
	/// <param name="v2ParameterInfo">The v2 parameter info to wrap.</param>
	public Xunit3ParameterInfo(IParameterInfo v2ParameterInfo)
	{
		V2ParameterInfo = Guard.ArgumentNotNull(v2ParameterInfo);

		ParameterType = new Xunit3TypeInfo(V2ParameterInfo.ParameterType);
	}

	/// <inheritdoc/>
	public bool IsOptional => false;  // New for v3

	/// <inheritdoc/>
	public string Name => V2ParameterInfo.Name;

	/// <inheritdoc/>
	public _ITypeInfo ParameterType { get; }

	/// <summary>
	/// Gets the underlying xUnit.net v2 <see cref="IParameterInfo"/> that this class is wrapping.
	/// </summary>
	public IParameterInfo V2ParameterInfo { get; }

	/// <inheritdoc/>
	public _IAttributeInfo? GetCustomAttribute(_ITypeInfo attributeType) =>
		null;  // New for v3
}
