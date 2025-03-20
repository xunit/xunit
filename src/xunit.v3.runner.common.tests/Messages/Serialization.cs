using System;
using System.Collections.Generic;
using Xunit;
using Xunit.Runner.Common;
using Xunit.Sdk;

public class Serialization
{
	[Theory]
	[MemberData(nameof(Messages), DisableDiscoveryEnumeration = true)]
	public void MessageCanSerializeAndDeserialize(IMessageSinkMessage original)
	{
		// Serialize
		var serialized = original.ToJson();

		Assert.NotNull(serialized);

		// Deserialize
		var success = JsonDeserializer.TryDeserialize(serialized, out var deserialized);

		Assert.True(success);
		var rootObject = Assert.IsAssignableFrom<IReadOnlyDictionary<string, object?>>(deserialized);
		var updated = Activator.CreateInstance(original.GetType()) as IJsonDeserializable;
		Assert.NotNull(updated);
		updated.FromJson(rootObject);

		// Ensure the two objects contain the same data
		Assert.Equivalent(original, updated);
	}

	static readonly IReadOnlyDictionary<string, IReadOnlyCollection<string>> DefaultTraits = new Dictionary<string, IReadOnlyCollection<string>>
	{
		["foo"] = ["bar"],
		["baz"] = ["biff", "bang"],
	};

