using System.Reflection;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Extension methods for <see cref="ITypeActivator"/>.
/// </summary>
public static class ITypeActivatorExtensions
{
	/// <summary>
	/// Creates an instance of the specified type using the constructor that best matches the
	/// specified parameters. A default message is used when throwing exceptions for missing
	/// arguments.
	/// </summary>
	/// <param name="activator"/>
	/// <param name="constructor">The constructor to be invoked.</param>
	/// <param name="arguments">An array of arguments that match in number, order, and type the
	/// parameters of the constructor to invoke. If args is an empty array or null, the constructor
	/// that takes no parameters (the default constructor) is invoked.</param>
	/// <returns>A reference to the newly created object.</returns>
	/// <exception cref="TestPipelineException">Thrown when the object creation is not successful.</exception>
	/// <remarks>
	/// This method may called with parameter values that are set to <see cref="Missing.Value"/>.
	/// This indicates that the test framework does not know how to resolve the argument in question.
	/// Implementation for missing parameter resolution is an implementation detail for the type
	/// activator (throwing is an acceptable result).
	/// </remarks>
	public static object CreateInstance(
		this ITypeActivator activator,
		ConstructorInfo constructor,
		object?[]? arguments) =>
			Guard.ArgumentNotNull(activator).CreateInstance(
				constructor,
				arguments,
				(type, missingArguments) =>
					string.Format(
						CultureInfo.CurrentCulture,
						"Cannot create type '{0}' due to missing constructor arguments: {1}",
						type.SafeName(),
						string.Join(", ", missingArguments.Select(a => a.ParameterType.Name + " " + a.Name))
					)
			);
}
