using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using Xunit.Internal;
using Xunit.Sdk;

#if XUNIT_RUNNER_COMMON
namespace Xunit.Runner.Common;
#else
namespace Xunit.v3;
#endif

abstract partial class MessageSinkMessage : IMessageSinkMessage
{
	readonly string? type;
	static readonly ConcurrentDictionary<Type, string?> typeToTypeIDMappings = new();

	/// <summary>
	/// Initializes a new instance of the see <see cref="MessageSinkMessage"/> class.
	/// </summary>
	protected MessageSinkMessage() =>
		type = typeToTypeIDMappings.GetOrAdd(GetType(), t => t.GetCustomAttribute<JsonTypeIDAttribute>()?.ID);

	/// <summary>
	/// Override to serialize the values in the message into JSON.
	/// </summary>
	/// <param name="serializer">The serializer to write values to.</param>
	protected abstract void Serialize(JsonObjectSerializer serializer);

	/// <summary>
	/// Converts a string array into a display value, for use in an <see cref="object.ToString"/> overload.
	/// </summary>
	protected static string ToDisplayString(string?[]? array) =>
		array is null
			? "null"
			: "[" + string.Join(", ", array.Select(v => v.QuotedWithTrim(25))) + "]";

	/// <summary>
	/// Converts am array into a display value, for use in an <see cref="object.ToString"/> overload.
	/// </summary>
	protected static string ToDisplayString<T>(T[]? array) =>
		array is null
			? "null"
			: "[" + string.Join(", ", array.Select(v => v?.ToString() ?? "null")) + "]";

	/// <summary>
	/// Creates a JSON serialized version of this message.
	/// </summary>
	/// <exception cref="InvalidOperationException">Thrown if the message class does not have a <see cref="JsonTypeIDAttribute"/>.</exception>
	/// <exception cref="UnsetPropertiesException">Thrown when one or more properties are missing values.</exception>
	public string ToJson()
	{
		if (type is null)
			throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "Message sink message type '{0}' is missing its [JsonTypeID] decoration", GetType().SafeName()));

		ValidateObjectState();

		var buffer = new StringBuilder();
		using (var serializer = new JsonObjectSerializer(buffer))
		{
			serializer.Serialize("$type", type);
			Serialize(serializer);
		}

		return buffer.ToString();
	}
}
