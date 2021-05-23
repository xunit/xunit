using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Xunit.v3
{
	/// <summary>
	/// Represents information about an attribute. The primary implementation is based on runtime
	/// reflection, but may also be implemented by runner authors to provide non-reflection-based
	/// test discovery (for example, AST-based runners like CodeRush or Resharper).
	/// </summary>
	public interface _IAttributeInfo
	{
		/// <summary>
		/// Gets the arguments passed to the constructor.
		/// </summary>
		/// <returns>The constructor arguments, in order</returns>
		IReadOnlyCollection<object?> GetConstructorArguments();

		/// <summary>
		/// Gets all the custom attributes for the given attribute.
		/// </summary>
		/// <param name="assemblyQualifiedAttributeTypeName">The type of the attribute to find, in assembly-qualified form</param>
		/// <returns>The matching attributes that decorate the attribute</returns>
		IReadOnlyCollection<_IAttributeInfo> GetCustomAttributes(string assemblyQualifiedAttributeTypeName);

		/// <summary>
		/// Gets a named-argument initialized value of the attribute. If there is no named argument for the given name
		/// on this attribute, then returns default(TValue).
		/// </summary>
		/// <typeparam name="TValue">The type of the argument</typeparam>
		/// <param name="argumentName">The name of the argument</param>
		/// <returns>The argument value</returns>
		[return: MaybeNull]
		TValue GetNamedArgument<TValue>(string argumentName);
	}
}
