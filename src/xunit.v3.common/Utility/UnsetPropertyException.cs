using System;
using Xunit.Internal;

namespace Xunit.Sdk;

/// <summary>
/// An exception which indicates an object was not properly initialized, thrown by a property
/// getter that was accessed by the uninitialized object.
/// </summary>
public class UnsetPropertyException : InvalidOperationException
{
	/// <summary>
	/// Initializes a new instance of the <see cref="UnsetPropertyException"/> class.
	/// </summary>
	/// <param name="propertyName"></param>
	/// <param name="type"></param>
	public UnsetPropertyException(
		string propertyName,
		Type type)
	{
		PropertyName = Guard.ArgumentNotNull(propertyName);
		TypeName = Guard.ArgumentNotNull(type).SafeName();
	}

	/// <inheritdoc/>
	public override string Message =>
		$"Attempted to get '{PropertyName}' on an uninitialized '{TypeName}' object";

	/// <summary>
	/// Gets the property name of the uninitialized property.
	/// </summary>
	public string PropertyName { get; }

	/// <summary>
	/// Gets the type name of the uninitialized property.
	/// </summary>
	public string TypeName { get; }
}
