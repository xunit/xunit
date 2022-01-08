using System;
using System.Globalization;
using System.Linq;
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
			() => string.Format(CultureInfo.CurrentCulture, "Attribute {0} has been provided without a class type assigned to it.", nameof(ClassDataAttribute)),
			dataAttribute.GetConstructorArguments().FirstOrDefault() as Type
		);

		return !typeof(IDisposable).IsAssignableFrom(@class)
			&& !typeof(IAsyncDisposable).IsAssignableFrom(@class);
	}
}
