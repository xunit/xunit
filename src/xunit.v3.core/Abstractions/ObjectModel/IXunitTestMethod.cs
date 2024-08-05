using System;
using System.Collections.Generic;
using System.Reflection;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Represents a test class from xUnit.net v3 based on reflection.
/// </summary>
public interface IXunitTestMethod : ITestMethod
{
	/// <summary>
	/// Gets the <see cref="IBeforeAfterTestAttribute"/>s attached to the test method (and the test class,
	/// test collection, and test assembly).
	/// </summary>
	IReadOnlyCollection<IBeforeAfterTestAttribute> BeforeAfterTestAttributes { get; }

	/// <summary>
	/// Gets the <see cref="IDataAttribute"/>s attached to the test method.
	/// </summary>
	IReadOnlyCollection<IDataAttribute> DataAttributes { get; }

	/// <summary>
	/// Gets the <see cref="IFactAttribute"/>s attached to the test method.
	/// </summary>
	IReadOnlyCollection<IFactAttribute> FactAttributes { get; }

	/// <summary>
	/// Gets a flag which indicates whether this is a generic method definition.
	/// </summary>
	bool IsGenericMethodDefinition { get; }

	/// <summary>
	/// Gets the method that this test method refers to.
	/// </summary>
	/// <remarks>
	/// This should only be used to execute a test method. All reflection should be abstracted here
	/// instead for better testability.
	/// </remarks>
	MethodInfo Method { get; }

	/// <summary>
	/// Gets the parameters of the test method.
	/// </summary>
	IReadOnlyCollection<ParameterInfo> Parameters { get; }

	/// <summary>
	/// Gets the return type of the test method.
	/// </summary>
	Type ReturnType { get; }

	/// <summary>
	/// Gets the arguments that will be passed to the test method.
	/// </summary>
	object?[] TestMethodArguments { get; }

	/// <summary>
	/// Gets the test class that this test method belongs to.
	/// </summary>
	new IXunitTestClass TestClass { get; }

	/// <summary>
	/// Gets the display name for the test method, factoring in arguments and generic types.
	/// </summary>
	/// <param name="baseDisplayName">The base display name.</param>
	/// <param name="testMethodArguments">The test method arguments.</param>
	/// <param name="methodGenericTypes">The generic types of the method.</param>
	string GetDisplayName(string baseDisplayName, object?[]? testMethodArguments, Type[]? methodGenericTypes);

	/// <summary>
	/// Creates a generic version of the test method with the given generic types.
	/// </summary>
	/// <param name="genericTypes">The generic types</param>
	MethodInfo MakeGenericMethod(Type[] genericTypes);

	/// <summary>
	/// Resolves the generic types for the test method given the method's arguments. If the method
	/// is not generic, will return <c>null</c>.
	/// </summary>
	/// <param name="arguments">The method arguments</param>
	Type[]? ResolveGenericTypes(object?[] arguments);

	/// <summary>
	/// Resolves argument values for the test method, ensuring they are the correct type, including
	/// support for optional method arguments.
	/// </summary>
	/// <param name="arguments">The test method arguments</param>
	object?[] ResolveMethodArguments(object?[] arguments);
}
