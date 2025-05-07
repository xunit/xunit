using Xunit.Sdk;

namespace Xunit.Internal;

/// <summary>
/// INTERNAL CLASS. DO NOT USE.
/// </summary>
public class TestContextInternal
{
	readonly TestContext testContext;

	TestContextInternal(TestContext testContext) =>
		this.testContext = testContext;

	/// <summary>
	/// INTERNAL PROPERTY. DO NOT USE.
	/// </summary>
	public static TestContextInternal Current =>
		new(TestContext.CurrentInternal);

	/// <summary>
	/// INTERNAL PROPERTY. DO NOT USE.
	/// </summary>
	public IMessageSink? DiagnosticMessageSink
	{
		get => testContext.DiagnosticMessageSink;
		set => testContext.DiagnosticMessageSink = value;
	}

	/// <summary>
	/// INTERNAL PROPERTY. DO NOT USE.
	/// </summary>
	public IMessageSink? InternalDiagnosticMessageSink
	{
		get => testContext.InternalDiagnosticMessageSink;
		set => testContext.InternalDiagnosticMessageSink = value;
	}

	/// <summary>
	/// INTERNAL PROPERTY. DO NOT USE.
	/// </summary>
	public object? TestClassInstance
	{
		get => testContext.TestClassInstance;
		set => testContext.TestClassInstance = value;
	}

	/// <summary>
	/// INTERNAL PROPERTY. DO NOT USE.
	/// </summary>
	public TestResultState? TestState
	{
		get => testContext.TestState;
		set => testContext.TestState = value;
	}

	/// <summary>
	/// INTERNAL METHOD. DO NOT USE.
	/// </summary>
	public void SendInternalDiagnosticMessage(string message) =>
		testContext.SendDiagnosticMessage(message);

	/// <summary>
	/// INTERNAL METHOD. DO NOT USE.
	/// </summary>
	public void SendInternalDiagnosticMessage(
		string format,
		object? arg0) =>
			testContext.SendInternalDiagnosticMessage(format, arg0);

	/// <summary>
	/// INTERNAL METHOD. DO NOT USE.
	/// </summary>
	public void SendInternalDiagnosticMessage(
		string format,
		object? arg0,
		object? arg1) =>
			testContext.SendInternalDiagnosticMessage(format, arg0, arg1);

	/// <summary>
	/// INTERNAL METHOD. DO NOT USE.
	/// </summary>
	public void SendInternalDiagnosticMessage(
		string format,
		object? arg0,
		object? arg1,
		object? arg2) =>
			testContext.SendInternalDiagnosticMessage(format, arg0, arg1, arg2);

	/// <summary>
	/// INTERNAL METHOD. DO NOT USE.
	/// </summary>
	public void SendInternalDiagnosticMessage(
		string format,
		params object?[] args) =>
			testContext.SendInternalDiagnosticMessage(format, args);
}
