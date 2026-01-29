using System.Reflection;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Represents the ability to create instances of types on behalf of the test framework.
/// </summary>
/// <remarks>
/// This is currently only used for creating test class instances, but may be used in the future
/// for other class creation operations.
/// </remarks>
public interface ITypeActivator
{
	/// <summary>
	/// Creates an instance of the specified type using the constructor that best matches the
	/// specified parameters.
	/// </summary>
	/// <param name="constructor">The constructor to be invoked.</param>
	/// <param name="arguments">An array of arguments that match in number, order, and type the
	/// parameters of the constructor to invoke. If args is an empty array or null, the constructor
	/// that takes no parameters (the default constructor) is invoked.</param>
	/// <param name="missingArgumentMessageFormatter">A function that will format the message to be used
	/// to throw an exception when one or more arguments could not be resolved.</param>
	/// <returns>A reference to the newly created object.</returns>
	/// <exception cref="TestPipelineException">Thrown when the object creation is not successful.</exception>
	/// <remarks>
	/// This method may called with parameter values that are set to <see cref="Missing.Value"/>.
	/// This indicates that the test framework does not know how to resolve the argument in question.
	/// Implementation for missing parameter resolution is an implementation detail for the type
	/// activator (throwing is an acceptable result).
	/// </remarks>
	object CreateInstance(
		ConstructorInfo constructor,
		object?[]? arguments,
		Func<Type, IReadOnlyCollection<ParameterInfo>, string> missingArgumentMessageFormatter);
}
