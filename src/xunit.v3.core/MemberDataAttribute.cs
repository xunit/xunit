using System;
using Xunit.Sdk;

namespace Xunit;

/// <summary>
/// Provides a data source for a data theory, with the data coming from one of the following sources:
/// 1. A static property
/// 2. A static field
/// 3. A static method (with parameters)
/// The member must return something compatible with IEnumerable&lt;object?[]&gt; with the test data.
/// </summary>
[DataDiscoverer(typeof(MemberDataDiscoverer))]
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public sealed class MemberDataAttribute : MemberDataAttributeBase
{
	/// <summary>
	/// Initializes a new instance of the <see cref="MemberDataAttribute"/> class.
	/// </summary>
	/// <param name="memberName">The name of the public static member on the test class that will provide the test data</param>
	/// <param name="arguments">The arguments to be passed to the member (only supported for methods; ignored for everything else)</param>
	public MemberDataAttribute(
		string memberName,
		params object?[] arguments)
			: base(memberName, arguments)
	{ }
}
