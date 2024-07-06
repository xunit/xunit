using System;

namespace Xunit.v3;

/// <summary>
/// This interface is intended to be implemented by components which generate test collections.
/// End users specify the desired test collection factory by applying <see cref="CollectionBehaviorAttribute"/>
/// (or any attribute that implements <see cref="ICollectionBehaviorAttribute"/>) at the assembly level.
/// Classes which implement this interface must have a constructor that takes <see cref="IXunitTestAssembly"/>.
/// </summary>
public interface IXunitTestCollectionFactory
{
	/// <summary>
	/// Gets the display name for the test collection factory. This information is shown to the end
	/// user as part of the description of the test environment.
	/// </summary>
	string DisplayName { get; }

	/// <summary>
	/// Gets the test collection for a given test class.
	/// </summary>
	/// <param name="testClass">The test class.</param>
	/// <returns>The test collection.</returns>
	IXunitTestCollection Get(Type testClass);
}
