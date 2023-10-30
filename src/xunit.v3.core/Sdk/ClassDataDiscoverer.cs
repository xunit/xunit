using System;
using Xunit.Internal;
using Xunit.v3;

namespace Xunit.Sdk;

/// <summary>
/// Implementation of <see cref="IDataDiscoverer"/> for discovering <see cref="ClassDataAttribute"/>.
/// </summary>
public class ClassDataDiscoverer : DataDiscoverer
{
	/// <inheritdoc/>
	public override bool SupportsDiscoveryEnumeration(
		_IAttributeInfo dataAttribute,
		_IMethodInfo testMethod)
	{
		Guard.ArgumentNotNull(dataAttribute);
		Guard.ArgumentNotNull(testMethod);

		var @class = Guard.NotNull(
			() => $"Attribute {nameof(ClassDataAttribute)} has been provided without a class type assigned to it.",
			dataAttribute.GetNamedArgument<Type>(nameof(ClassDataAttribute.Class))
		);

		return !typeof(IDisposable).IsAssignableFrom(@class)
			&& !typeof(IAsyncDisposable).IsAssignableFrom(@class);
	}
}
