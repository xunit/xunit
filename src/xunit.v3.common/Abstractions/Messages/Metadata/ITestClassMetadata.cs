using System;
using System.Collections.Generic;

namespace Xunit.Sdk;

/// <summary>
/// Represents metadata about a test class.
/// </summary>
public interface ITestClassMetadata
{
	/// <summary>
	/// Gets the full name of the test class (i.e., <see cref="Type.FullName"/>).
	/// </summary>
	string TestClassName { get; }

	/// <summary>
	/// Gets the namespace of the class where the test is defined. Will return <c>null</c> for
	/// classes not residing in a namespace.
	/// </summary>
	string? TestClassNamespace { get; }

	/// <summary>
	/// Gets the simple name of the test class (the class name without namespace).
	/// </summary>
	string TestClassSimpleName { get; }

	/// <summary>
	/// Gets the trait values associated with this test class (and the test collection, and test
	/// assembly). If there are none, or the framework does not support traits, this returns an
	/// empty dictionary (not <c>null</c>).
	/// </summary>
	IReadOnlyDictionary<string, IReadOnlyCollection<string>> Traits { get; }

	/// <summary>
	/// Gets the unique ID for this test class.
	/// </summary>
	/// <remarks>
	/// The unique identifier for a test class should be able to discriminate among test classes in the
	/// same test assembly. This identifier should remain stable until such time as the developer changes
	/// some fundamental part of the identity (assembly, collection, or test class). Recompilation of the
	/// test assembly is reasonable as a stability changing event.
	/// </remarks>
	string UniqueID { get; }
}
