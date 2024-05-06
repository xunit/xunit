using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using Xunit.Internal;
using Xunit.v3;

namespace Xunit.Runner.Common;

/// <summary>
/// A class with helper functions related to <see cref="_MessageSinkMessage"/>.
/// </summary>
public static class MessageSinkMessageHelper
{
	static readonly MethodInfo? deserialize;
	static readonly List<string> errors = [];
	static readonly Dictionary<string, Type> typeIdToTypeMappings = [];

	static MessageSinkMessageHelper()
	{
		RegisterTypeMapping(typeof(_AfterTestFinished));
		RegisterTypeMapping(typeof(_AfterTestStarting));
		RegisterTypeMapping(typeof(_BeforeTestFinished));
		RegisterTypeMapping(typeof(_BeforeTestStarting));
		RegisterTypeMapping(typeof(_DiagnosticMessage));
		RegisterTypeMapping(typeof(_DiscoveryComplete));
		RegisterTypeMapping(typeof(_DiscoveryStarting));
		RegisterTypeMapping(typeof(_ErrorMessage));
		RegisterTypeMapping(typeof(_InternalDiagnosticMessage));
		RegisterTypeMapping(typeof(_TestAssemblyCleanupFailure));
		RegisterTypeMapping(typeof(_TestAssemblyFinished));
		RegisterTypeMapping(typeof(_TestAssemblyStarting));
		RegisterTypeMapping(typeof(_TestCaseCleanupFailure));
		RegisterTypeMapping(typeof(_TestCaseDiscovered));
		RegisterTypeMapping(typeof(_TestCaseFinished));
		RegisterTypeMapping(typeof(_TestCaseStarting));
		RegisterTypeMapping(typeof(_TestClassCleanupFailure));
		RegisterTypeMapping(typeof(_TestClassConstructionFinished));
		RegisterTypeMapping(typeof(_TestClassConstructionStarting));
		RegisterTypeMapping(typeof(_TestClassDisposeFinished));
		RegisterTypeMapping(typeof(_TestClassDisposeStarting));
		RegisterTypeMapping(typeof(_TestClassFinished));
		RegisterTypeMapping(typeof(_TestClassStarting));
		RegisterTypeMapping(typeof(_TestCleanupFailure));
		RegisterTypeMapping(typeof(_TestCollectionCleanupFailure));
		RegisterTypeMapping(typeof(_TestCollectionFinished));
		RegisterTypeMapping(typeof(_TestCollectionStarting));
		RegisterTypeMapping(typeof(_TestFailed));
		RegisterTypeMapping(typeof(_TestFinished));
		RegisterTypeMapping(typeof(_TestMethodCleanupFailure));
		RegisterTypeMapping(typeof(_TestMethodFinished));
		RegisterTypeMapping(typeof(_TestMethodStarting));
		RegisterTypeMapping(typeof(_TestNotRun));
		RegisterTypeMapping(typeof(_TestOutput));
		RegisterTypeMapping(typeof(_TestPassed));
		RegisterTypeMapping(typeof(_TestSkipped));
		RegisterTypeMapping(typeof(_TestStarting));

		deserialize = typeof(_MessageSinkMessage).GetMethod("Deserialize", BindingFlags.Instance | BindingFlags.NonPublic);
		if (deserialize is null)
			errors.Add("Could not find method _MessageSinkMessage.Deserialize");
	}

	/// <summary>
	/// Parses a previously serialized <see cref="_MessageSinkMessage"/>-derived object.
	/// </summary>
	/// <param name="serialization">The serialized value</param>
	/// <returns>The deserialized object</returns>
	public static _MessageSinkMessage? Deserialize(string serialization)
	{
		if (errors.Count != 0 || deserialize is null)
			throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "JSON deserialization errors occurred during startup:{0}{1}", Environment.NewLine, string.Join(Environment.NewLine, errors)));

		if (!JsonDeserializer.TryDeserialize(serialization, out var json) || json is not IReadOnlyDictionary<string, object?> root)
			return null;

		if (!root.TryGetValue("$type", out var typeNameValue) || typeNameValue is not string typeName)
			return null;

		if (!typeIdToTypeMappings.TryGetValue(typeName, out var type))
			return null;

		var message = Activator.CreateInstance(type) as _MessageSinkMessage;
		if (message is null)
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
