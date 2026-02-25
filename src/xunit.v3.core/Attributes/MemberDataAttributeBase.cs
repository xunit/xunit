namespace Xunit.v3;

/// <summary>
/// Provides a base class for attributes that will provide member data.
/// </summary>
/// <param name="memberName">The name of the public static member on the test class that will provide the test data.
/// It is recommended to use the <see langword="nameof"/> operator to ensure compile-time safety, e.g.,
/// <c>nameof(SomeMemberName)</c>.</param>
/// <param name="arguments">The arguments to be passed to the member (only supported for methods; ignored for
/// everything else)</param>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public abstract partial class MemberDataAttributeBase(
	string memberName,
	object?[] arguments) :
		DataAttribute
{
	/// <summary>
	/// Gets or sets the arguments passed to the member. Only supported for static methods.
	/// </summary>
	public object?[] Arguments { get; } = Guard.ArgumentNotNull(arguments);

	/// <summary>
	/// Returns <see langword="true"/> if the data attribute wants to skip enumerating data during discovery.
	/// This will cause the theory to yield a single test case for all data, and the data discovery
	/// will be during test execution instead of discovery.
	/// </summary>
	public bool DisableDiscoveryEnumeration { get; set; }

	/// <summary>
	/// Gets the member name.
	/// </summary>
	public string MemberName { get; } = Guard.ArgumentNotNull(memberName);

	/// <summary>
	/// Gets or sets the type to retrieve the member from. If not set, then the member will be
	/// retrieved from the unit test class.
	/// </summary>
	public Type? MemberType { get; set; }
}
