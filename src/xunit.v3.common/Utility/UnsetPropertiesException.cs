using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Xunit.Internal;

namespace Xunit.Sdk;

/// <summary>
/// An exception which indicates an object had several properties that were not properly initialized.
/// </summary>
public class UnsetPropertiesException : InvalidOperationException
{
	/// <summary>
	/// Initializes a new instance of the <see cref="UnsetPropertiesException"/> class.
	/// </summary>
	/// <param name="propertyNames">The properties that were not set</param>
	/// <param name="type">The type that the property belongs to</param>
	public UnsetPropertiesException(
		IEnumerable<string> propertyNames,
		Type type)
	{
		PropertyNames = Guard.ArgumentNotNull(propertyNames).OrderBy(x => x).ToArray();
		TypeName = Guard.ArgumentNotNull(type).SafeName();
	}

	/// <inheritdoc/>
	public override string Message =>
		string.Format(CultureInfo.CurrentCulture, "Object of type '{0}' had one or more properties that were not set: {1}", TypeName, string.Join(", ", PropertyNames));

	/// <summary>
	/// Gets the property names of the uninitialized properties.
	/// </summary>
	public string[] PropertyNames { get; }

	/// <summary>
	/// Gets the type name of the uninitialized property.
	/// </summary>
	public string TypeName { get; }
}
