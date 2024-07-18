using System.Collections.Generic;

namespace Xunit.Sdk;

/// <summary>
/// Represents metadata about a test method.
/// </summary>
public interface ITestMethodMetadata
{
	/// <summary>
	/// Gets the name of the test method that is associated with this message.
	/// </summary>
	string MethodName { get; }

	/// <summary>
	/// Gets the trait values associated with this test method (and the test class,
	/// test collection, and test assembly). If there are none, or the framework does
	/// not support traits, this returns an empty dictionary (not <c>null</c>).
	/// </summary>
	IReadOnlyDictionary<string, IReadOnlyCollection<string>> Traits { get; }

	/// <summary>
	/// Gets the unique ID for this test method.
	/// </summary>
	/// <remarks>
	/// The unique identifier for a test method should be able to discriminate among test methods in the
	/// same test assembly. This identifier should remain stable until such time as the developer changes
	/// some fundamental part of the identity (assembly, collection, test class, or test method).
	/// Recompilation of the test assembly is reasonable as a stability changing event.
	/// </remarks>
	string UniqueID { get; }
}
