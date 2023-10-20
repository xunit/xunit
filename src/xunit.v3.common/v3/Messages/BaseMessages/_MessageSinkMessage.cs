using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// The base type for all messages. This utilizes JSON polymorphic serialization via System.Text.Json
/// so all derived types must be declared here with a short name to support deserialization.
/// </summary>
//[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
//[JsonDerivedType(typeof(_AfterTestFinished), nameof(_AfterTestFinished))]
//[JsonDerivedType(typeof(_AfterTestStarting), nameof(_AfterTestStarting))]
//[JsonDerivedType(typeof(_BeforeTestFinished), nameof(_BeforeTestFinished))]
//[JsonDerivedType(typeof(_BeforeTestStarting), nameof(_BeforeTestStarting))]
//[JsonDerivedType(typeof(_DiagnosticMessage), nameof(_DiagnosticMessage))]
//[JsonDerivedType(typeof(_DiscoveryComplete), nameof(_DiscoveryComplete))]
//[JsonDerivedType(typeof(_DiscoveryStarting), nameof(_DiscoveryStarting))]
//[JsonDerivedType(typeof(_ErrorMessage), nameof(_ErrorMessage))]
//[JsonDerivedType(typeof(_InternalDiagnosticMessage), nameof(_InternalDiagnosticMessage))]
//[JsonDerivedType(typeof(_MessageSinkMessage), nameof(_MessageSinkMessage))]
//[JsonDerivedType(typeof(_TestAssemblyCleanupFailure), nameof(_TestAssemblyCleanupFailure))]
//[JsonDerivedType(typeof(_TestAssemblyFinished), nameof(_TestAssemblyFinished))]
//[JsonDerivedType(typeof(_TestAssemblyMessage), nameof(_TestAssemblyMessage))]
//[JsonDerivedType(typeof(_TestAssemblyStarting), nameof(_TestAssemblyStarting))]
//[JsonDerivedType(typeof(_TestCaseCleanupFailure), nameof(_TestCaseCleanupFailure))]
//[JsonDerivedType(typeof(_TestCaseDiscovered), nameof(_TestCaseDiscovered))]
//[JsonDerivedType(typeof(_TestCaseFinished), nameof(_TestCaseFinished))]
//[JsonDerivedType(typeof(_TestCaseMessage), nameof(_TestCaseMessage))]
//[JsonDerivedType(typeof(_TestCaseStarting), nameof(_TestCaseStarting))]
//[JsonDerivedType(typeof(_TestClassCleanupFailure), nameof(_TestClassCleanupFailure))]
//[JsonDerivedType(typeof(_TestClassConstructionFinished), nameof(_TestClassConstructionFinished))]
//[JsonDerivedType(typeof(_TestClassConstructionStarting), nameof(_TestClassConstructionStarting))]
//[JsonDerivedType(typeof(_TestClassDisposeFinished), nameof(_TestClassDisposeFinished))]
//[JsonDerivedType(typeof(_TestClassDisposeStarting), nameof(_TestClassDisposeStarting))]
//[JsonDerivedType(typeof(_TestClassFinished), nameof(_TestClassFinished))]
//[JsonDerivedType(typeof(_TestClassMessage), nameof(_TestClassMessage))]
//[JsonDerivedType(typeof(_TestClassStarting), nameof(_TestClassStarting))]
//[JsonDerivedType(typeof(_TestCleanupFailure), nameof(_TestCleanupFailure))]
//[JsonDerivedType(typeof(_TestCollectionCleanupFailure), nameof(_TestCollectionCleanupFailure))]
//[JsonDerivedType(typeof(_TestCollectionFinished), nameof(_TestCollectionFinished))]
//[JsonDerivedType(typeof(_TestCollectionMessage), nameof(_TestCollectionMessage))]
//[JsonDerivedType(typeof(_TestCollectionStarting), nameof(_TestCollectionStarting))]
//[JsonDerivedType(typeof(_TestFailed), nameof(_TestFailed))]
//[JsonDerivedType(typeof(_TestFinished), nameof(_TestFinished))]
//[JsonDerivedType(typeof(_TestMessage), nameof(_TestMessage))]
//[JsonDerivedType(typeof(_TestMethodCleanupFailure), nameof(_TestMethodCleanupFailure))]
//[JsonDerivedType(typeof(_TestMethodFinished), nameof(_TestMethodFinished))]
//[JsonDerivedType(typeof(_TestMethodMessage), nameof(_TestMethodMessage))]
//[JsonDerivedType(typeof(_TestMethodStarting), nameof(_TestMethodStarting))]
//[JsonDerivedType(typeof(_TestNotRun), nameof(_TestNotRun))]
//[JsonDerivedType(typeof(_TestOutput), nameof(_TestOutput))]
//[JsonDerivedType(typeof(_TestPassed), nameof(_TestPassed))]
//[JsonDerivedType(typeof(_TestResultMessage), nameof(_TestResultMessage))]
//[JsonDerivedType(typeof(_TestSkipped), nameof(_TestSkipped))]
//[JsonDerivedType(typeof(_TestStarting), nameof(_TestStarting))]
public class _MessageSinkMessage
{
	static readonly JsonSerializerOptions jsonSerializerOptions = new()
	{
		Converters = { new JsonStringEnumConverter() },
		DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
		IgnoreReadOnlyProperties = true,
	};

	/// <summary>
	/// Parses a previously serialized <see cref="_MessageSinkMessage"/>-derived object.
	/// </summary>
	/// <param name="serialization">The serialized value</param>
	/// <returns>The deserialized object</returns>
	public static _MessageSinkMessage ParseJson(ReadOnlyMemory<byte> serialization)
	{
		var result = JsonSerializer.Deserialize<_MessageSinkMessage>(serialization.Span, jsonSerializerOptions);
		if (result is null)
			throw new ArgumentException("Deserialization of JSON message unexpectedly returned null", nameof(serialization));

		result.ValidateObjectState();
		return result;
	}

	/// <summary>
	/// Creates a JSON serialized version of this message. Can be re-hydrated using <see cref="ParseJson"/>.
	/// </summary>
	/// <returns>The serialization of this message</returns>
	public byte[] ToJson()
	{
		ValidateObjectState();

		return JsonSerializer.SerializeToUtf8Bytes(this, jsonSerializerOptions);
	}

	/// <summary>
	/// Validates that the property value is not <c>null</c>, and if it is, adds the given
	/// property name to the invalid property hash set.
	/// </summary>
	/// <param name="propertyValue">The property value</param>
	/// <param name="propertyName">The property name</param>
	/// <param name="invalidProperties">The hash set to contain the invalid property name list</param>
	protected static void ValidateNullableProperty(
		object? propertyValue,
		string propertyName,
		HashSet<string> invalidProperties)
	{
		Guard.ArgumentNotNull(invalidProperties);

		if (propertyValue is null)
			invalidProperties.Add(propertyName);
	}

	void ValidateObjectState()
	{
		var invalidProperties = new HashSet<string>();

		ValidateObjectState(invalidProperties);

		if (invalidProperties.Count != 0)
			throw new UnsetPropertiesException(invalidProperties, GetType());
	}

	/// <summary>
	/// Called by <see cref="ToJson"/> before serializing the message, and by <see cref="ParseJson"/> after
	/// the message has been serialized. Implementers are expected to call <see cref="ValidateNullableProperty"/>
	/// for each nullable property value to record invalid property values into the provided hash set.
	/// </summary>
	/// <param name="invalidProperties">The hash set to record invalid properties into</param>
	protected virtual void ValidateObjectState(HashSet<string> invalidProperties)
	{ }
}
