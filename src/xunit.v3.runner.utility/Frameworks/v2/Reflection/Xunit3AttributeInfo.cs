using System;
using System.Collections.Generic;
using System.Linq;
using Xunit.Abstractions;
using Xunit.Internal;
using Xunit.v3;

namespace Xunit.Runner.v2;

/// <summary>
/// Provides a class which wraps v2 <see cref="IAttributeInfo"/> instances to implement v3 <see cref="_IAttributeInfo"/>.
/// </summary>
public class Xunit3AttributeInfo : _IAttributeInfo
{
	/// <summary>
	/// Initializes a new instance of the <see cref="Xunit3AttributeInfo"/> class.
	/// </summary>
	/// <param name="v2AttributeInfo">The v2 attribute info to wrap.</param>
	public Xunit3AttributeInfo(IAttributeInfo v2AttributeInfo)
	{
		V2AttributeInfo = Guard.ArgumentNotNull(v2AttributeInfo);
	}

#pragma warning disable CA1065 // This is satisfying a contract

	/// <inheritdoc/>
	public _ITypeInfo AttributeType =>
		throw new NotImplementedException("This is only available on v3 attributes");

#pragma warning restore CA1065

	/// <summary>
	/// Gets the underlying xUnit.net v2 <see cref="IAttributeInfo"/> that this class is wrapping.
	/// </summary>
	public IAttributeInfo V2AttributeInfo { get; }

	/// <inheritdoc/>
	public IReadOnlyCollection<object?> GetConstructorArguments() =>
		V2AttributeInfo.GetConstructorArguments().CastOrToReadOnlyCollection();

	/// <inheritdoc/>
	public IReadOnlyCollection<_IAttributeInfo> GetCustomAttributes(string assemblyQualifiedAttributeTypeName) =>
		V2AttributeInfo.GetCustomAttributes(assemblyQualifiedAttributeTypeName).Select(a => new Xunit3AttributeInfo(a)).CastOrToReadOnlyCollection();

	/// <inheritdoc/>
	public TValue GetNamedArgument<TValue>(string argumentName) =>
		V2AttributeInfo.GetNamedArgument<TValue>(argumentName);
}
