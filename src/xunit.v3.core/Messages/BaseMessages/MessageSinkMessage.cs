using System.Collections.Generic;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Default implementation of <see cref="IMessageSinkMessage"/>, with serialization support.
/// </summary>
/// <remarks>
/// Because of serialization, all concrete message sink message types must be decorated
/// with <see cref="JsonTypeIDAttribute"/> to set a unique type ID for serialization purposes.
/// </remarks>
public partial class MessageSinkMessage
{
	/// <summary>
	/// Validates the state of the message object. This should be called just before serializing the message
	/// or just after deserializing the message to ensure that the message is not missing any required
	/// property values.
	/// </summary>
	/// <exception cref="UnsetPropertiesException">Throw when one or more properties are missing values.</exception>
	public void ValidateObjectState()
	{
		var invalidProperties = new HashSet<string>();

		ValidateObjectState(invalidProperties);

		if (invalidProperties.Count != 0)
			throw new UnsetPropertiesException(invalidProperties, GetType());
	}

	/// <summary>
	/// Called before serializing the message. Implementers are expected to call <see cref="ValidatePropertyIsNotNull"/>
	/// for each property that must have a value, to record invalid property values into the provided hash set.
	/// </summary>
	/// <param name="invalidProperties">The hash set to record invalid properties into</param>
	protected abstract void ValidateObjectState(HashSet<string> invalidProperties);

	/// <summary>
	/// Validates that the property value is not <c>null</c>, and if it is, adds the given
	/// property name to the invalid property hash set.
	/// </summary>
	/// <param name="propertyValue">The property value</param>
	/// <param name="propertyName">The property name</param>
	/// <param name="invalidProperties">The hash set to contain the invalid property name list</param>
	protected static void ValidatePropertyIsNotNull(
		object? propertyValue,
		string propertyName,
		HashSet<string> invalidProperties)
	{
		Guard.ArgumentNotNull(invalidProperties);

		if (propertyValue is null)
			invalidProperties.Add(propertyName);
	}
}
