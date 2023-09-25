#pragma warning disable CA1813 // This attribute is unsealed because it's an extensibility point

using System;
using Xunit.Sdk;

namespace Xunit;

/// <summary>
/// Attribute that is applied to a method to indicate that it is a fact that should be run
/// by the test runner. It can also be extended to support a customized definition of a
/// test method.
/// </summary>
[XunitTestCaseDiscoverer(typeof(FactDiscoverer))]
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class FactAttribute : Attribute
{
	/// <summary>
	/// Gets the name of the test to be used when the test is skipped. Defaults to
	/// <c>null</c>, which will cause the fully qualified test name to be used.
	/// </summary>
	public virtual string? DisplayName { get; set; }

	/// <summary>
	/// Gets or sets a flag which indicates that this is an explicit test, and will normally
	/// be skipped unless it is requested to be explicitly run. (The mechanism for requesting
	/// to run explicit tests will vary from runner to runner; the command-line runners support
	/// an "-explicit" option.)
	/// </summary>
	public virtual bool Explicit { get; set; }

	/// <summary>
	/// A non-<c>null</c> value marks the test so that it will not be run with the given
	/// string value as the skip reason.
	/// </summary>
	public virtual string? Skip { get; set; }

	/// <summary>
	/// A value greater than zero marks the test as having a timeout, and gets or sets the
	/// timeout (in milliseconds).
	/// </summary>
	/// <remarks>
	/// WARNING: Using this with parallelization turned on will result in undefined behavior.
	/// Timeout is only supported when parallelization is disabled, either globally or with
	/// a parallelization-disabled test collection.
	/// </remarks>
	public virtual int Timeout { get; set; }
}
