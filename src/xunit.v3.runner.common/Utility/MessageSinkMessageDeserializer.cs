using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.Runner.Common;

/// <summary>
/// A class which understands how to deserialize <see cref="IMessageSinkMessage"/> instances that are decorated
/// with <see cref="JsonTypeIDAttribute"/>. The built-in messages are registered by default, and additional messages
/// can be registered via <see cref="RegisterMessageSinkMessageType"/>.
/// </summary>
public static class MessageSinkMessageDeserializer
{
	static readonly List<string> errors = [];
	static readonly Dictionary<string, Type> typeIdToTypeMappings = [];

	static MessageSinkMessageDeserializer()
	{
		RegisterMessageSinkMessageType(typeof(AfterTestFinished));
		RegisterMessageSinkMessageType(typeof(AfterTestStarting));
		RegisterMessageSinkMessageType(typeof(BeforeTestFinished));
		RegisterMessageSinkMessageType(typeof(BeforeTestStarting));
		RegisterMessageSinkMessageType(typeof(DiagnosticMessage));
		RegisterMessageSinkMessageType(typeof(DiscoveryComplete));
		RegisterMessageSinkMessageType(typeof(DiscoveryStarting));
		RegisterMessageSinkMessageType(typeof(ErrorMessage));
		RegisterMessageSinkMessageType(typeof(InternalDiagnosticMessage));
		RegisterMessageSinkMessageType(typeof(TestAssemblyCleanupFailure));
		RegisterMessageSinkMessageType(typeof(TestAssemblyFinished));
		RegisterMessageSinkMessageType(typeof(TestAssemblyStarting));
		RegisterMessageSinkMessageType(typeof(TestCaseCleanupFailure));
		RegisterMessageSinkMessageType(typeof(TestCaseDiscovered));
		RegisterMessageSinkMessageType(typeof(TestCaseFinished));
		RegisterMessageSinkMessageType(typeof(TestCaseStarting));
		RegisterMessageSinkMessageType(typeof(TestClassCleanupFailure));
		RegisterMessageSinkMessageType(typeof(TestClassConstructionFinished));
		RegisterMessageSinkMessageType(typeof(TestClassConstructionStarting));
		RegisterMessageSinkMessageType(typeof(TestClassDisposeFinished));
		RegisterMessageSinkMessageType(typeof(TestClassDisposeStarting));
		RegisterMessageSinkMessageType(typeof(TestClassFinished));
		RegisterMessageSinkMessageType(typeof(TestClassStarting));
		RegisterMessageSinkMessageType(typeof(TestCleanupFailure));
		RegisterMessageSinkMessageType(typeof(TestCollectionCleanupFailure));
		RegisterMessageSinkMessageType(typeof(TestCollectionFinished));
		RegisterMessageSinkMessageType(typeof(TestCollectionStarting));
		RegisterMessageSinkMessageType(typeof(TestFailed));
		RegisterMessageSinkMessageType(typeof(TestFinished));
		RegisterMessageSinkMessageType(typeof(TestMethodCleanupFailure));
		RegisterMessageSinkMessageType(typeof(TestMethodFinished));
		RegisterMessageSinkMessageType(typeof(TestMethodStarting));
		RegisterMessageSinkMessageType(typeof(TestNotRun));
		RegisterMessageSinkMessageType(typeof(TestOutput));
		RegisterMessageSinkMessageType(typeof(TestPassed));
		RegisterMessageSinkMessageType(typeof(TestSkipped));
		RegisterMessageSinkMessageType(typeof(TestStarting));
	}

	/// <summary>
	/// Parses a previously serialized <see cref="IMessageSinkMessage"/>-derived object.
	/// </summary>
	/// <param name="serialization">The serialized value</param>
	/// <param name="diagnosticMessageSink">The mesage sink to report </param>
	/// <returns>The deserialized object</returns>
	public static IMessageSinkMessage? Deserialize(
		string serialization,
		IMessageSink? diagnosticMessageSink)
	{
		if (errors.Count != 0)
			throw new InvalidOperationException("Errors occurred during JSON deserialization type registration:" + Environment.NewLine + string.Join(Environment.NewLine, errors));

		if (!JsonDeserializer.TryDeserialize(serialization, out var json) || json is not IReadOnlyDictionary<string, object?> root)
		{
			diagnosticMessageSink?.OnMessage(new InternalDiagnosticMessage("JSON message deserialization failure: invalid JSON, or not a JSON object{0}{1}", Environment.NewLine, serialization));
			return null;
		}

		if (!root.TryGetValue("$type", out var typeNameValue) || typeNameValue is not string typeName)
		{
			diagnosticMessageSink?.OnMessage(new InternalDiagnosticMessage("JSON message deserialization failure: root object did not include string property '$type'{0}{1}", Environment.NewLine, serialization));
			return null;
		}

		if (!typeIdToTypeMappings.TryGetValue(typeName, out var type))
		{
			diagnosticMessageSink?.OnMessage(new InternalDiagnosticMessage("JSON message deserialization failure: message '$type' {0} does not have an associated registration", typeName));
			return null;
		}

		if (Activator.CreateInstance(type) is not IMessageSinkMessage message)
		{
			diagnosticMessageSink?.OnMessage(new InternalDiagnosticMessage("Registered JSON message type '{0}' does not implement '{1}'", type.SafeName(), typeof(IMessageSinkMessage).SafeName()));
			return null;
		}

		if (message is not IJsonDeserializable deserializable)
		{
			diagnosticMessageSink?.OnMessage(new InternalDiagnosticMessage("Registered JSON message type '{0}' does not implement '{1}'", type.SafeName(), typeof(IJsonDeserializable).SafeName()));
			return null;
		}

		deserializable.FromJson(root);
		return message;
	}

	/// <summary>
	/// Registers an implementation of <see cref="IMessageSinkMessage"/> and <see cref="IJsonDeserializable"/>, decorated
	/// with <see cref="JsonTypeIDAttribute"/> so that it can be deserialized by the runner pipeline.
	/// </summary>
	/// <param name="type">The message type to register</param>
	public static void RegisterMessageSinkMessageType(Type type)
	{
		Guard.ArgumentNotNull(type);

		if (!typeof(IMessageSinkMessage).IsAssignableFrom(type))
		{
			errors.Add(string.Format(CultureInfo.CurrentCulture, "Message sink message type '{0}' must implement '{1}'", type.SafeName(), typeof(IMessageSinkMessage).SafeName()));
			return;
		}

		if (!typeof(IJsonDeserializable).IsAssignableFrom(type))
		{
			errors.Add(string.Format(CultureInfo.CurrentCulture, "Message sink message type '{0}' must implement '{1}'", type.SafeName(), typeof(IJsonDeserializable).SafeName()));
			return;
		}

		var attr = type.GetCustomAttribute<JsonTypeIDAttribute>();
		if (attr is null)
		{
			errors.Add(string.Format(CultureInfo.CurrentCulture, "Message sink message type '{0}' is missing [JsonTypeID]", type.SafeName()));
			return;
		}

		if (typeIdToTypeMappings.TryGetValue(attr.ID, out var existingType))
		{
			errors.Add(string.Format(CultureInfo.CurrentCulture, "Could not add type '{0}' with JSON type ID of '{1}' because it's already assigned to '{2}'", type.SafeName(), attr.ID, existingType.SafeName()));
			return;
		}

		typeIdToTypeMappings[attr.ID] = type;
	}
}
