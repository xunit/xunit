using System;
using Xunit.v3;

namespace Xunit.Sdk;

/// <summary>
/// Interface to be implemented by classes which are used to discover the test framework. Classes which implement
/// this interface may take <see cref="_IMessageSink"/> as a constructor argument to get access to a message sink
/// to which you can send <see cref="_DiagnosticMessage"/> and <see cref="_InternalDiagnosticMessage"/> instances,
/// since <see cref="TestContext.Current"/> will always be <c>null</c> here.
/// </summary>
public interface ITestFrameworkTypeDiscoverer
{
	/// <summary>
	/// Gets the type that implements <see cref="_ITestFramework"/> to be used to discover
	/// and run tests.
	/// </summary>
	/// <param name="attribute">The test framework attribute that decorated the assembly</param>
	/// <returns>The test framework type</returns>
	Type? GetTestFrameworkType(_IAttributeInfo attribute);
}
