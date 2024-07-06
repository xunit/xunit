using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using Xunit.Sdk;

namespace Xunit.Runner.Common;

/// <summary>
/// A class with helper functions related to <see cref="MessageSinkMessage"/>.
/// </summary>
public static class MessageSinkMessageHelper
{
	static readonly MethodInfo? deserialize;
	static readonly List<string> errors = [];
	static readonly Dictionary<string, Type> typeIdToTypeMappings = [];

	// TODO: We need to generally solve the problem of how messages get registered, so third parties can create their
	// own messages and participate in deserialization of those messages.
	static MessageSinkMessageHelper()
	{
		RegisterTypeMapping(typeof(AfterTestFinished));
		RegisterTypeMapping(typeof(AfterTestStarting));
		RegisterTypeMapping(typeof(BeforeTestFinished));
		RegisterTypeMapping(typeof(BeforeTestStarting));
		RegisterTypeMapping(typeof(DiagnosticMessage));
		RegisterTypeMapping(typeof(DiscoveryComplete));
		RegisterTypeMapping(typeof(DiscoveryStarting));
		RegisterTypeMapping(typeof(ErrorMessage));
		RegisterTypeMapping(typeof(InternalDiagnosticMessage));
		RegisterTypeMapping(typeof(TestAssemblyCleanupFailure));
		RegisterTypeMapping(typeof(TestAssemblyFinished));
		RegisterTypeMapping(typeof(TestAssemblyStarting));
		RegisterTypeMapping(typeof(TestCaseCleanupFailure));
		RegisterTypeMapping(typeof(TestCaseDiscovered));
		RegisterTypeMapping(typeof(TestCaseFinished));
		RegisterTypeMapping(typeof(TestCaseStarting));
		RegisterTypeMapping(typeof(TestClassCleanupFailure));
		RegisterTypeMapping(typeof(TestClassConstructionFinished));
		RegisterTypeMapping(typeof(TestClassConstructionStarting));
		RegisterTypeMapping(typeof(TestClassDisposeFinished));
		RegisterTypeMapping(typeof(TestClassDisposeStarting));
		RegisterTypeMapping(typeof(TestClassFinished));
		RegisterTypeMapping(typeof(TestClassStarting));
		RegisterTypeMapping(typeof(TestCleanupFailure));
		RegisterTypeMapping(typeof(TestCollectionCleanupFailure));
		RegisterTypeMapping(typeof(TestCollectionFinished));
		RegisterTypeMapping(typeof(TestCollectionStarting));
		RegisterTypeMapping(typeof(TestFailed));
		RegisterTypeMapping(typeof(TestFinished));
		RegisterTypeMapping(typeof(TestMethodCleanupFailure));
		RegisterTypeMapping(typeof(TestMethodFinished));
		RegisterTypeMapping(typeof(TestMethodStarting));
		RegisterTypeMapping(typeof(TestNotRun));
		RegisterTypeMapping(typeof(TestOutput));
		RegisterTypeMapping(typeof(TestPassed));
		RegisterTypeMapping(typeof(TestSkipped));
		RegisterTypeMapping(typeof(TestStarting));

		deserialize = typeof(MessageSinkMessage).GetMethod("Deserialize", BindingFlags.Instance | BindingFlags.NonPublic);
		if (deserialize is null)
			errors.Add("Could not find method _MessageSinkMessage.Deserialize");
	}

	/// <summary>
	/// Parses a previously serialized <see cref="MessageSinkMessage"/>-derived object.
	/// </summary>
	/// <param name="serialization">The serialized value</param>
	/// <returns>The deserialized object</returns>
	public static MessageSinkMessage? Deserialize(string serialization)
	{
		if (errors.Count != 0 || deserialize is null)
			throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "JSON deserialization errors occurred during startup:{0}{1}", Environment.NewLine, string.Join(Environment.NewLine, errors)));

		// TODO: All these nulls... should they be exceptions?

		if (!JsonDeserializer.TryDeserialize(serialization, out var json) || json is not IReadOnlyDictionary<string, object?> root)
			return null;

		if (!root.TryGetValue("$type", out var typeNameValue) || typeNameValue is not string typeName)
			return null;

		if (!typeIdToTypeMappings.TryGetValue(typeName, out var type))
			return null;

		if (Activator.CreateInstance(type) is not MessageSinkMessage message)
			return null;

		deserialize.Invoke(message, [root]);
		message.ValidateObjectState();
		return message;
	}

	static void RegisterTypeMapping(Type type)
	{
		var attr = type.GetCustomAttribute<JsonTypeIDAttribute>();
		if (attr is null)
		{
			errors.Add(string.Format(CultureInfo.CurrentCulture, "Could not find [JsonTypeID] on type '{0}'", type.SafeName()));
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
