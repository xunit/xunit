using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Represents a single test case from xUnit.net v3 based on reflection.
/// </summary>
public interface IXunitTestCase : ITestCase
{
	/// <summary>
	/// Gets the display text for the reason a test that might being skipped.
	/// </summary>
	/// <remarks>
	/// This differs from the contract of <see cref="ITestCaseMetadata.SkipReason"/> by virtue
	/// of the fact that when this value is non-<c>null</c>, it may indicate that a test is
	/// statically skipped (if both <see cref="SkipUnless"/> and <see cref="SkipWhen"/> are
	/// <c>null</c>) or dynamically skipped (if one is non-<c>null</c>).
	/// </remarks>
	new string? SkipReason { get; }

	/// <summary>
	/// When set, indicates the type to use when resolving <see cref="SkipUnless"/> or
	/// <see cref="SkipWhen"/>. If not set, uses the test class type.
	/// </summary>
	Type? SkipType { get; }

	/// <summary>
	/// When set, indicates a public static property that is used at runtime to determine
	/// whether the test is skipped or not (<c>true</c> to run, <c>false</c> to skip).
	/// </summary>
	/// <remarks>
	/// Note: It is an error condition for both <see cref="SkipUnless"/> and <see cref="SkipWhen"/>
	/// to return a non-<c>null</c> value.
	/// </remarks>
	string? SkipUnless { get; }

	/// <summary>
	/// When set, indicates a public static property that is used at runtime to determine
	/// whether the test is skipped or not (<c>false</c> to run, <c>true</c> to skip).
	/// </summary>
	/// <remarks>
	/// Note: It is an error condition for both <see cref="SkipUnless"/> and <see cref="SkipWhen"/>
	/// to return a non-<c>null</c> value.
	/// </remarks>
	string? SkipWhen { get; }

	/// <summary>
	/// Gets the test class that this test case belongs to.
	/// </summary>
	new IXunitTestClass TestClass { get; }

	/// <summary>
	/// Gets the <see cref="MemberInfo.MetadataToken"/> for the test class.
	/// </summary>
	new int TestClassMetadataToken { get; }

	/// <summary>
	/// Gets the name of the class where the test is defined.
	/// </summary>
	new string TestClassName { get; }

	/// <summary>
	/// Gets the test collection this test case belongs to.
	/// </summary>
	new IXunitTestCollection TestCollection { get; }

	/// <summary>
	/// Gets the test method this test case belongs to.
	/// </summary>
	new IXunitTestMethod TestMethod { get; }

	/// <summary>
	/// Gets the <see cref="MemberInfo.MetadataToken"/> for the test method.
	/// </summary>
	new int TestMethodMetadataToken { get; }

	/// <summary>
	/// Gets the method name where the test is defined.
	/// </summary>
	new string TestMethodName { get; }

	/// <summary>
	/// Gets the types for the test method parameters.
	/// </summary>
	new string[] TestMethodParameterTypes { get; }

	/// <summary>
	/// Gets the test method return type.
	/// </summary>
	new string TestMethodReturnType { get; }

	/// <summary>
	/// Gets the timeout of the test, in milliseconds; if zero or negative, means the test case has no timeout.
	/// </summary>
	int Timeout { get; }

	/// <summary>
	/// Executes the test case, returning 0 or more result messages through the message sink.
	/// </summary>
	/// <param name="explicitOption">A flag to indicate how explicit tests should be treated.</param>
	/// <param name="messageBus">The message bus to report results to.</param>
	/// <param name="constructorArguments">The arguments to pass to the constructor.</param>
	/// <param name="aggregator">The error aggregator to use for catching exception.</param>
	/// <param name="cancellationTokenSource">The cancellation token source that indicates whether cancellation has been requested.</param>
	/// <returns>Returns the summary of the test case run.</returns>
	ValueTask<RunSummary> RunAsync(
		ExplicitOption explicitOption,
		IMessageBus messageBus,
		object?[] constructorArguments,
		ExceptionAggregator aggregator,
		CancellationTokenSource cancellationTokenSource
	);
}
