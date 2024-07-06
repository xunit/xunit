using System;
using System.Collections.Generic;
using System.Reflection;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Represents a test class from xUnit.net v3 based on reflection.
/// </summary>
public interface IXunitTestClass : ITestClass
{
	/// <summary>
	/// Gets the <see cref="IBeforeAfterTestAttribute"/>s attached to the test class (and
	/// the test collection and test assembly).
	/// </summary>
	IReadOnlyCollection<IBeforeAfterTestAttribute> BeforeAfterTestAttributes { get; }

	/// <summary>
	/// Gets the type that this test class refers to.
	/// </summary>
	/// <remarks>
	/// This should only be used to execute a test class. All reflection should be abstracted here
	/// instead for better testability.
	/// </remarks>
	Type Class { get; }

	/// <summary>
	/// Gets a list of class fixture types associated with the test class (and the test collection).
	/// </summary>
	IReadOnlyCollection<Type> ClassFixtureTypes { get; }

	/// <summary>
	/// Gets the public constructors on the test class. If the test class is static, will return <c>null</c>.
	/// </summary>
	IReadOnlyCollection<ConstructorInfo>? Constructors { get; }

	/// <summary>
	/// Gets the public methods on the test class.
	/// </summary>
	IReadOnlyCollection<MethodInfo> Methods { get; }

	/// <summary>
	/// Gets the test case orderer for the test class, if present.
	/// </summary>
	ITestCaseOrderer? TestCaseOrderer { get; }

	/// <summary>
	/// Gets the test collection this test class belongs to.
	/// </summary>
	new IXunitTestCollection TestCollection { get; }
}
