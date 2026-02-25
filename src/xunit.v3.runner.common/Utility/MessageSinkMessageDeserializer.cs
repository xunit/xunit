using System.Collections.Concurrent;
using Xunit.Sdk;

namespace Xunit.Runner.Common;

public static partial class MessageSinkMessageDeserializer
{
	static readonly List<string> errors = [];
	static readonly ConcurrentDictionary<string, Func<object?>> typeIDToFactoryMappings = [];

	static MessageSinkMessageDeserializer()
	{
		RegisterMessageSinkMessageType(AfterTestFinished.TypeID, Activator.CreateInstance<AfterTestFinished>);
		RegisterMessageSinkMessageType(AfterTestStarting.TypeID, Activator.CreateInstance<AfterTestStarting>);
		RegisterMessageSinkMessageType(BeforeTestFinished.TypeID, Activator.CreateInstance<BeforeTestFinished>);
		RegisterMessageSinkMessageType(BeforeTestStarting.TypeID, Activator.CreateInstance<BeforeTestStarting>);
		RegisterMessageSinkMessageType(DiagnosticMessage.TypeID, Activator.CreateInstance<DiagnosticMessage>);
		RegisterMessageSinkMessageType(DiscoveryComplete.TypeID, Activator.CreateInstance<DiscoveryComplete>);
		RegisterMessageSinkMessageType(DiscoveryStarting.TypeID, Activator.CreateInstance<DiscoveryStarting>);
		RegisterMessageSinkMessageType(ErrorMessage.TypeID, Activator.CreateInstance<ErrorMessage>);
		RegisterMessageSinkMessageType(InternalDiagnosticMessage.TypeID, Activator.CreateInstance<InternalDiagnosticMessage>);
		RegisterMessageSinkMessageType(TestAssemblyCleanupFailure.TypeID, Activator.CreateInstance<TestAssemblyCleanupFailure>);
		RegisterMessageSinkMessageType(TestAssemblyFinished.TypeID, Activator.CreateInstance<TestAssemblyFinished>);
		RegisterMessageSinkMessageType(TestAssemblyStarting.TypeID, Activator.CreateInstance<TestAssemblyStarting>);
		RegisterMessageSinkMessageType(TestCaseCleanupFailure.TypeID, Activator.CreateInstance<TestCaseCleanupFailure>);
		RegisterMessageSinkMessageType(TestCaseDiscovered.TypeID, Activator.CreateInstance<TestCaseDiscovered>);
		RegisterMessageSinkMessageType(TestCaseFinished.TypeID, Activator.CreateInstance<TestCaseFinished>);
		RegisterMessageSinkMessageType(TestCaseStarting.TypeID, Activator.CreateInstance<TestCaseStarting>);
		RegisterMessageSinkMessageType(TestClassCleanupFailure.TypeID, Activator.CreateInstance<TestClassCleanupFailure>);
		RegisterMessageSinkMessageType(TestClassConstructionFinished.TypeID, Activator.CreateInstance<TestClassConstructionFinished>);
		RegisterMessageSinkMessageType(TestClassConstructionStarting.TypeID, Activator.CreateInstance<TestClassConstructionStarting>);
		RegisterMessageSinkMessageType(TestClassDisposeFinished.TypeID, Activator.CreateInstance<TestClassDisposeFinished>);
		RegisterMessageSinkMessageType(TestClassDisposeStarting.TypeID, Activator.CreateInstance<TestClassDisposeStarting>);
		RegisterMessageSinkMessageType(TestClassFinished.TypeID, Activator.CreateInstance<TestClassFinished>);
		RegisterMessageSinkMessageType(TestClassStarting.TypeID, Activator.CreateInstance<TestClassStarting>);
		RegisterMessageSinkMessageType(TestCleanupFailure.TypeID, Activator.CreateInstance<TestCleanupFailure>);
		RegisterMessageSinkMessageType(TestCollectionCleanupFailure.TypeID, Activator.CreateInstance<TestCollectionCleanupFailure>);
		RegisterMessageSinkMessageType(TestCollectionFinished.TypeID, Activator.CreateInstance<TestCollectionFinished>);
		RegisterMessageSinkMessageType(TestCollectionStarting.TypeID, Activator.CreateInstance<TestCollectionStarting>);
		RegisterMessageSinkMessageType(TestFailed.TypeID, Activator.CreateInstance<TestFailed>);
		RegisterMessageSinkMessageType(TestFinished.TypeID, Activator.CreateInstance<TestFinished>);
		RegisterMessageSinkMessageType(TestMethodCleanupFailure.TypeID, Activator.CreateInstance<TestMethodCleanupFailure>);
		RegisterMessageSinkMessageType(TestMethodFinished.TypeID, Activator.CreateInstance<TestMethodFinished>);
		RegisterMessageSinkMessageType(TestMethodStarting.TypeID, Activator.CreateInstance<TestMethodStarting>);
		RegisterMessageSinkMessageType(TestNotRun.TypeID, Activator.CreateInstance<TestNotRun>);
		RegisterMessageSinkMessageType(TestOutput.TypeID, Activator.CreateInstance<TestOutput>);
		RegisterMessageSinkMessageType(TestPassed.TypeID, Activator.CreateInstance<TestPassed>);
		RegisterMessageSinkMessageType(TestSkipped.TypeID, Activator.CreateInstance<TestSkipped>);
		RegisterMessageSinkMessageType(TestStarting.TypeID, Activator.CreateInstance<TestStarting>);
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

		if (!typeIDToFactoryMappings.TryGetValue(typeName, out var factory))
		{
			diagnosticMessageSink?.OnMessage(new InternalDiagnosticMessage("JSON message deserialization failure: message '$type' {0} does not have an associated registration", typeName));
			return null;
		}

		var obj = factory();
		if (obj is null)
		{
			diagnosticMessageSink?.OnMessage(new InternalDiagnosticMessage("Registered JSON message type '{0}' factory returned null", typeName));
			return null;
		}

		if (obj is not IMessageSinkMessage message)
		{
			diagnosticMessageSink?.OnMessage(new InternalDiagnosticMessage("Registered JSON message type '{0}' does not implement '{1}'", typeName, typeof(IMessageSinkMessage).SafeName()));
			return null;
		}

		if (obj is not IJsonDeserializable deserializable)
		{
			diagnosticMessageSink?.OnMessage(new InternalDiagnosticMessage("Registered JSON message type '{0}' does not implement '{1}'", typeName, typeof(IJsonDeserializable).SafeName()));
			return null;
		}

		deserializable.FromJson(root);
		return message;
	}

	/// <summary>
	/// Registers a deserializable JSON container message sink message object.
	/// </summary>
	/// <param name="jsonTypeID">The JSON type ID used to identify this type instance</param>
	/// <param name="factory">The factory used to create instances of the object during deserialization</param>
	/// <remarks>
	/// The object returned from <paramref name="factory"/> must implement both <see cref="IMessageSinkMessage"/> and <see cref="IJsonDeserializable"/>.
	/// </remarks>
	public static void RegisterMessageSinkMessageType(
		string jsonTypeID,
		Func<object?> factory)
	{
		if (string.IsNullOrWhiteSpace(jsonTypeID))
		{
			errors.Add("Message sink message ID must be non-null, non-whitespace");
			return;
		}

		if (factory is null)
		{
			errors.Add(string.Format(CultureInfo.CurrentCulture, "Message factory for type ID '{0}' must not be null", jsonTypeID));
			return;
		}

		if (!typeIDToFactoryMappings.TryAdd(jsonTypeID, factory))
			errors.Add(string.Format(CultureInfo.CurrentCulture, "Could not add deserializer with JSON type ID of '{0}' because it's already registered", jsonTypeID));
	}
}
