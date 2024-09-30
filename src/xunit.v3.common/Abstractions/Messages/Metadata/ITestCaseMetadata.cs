using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Xunit.Sdk;

/// <summary>
/// Represents metadata about a test case.
/// </summary>
public interface ITestCaseMetadata
{
	/// <summary>
	/// Gets a flag indicating whether this test case was marked as explicit or not.
	/// </summary>
	bool Explicit { get; }

	/// <summary>
	/// Gets the display text for the reason a test is being skipped; if the test
	/// is not statically skipped, returns <c>null</c>. (A test may be dynamically
	/// skipped at runtime while still returning <c>null</c>.)
	/// </summary>
	string? SkipReason { get; }

	/// <summary>
	/// Gets the source file name. A <c>null</c> value indicates that the
	/// source file name is not known.
	/// </summary>
	string? SourceFilePath { get; }

	/// <summary>
	/// Gets the source file line number. A <c>null</c> value indicates that the
	/// source file line number is not known.
	/// </summary>
	int? SourceLineNumber { get; }

	/// <summary>
	/// Gets the display name of the test case.
	/// </summary>
	string TestCaseDisplayName { get; }

	/// <summary>
	/// Gets the <see cref="MemberInfo.MetadataToken"/> for the test class. If the test did not
	/// originate in a class, will return <c>null</c>.
	/// </summary>
	/// <remarks>
	/// This value is only populated for xUnit.net v3 or later test cases, and will return <c>null</c>
	/// for v1 or v2 test cases, regardless of whether <see cref="TestClassName"/> is <c>null</c>.
	/// </remarks>
	int? TestClassMetadataToken { get; }

	/// <summary>
	/// Gets the full name of the class where the test is defined (i.e., <see cref="Type.FullName"/>).
	/// If the test did not originiate in a class, will return <c>null</c>.
	/// </summary>
	[NotNullIfNotNull(nameof(TestMethodName))]
	string? TestClassName { get; }

	/// <summary>
	/// Gets the namespace of the class where the test is defined. If the test did not
	/// originate in a class, or the class it originated in does not reside in a namespace,
	/// will return <c>null</c>.
	/// </summary>
	string? TestClassNamespace { get; }

	/// <summary>
	/// Gets the simple name of the class where the test is defined (the class name without namespace).
	/// If the test did not originiate in a class, will return <c>null</c>.
	/// </summary>
	[NotNullIfNotNull(nameof(TestMethodName))]
	string? TestClassSimpleName { get; }

	/// <summary>
	/// Gets the <see cref="MemberInfo.MetadataToken"/> for the test method. If the test did not
	/// originate in a method, or the test framework did not provide this information, will return <c>null</c>.
	/// </summary>
	/// <remarks>
	/// This value is only populated for xUnit.net v3 or later test cases, and will return <c>null</c>
	/// for v1 or v2 test cases, regardless of whether <see cref="TestMethodName"/> is <c>null</c>.
	/// </remarks>
	int? TestMethodMetadataToken { get; }

	/// <summary>
	/// Gets the method name where the test is defined, in the <see cref="TestClassName"/> class.
	/// If the test did not originiate in a method, will return <c>null</c>.
	/// </summary>
	string? TestMethodName { get; }

	/// <summary>
	/// Gets the types for the test method parameters. If the test did not originate in a method,
	/// or the test framework does not provide this information, will return <c>null</c>; if the test
	/// method has no parameters, will return an empty array.
	/// </summary>
	/// <remarks>
	/// The values here are formatted according to
	/// <see href="https://github.com/microsoft/vstest/blob/main/docs/RFCs/0017-Managed-TestCase-Properties.md">VSTest rules</see>
	/// in order to support Test Explorer. Note that this is not the same as <see cref="Type.FullName"/>.
	/// </remarks>
	string[]? TestMethodParameterTypesVSTest { get; }

	/// <summary>
	/// Gets the test method return type. If the test did not originate in a method, or the test framework
	/// did not provide this information, will return <c>null</c>.
	/// </summary>
	/// <remarks>
	/// The value here is formatted according to
	/// <see href="https://github.com/microsoft/vstest/blob/main/docs/RFCs/0017-Managed-TestCase-Properties.md">VSTest rules</see>
	/// in order to support Test Explorer. Note that this is not the same as <see cref="Type.FullName"/>.
	/// </remarks>
	string? TestMethodReturnTypeVSTest { get; }

	/// <summary>
	/// Gets the trait values associated with this test case. If there are none, or the framework
	/// does not support traits, this should return an empty dictionary (not <c>null</c>).
	/// </summary>
	IReadOnlyDictionary<string, IReadOnlyCollection<string>> Traits { get; }

	/// <summary>
	/// Gets a unique identifier for the test case.
	/// </summary>
	/// <remarks>
	/// The unique identifier for a test case should be able to discriminate among test cases, even those
	/// which are varied invocations against the same test method (i.e., theories). This identifier should
	/// remain stable until such time as the developer changes some fundamental part of the identity
	/// (assembly, class name, test name, or test data). Recompilation of the test assembly is reasonable
	/// as a stability changing event.
	/// </remarks>
	string UniqueID { get; }
}