	// Make sure this list always includes every message we serialize
	public static IEnumerable<TheoryDataRow<IMessageSinkMessage>> Messages =
	[
		new AfterTestFinished
		{
			AttributeName = "attr-name",
			AssemblyUniqueID = "asm-id",
			TestCaseUniqueID = "case-id",
			TestClassUniqueID = "class-id",
			TestCollectionUniqueID = "collection-id",
			TestMethodUniqueID = "method-id",
			TestUniqueID = "test-id",
		},
		new AfterTestStarting
		{
			AttributeName = "attr-name",
			AssemblyUniqueID = "asm-id",
			TestCaseUniqueID = "case-id",
			TestClassUniqueID = "class-id",
			TestCollectionUniqueID = "collection-id",
			TestMethodUniqueID = "method-id",
			TestUniqueID = "test-id",
		},
		new BeforeTestFinished
		{
			AttributeName = "attr-name",
			AssemblyUniqueID = "asm-id",
			TestCaseUniqueID = "case-id",
			TestClassUniqueID = "class-id",
			TestCollectionUniqueID = "collection-id",
			TestMethodUniqueID = "method-id",
			TestUniqueID = "test-id",
		},
		new BeforeTestStarting
		{
			AttributeName = "attr-name",
			AssemblyUniqueID = "asm-id",
			TestCaseUniqueID = "case-id",
			TestClassUniqueID = "class-id",
			TestCollectionUniqueID = "collection-id",
			TestMethodUniqueID = "method-id",
			TestUniqueID = "test-id",
		},
		new DiagnosticMessage
		{
			Message = "diag-message",
		},
		new ErrorMessage
		{
			ExceptionParentIndices = [-1, 0],
			ExceptionTypes = ["parent-type", "child-type"],
			Messages = ["parent-message", "child-message"],
			StackTraces = ["parent-stack", "child-stack"],
		},
		new InternalDiagnosticMessage
		{
			Message = "internal-message",
		},
		new TestAssemblyCleanupFailure
		{
			AssemblyUniqueID = "asm-id",
			ExceptionParentIndices = [-1, 0],
			ExceptionTypes = ["parent-type", "child-type"],
			Messages = ["parent-message", "child-message"],
			StackTraces = ["parent-stack", "child-stack"],
		},
		new TestAssemblyFinished
		{
			AssemblyUniqueID = "asm-id",
			ExecutionTime = 123m,
			FinishTime = DateTimeOffset.UtcNow,
			TestsFailed = 42,
			TestsNotRun = 21,
			TestsSkipped = 12,
			TestsTotal = 2600,
		},
		new TestAssemblyStarting
		{
			AssemblyName = "asm-name",
			AssemblyPath = "asm-path",
			AssemblyUniqueID = "asm-id",
			ConfigFilePath = "config-path",
			Seed = 123456,
			StartTime = DateTimeOffset.UtcNow,
			TargetFramework = "target-framework",
			TestEnvironment = "test-environment",
			TestFrameworkDisplayName = "test-framework",
			Traits = DefaultTraits,
		},
		new TestCaseCleanupFailure
		{
			AssemblyUniqueID = "asm-id",
			ExceptionParentIndices = [-1, 0],
			ExceptionTypes = ["parent-type", "child-type"],
			Messages = ["parent-message", "child-message"],
			StackTraces = ["parent-stack", "child-stack"],
			TestCaseUniqueID = "case-id",
			TestClassUniqueID = "class-id",
			TestCollectionUniqueID = "collection-id",
			TestMethodUniqueID = "method-id",
		},
		new TestCaseDiscovered
		{
			AssemblyUniqueID = "asm-id",
			Explicit = true,
			Serialization = "serialization",
			SkipReason = "skip-reason",
			SourceFilePath = "source-file",
			SourceLineNumber = 42,
			TestCaseDisplayName = "test-case",
			TestCaseUniqueID = "case-id",
			TestClassMetadataToken = 2600,
			TestClassName = "test-class",
			TestClassNamespace = "test-class-namespace",
			TestClassSimpleName = "test-class",
			TestClassUniqueID = "class-id",
			TestCollectionUniqueID = "collection-id",
			TestMethodMetadataToken = 2112,
			TestMethodName = "test-method",
			TestMethodParameterTypesVSTest = ["System.Int32", "System.String"],
			TestMethodReturnTypeVSTest = "System.Void",
			TestMethodUniqueID = "method-id",
			Traits = DefaultTraits,
		},
		new TestCaseFinished
		{
			AssemblyUniqueID = "asm-id",
			ExecutionTime = 123m,
			TestCaseUniqueID = "case-id",
			TestClassUniqueID = "class-id",
			TestCollectionUniqueID = "collection-id",
			TestMethodUniqueID = "method-id",
			TestsFailed = 42,
			TestsNotRun = 21,
			TestsSkipped = 12,
			TestsTotal = 2600,
		},
		new TestCaseStarting
		{
			AssemblyUniqueID = "asm-id",
			Explicit = true,
			SkipReason = "skip-reason",
			SourceFilePath = "source-file",
			SourceLineNumber = 42,
			TestCaseDisplayName = "test-case",
			TestCaseUniqueID = "case-id",
			TestClassMetadataToken = 2600,
			TestClassName = "test-class",
			TestClassNamespace = "test-class-namespace",
			TestClassSimpleName = "test-class",
			TestClassUniqueID = "class-id",
			TestCollectionUniqueID = "collection-id",
			TestMethodMetadataToken = 2112,
			TestMethodName = "test-method",
			TestMethodParameterTypesVSTest = ["System.Int32", "System.String"],
			TestMethodReturnTypeVSTest = "System.Void",
			TestMethodUniqueID = "method-id",
			Traits = DefaultTraits,
		},
		new TestClassCleanupFailure
		{
			AssemblyUniqueID = "asm-id",
			ExceptionParentIndices = [-1, 0],
			ExceptionTypes = ["parent-type", "child-type"],
			Messages = ["parent-message", "child-message"],
			StackTraces = ["parent-stack", "child-stack"],
			TestClassUniqueID = "class-id",
			TestCollectionUniqueID = "collection-id",
		},
		new TestClassConstructionFinished
		{
			AssemblyUniqueID = "asm-id",
			TestCaseUniqueID = "case-id",
			TestClassUniqueID = "class-id",
			TestCollectionUniqueID = "collection-id",
			TestMethodUniqueID = "method-id",
			TestUniqueID = "test-id",
		},
		new TestClassConstructionStarting
		{
			AssemblyUniqueID = "asm-id",
			TestCaseUniqueID = "case-id",
			TestClassUniqueID = "class-id",
			TestCollectionUniqueID = "collection-id",
			TestMethodUniqueID = "method-id",
			TestUniqueID = "test-id",
		},
		new TestClassDisposeFinished
		{
			AssemblyUniqueID = "asm-id",
			TestCaseUniqueID = "case-id",
			TestClassUniqueID = "class-id",
			TestCollectionUniqueID = "collection-id",
			TestMethodUniqueID = "method-id",
			TestUniqueID = "test-id",
		},
		new TestClassDisposeStarting
		{
			AssemblyUniqueID = "asm-id",
			TestCaseUniqueID = "case-id",
			TestClassUniqueID = "class-id",
			TestCollectionUniqueID = "collection-id",
			TestMethodUniqueID = "method-id",
			TestUniqueID = "test-id",
		},
		new TestClassFinished
		{
			AssemblyUniqueID = "asm-id",
			ExecutionTime = 123m,
			TestClassUniqueID = "class-id",
			TestCollectionUniqueID = "collection-id",
			TestsFailed = 42,
			TestsNotRun = 21,
			TestsSkipped = 12,
			TestsTotal = 2600,
		},
		new TestClassStarting
		{
			AssemblyUniqueID = "asm-id",
			TestClassName = "test-class",
			TestClassNamespace = "test-class-namespace",
			TestClassSimpleName = "test-class",
			TestClassUniqueID = "class-id",
			TestCollectionUniqueID = "collection-id",
			Traits = DefaultTraits,
		},
		new TestCleanupFailure
		{
			AssemblyUniqueID = "asm-id",
			ExceptionParentIndices = [-1, 0],
			ExceptionTypes = ["parent-type", "child-type"],
			Messages = ["parent-message", "child-message"],
			StackTraces = ["parent-stack", "child-stack"],
			TestCaseUniqueID = "case-id",
			TestClassUniqueID = "class-id",
			TestCollectionUniqueID = "collection-id",
			TestMethodUniqueID = "method-id",
			TestUniqueID = "test-id",
		},
		new TestCollectionCleanupFailure
		{
			AssemblyUniqueID = "asm-id",
			ExceptionParentIndices = [-1, 0],
			ExceptionTypes = ["parent-type", "child-type"],
			Messages = ["parent-message", "child-message"],
			StackTraces = ["parent-stack", "child-stack"],
			TestCollectionUniqueID = "collection-id",
		},
		new TestCollectionFinished
		{
			AssemblyUniqueID = "asm-id",
			ExecutionTime = 123m,
			TestCollectionUniqueID = "collection-id",
			TestsFailed = 42,
			TestsNotRun = 21,
			TestsSkipped = 12,
			TestsTotal = 2600,
		},
		new TestCollectionStarting
		{
			AssemblyUniqueID = "asm-id",
			TestCollectionClassName = "test-collection-class",
			TestCollectionDisplayName = "test-collection",
			TestCollectionUniqueID = "collection-id",
			Traits = DefaultTraits,
		},
		new TestFailed
		{
			AssemblyUniqueID = "asm-id",
			Cause = FailureCause.Assertion,
			ExceptionParentIndices = [-1, 0],
			ExceptionTypes = ["parent-type", "child-type"],
			ExecutionTime = 123m,
			FinishTime = DateTimeOffset.UtcNow,
			Messages = ["parent-message", "child-message"],
			Output = "output",
			StackTraces = ["parent-stack", "child-stack"],
			TestCaseUniqueID = "case-id",
			TestClassUniqueID = "class-id",
			TestCollectionUniqueID = "collection-id",
			TestMethodUniqueID = "method-id",
			TestUniqueID = "test-id",
			Warnings = ["warning 1", "warning2"],
		},
		new TestFinished
		{
			AssemblyUniqueID = "asm-id",
			Attachments = new Dictionary<string, TestAttachment>
			{
				["StringValue"] = TestAttachment.Create("Hello"),
				["BinaryValue"] = TestAttachment.Create([1, 2, 3], "image/jpeg")
			},
			ExecutionTime = 123m,
			FinishTime = DateTimeOffset.UtcNow,
			Output = "output",
			TestCaseUniqueID = "case-id",
			TestClassUniqueID = "class-id",
			TestCollectionUniqueID = "collection-id",
			TestMethodUniqueID = "method-id",
			TestUniqueID = "test-id",
			Warnings = ["warning 1", "warning2"],
		},
		new TestMethodCleanupFailure
		{
			AssemblyUniqueID = "asm-id",
			ExceptionParentIndices = [-1, 0],
			ExceptionTypes = ["parent-type", "child-type"],
			Messages = ["parent-message", "child-message"],
			StackTraces = ["parent-stack", "child-stack"],
			TestClassUniqueID = "class-id",
			TestCollectionUniqueID = "collection-id",
			TestMethodUniqueID = "method-id",
		},
		new TestMethodFinished
		{
			AssemblyUniqueID = "asm-id",
			ExecutionTime = 123m,
			TestClassUniqueID = "class-id",
			TestCollectionUniqueID = "collection-id",
			TestMethodUniqueID = "method-id",
			TestsFailed = 42,
			TestsNotRun = 21,
			TestsSkipped = 12,
			TestsTotal = 2600,
		},
		new TestMethodStarting
		{
			AssemblyUniqueID = "asm-id",
			MethodName = "test-method",
			TestClassUniqueID = "class-id",
			TestCollectionUniqueID = "collection-id",
			TestMethodUniqueID = "method-id",
			Traits = DefaultTraits,
		},
		new TestNotRun
		{
			AssemblyUniqueID = "asm-id",
			ExecutionTime = 123m,
			FinishTime = DateTimeOffset.UtcNow,
			Output = "output",
			TestCaseUniqueID = "case-id",
			TestClassUniqueID = "class-id",
			TestCollectionUniqueID = "collection-id",
			TestMethodUniqueID = "method-id",
			TestUniqueID = "test-id",
			Warnings = ["warning 1", "warning2"],
		},
		new TestOutput
		{
			AssemblyUniqueID = "asm-id",
			Output = "output",
			TestCaseUniqueID = "case-id",
			TestClassUniqueID = "class-id",
			TestCollectionUniqueID = "collection-id",
			TestMethodUniqueID = "method-id",
			TestUniqueID = "test-id",
		},
		new TestPassed
		{
			AssemblyUniqueID = "asm-id",
			ExecutionTime = 123m,
			FinishTime = DateTimeOffset.UtcNow,
			Output = "output",
			TestCaseUniqueID = "case-id",
			TestClassUniqueID = "class-id",
			TestCollectionUniqueID = "collection-id",
			TestMethodUniqueID = "method-id",
			TestUniqueID = "test-id",
			Warnings = ["warning 1", "warning2"],
		},
		new TestSkipped
		{
			AssemblyUniqueID = "asm-id",
			ExecutionTime = 123m,
			FinishTime = DateTimeOffset.UtcNow,
			Output = "output",
			Reason = "skip-reason",
			TestCaseUniqueID = "case-id",
			TestClassUniqueID = "class-id",
			TestCollectionUniqueID = "collection-id",
			TestMethodUniqueID = "method-id",
			TestUniqueID = "test-id",
			Warnings = ["warning 1", "warning2"],
		},
		new TestStarting
		{
			AssemblyUniqueID = "asm-id",
			Explicit = true,
			StartTime = DateTimeOffset.UtcNow,
			TestCaseUniqueID = "case-id",
			TestClassUniqueID = "class-id",
			TestCollectionUniqueID = "collection-id",
			TestDisplayName = "test-name",
			TestMethodUniqueID = "method-id",
			TestUniqueID = "test-id",
			Timeout = 2600,
			Traits = DefaultTraits,
		},
	];
}
