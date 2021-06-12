using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit.Internal;

namespace Xunit.v3
{
	/// <summary>
	/// The base type for all messages. It provides serialization support. Because of the way type
	/// serialization works, messages can only be defined by xUnit.net itself.
	/// </summary>
	public class _MessageSinkMessage
	{
		delegate void PropertyWriter(Utf8JsonWriter writer, object? value, JsonSerializerOptions options);

		static string assemblyQualifiedNameTemplate;
		static JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions { Converters = { new JsonStringEnumConverter() } };
		static Dictionary<Type, PropertyWriter> propertyWriters = new Dictionary<Type, PropertyWriter>();

		static _MessageSinkMessage()
		{
			var assemblyQualifiedName = typeof(_MessageSinkMessage).AssemblyQualifiedName;
			if (assemblyQualifiedName == null)
				throw new InvalidOperationException($"Could not get AssemblyQualifiedName for {typeof(_MessageSinkMessage).FullName ?? typeof(_MessageSinkMessage).Name}");

			assemblyQualifiedNameTemplate = assemblyQualifiedName.Replace(nameof(_MessageSinkMessage), "{0}");

			// Default converters list doesn't support dictionaries, so we need to use this as an override
			propertyWriters.Add(typeof(Dictionary<string, List<string>>), SerializeTraits);
		}

		/// <summary>
		/// Parses a previously serialized <see cref="_MessageSinkMessage"/>-derived object.
		/// </summary>
		/// <param name="serialization">The serialized value</param>
		/// <returns>The deserialized object</returns>
		public static _MessageSinkMessage ParseJson(ReadOnlyMemory<byte> serialization)
		{
			var byteSpan = serialization.Span;
			var reader = new Utf8JsonReader(byteSpan);

			reader.Read();
			if (reader.TokenType != JsonTokenType.StartObject)
				throw new ArgumentException($"Expected object serialization, got {reader.TokenType} instead", nameof(serialization));

			reader.Read();
			if (reader.TokenType != JsonTokenType.PropertyName)
				throw new ArgumentException($"Expected first element to be a property, got {reader.TokenType} instead", nameof(serialization));

			var propertyName = reader.GetString();
			if (propertyName != "$type")
				throw new ArgumentException($"Expected first property to be named $type, got '{propertyName}' instead", nameof(serialization));

			reader.Read();
			if (reader.TokenType != JsonTokenType.String)
				throw new ArgumentException($"Expected $type to be a string, got {reader.TokenType} instead", nameof(serialization));

			var shortTypeName = reader.GetString();
			var typeName = string.Format(assemblyQualifiedNameTemplate, shortTypeName);
			var type = Type.GetType(typeName);
			if (type == null)
				throw new ArgumentException($"Could not load type '{typeName}' for deserialization", nameof(serialization));

			var result = JsonSerializer.Deserialize(byteSpan, type);
			if (result == null)
				throw new ArgumentException($"Deserialization of type '{typeName}' unexpectedly returned null", nameof(serialization));

			var typedResult = result as _MessageSinkMessage;
			if (typedResult == null)
				throw new ArgumentException($"Deserialization of type '{typeName}' returned a value of type '{result.GetType().FullName}' instead of something derived from '{typeof(_MessageSinkMessage).FullName}'", nameof(serialization));

			return typedResult;
		}

		/// <summary>
		/// Creates a JSON serialized version of this message. Can be re-hydrated using <see cref="ParseJson"/>.
		/// </summary>
		/// <returns>The serialization of this message</returns>
		public byte[] ToJson()
		{
			using var stream = new MemoryStream();
			using (var writer = new Utf8JsonWriter(stream))
			{
				var valueType = GetType();

				writer.WriteStartObject();
				writer.WriteString("$type", valueType.Name);

				// Only serializing the public read/write properties without [JsonIgnore]
				var properties =
					valueType
						.GetProperties()
						.Where(p => p.CanRead && p.CanWrite && p.GetCustomAttributes(typeof(JsonIgnoreAttribute), true).Length == 0)
						.OrderBy(p => p.Name, StringComparer.OrdinalIgnoreCase);

				foreach (var property in properties)
				{
					var propValue = property.GetValue(this);

					// Skip writing nulls, makes the payload smaller/faster
					if (propValue != null)
					{
						var propWriter = GetPropertyWriter(property.PropertyType);
						writer.WritePropertyName(property.Name);
						propWriter(writer, propValue, jsonSerializerOptions);
					}
				}

				writer.WriteEndObject();
			}

			stream.Seek(0, SeekOrigin.Begin);
			return stream.ToArray();
		}

		static void SerializeTraits(Utf8JsonWriter writer, object? value, JsonSerializerOptions options)
		{
			var traits = (Dictionary<string, List<string>>)value!;

			writer.WriteStartObject();

			foreach (var trait in traits)
			{
				writer.WritePropertyName(trait.Key);
				writer.WriteStartArray();

				foreach (var traitValue in trait.Value)
					writer.WriteStringValue(traitValue);

				writer.WriteEndArray();
			}

			writer.WriteEndObject();
		}

		static PropertyWriter GetPropertyWriter(Type propertyType)
		{
			// We don't write null values, and the converters don't seem to support nullable anyway,
			// so we request a converter for the non-nullable type instead
			if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
				propertyType = propertyType.GetGenericArguments()[0];

			return propertyWriters.GetOrAdd(propertyType, () =>
			{
				var converter = jsonSerializerOptions.GetConverter(propertyType);
				if (converter == null)
					throw new InvalidOperationException($"Could not find serializer for {propertyType.FullName}");

				var writeMethod =
					converter
						.GetType()
						.GetMethods()
						.Where(mi =>
						{
							if (mi.Name != "Write")
								return false;
							var parameters = mi.GetParameters();
							return parameters.Length == 3
								&& parameters[0].ParameterType == typeof(Utf8JsonWriter)
								&& parameters[2].ParameterType == typeof(JsonSerializerOptions);
						})
						.SingleOrDefault();

				if (writeMethod == null)
					throw new InvalidOperationException($"Could not find Write method on '{converter.GetType().FullName}' to serialize '{propertyType.FullName}'");

				return (Utf8JsonWriter writer, object? value, JsonSerializerOptions options) =>
					writeMethod.Invoke(converter, new object?[] { writer, value, options });
			});
		}
	}
}
