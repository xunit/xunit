using System;
using System.Globalization;
using Xunit.Internal;

namespace Xunit.Sdk;

/// <summary>
/// An exception which indicates an object was not properly initialized, thrown by a property
/// getter that was accessed by the uninitialized object.
/// </summary>
/// <param name="propertyName">The property that was not set</param>
/// <param name="type">The type that the property belongs to</param>
public class UnsetPropertyException(
	string propertyName,
	Type type) :
		InvalidOperationException
{
	/// <inheritdoc/>
	public override string Message =>
		string.Format(CultureInfo.CurrentCulture, "Attempted to get '{0}' on an uninitialized '{1}' object", PropertyName, TypeName);

	/// <summary>
	/// Gets the property name of the uninitialized property.
	/// </summary>
	public string PropertyName { get; } = Guard.ArgumentNotNull(propertyName);

	/// <summary>
	/// Gets the type name of the uninitialized property.
	/// </summary>
	public string TypeName { get; } = Guard.ArgumentNotNull(type).SafeName();
}
