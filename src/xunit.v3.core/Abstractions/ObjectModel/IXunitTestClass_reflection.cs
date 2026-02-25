using System.Reflection;

namespace Xunit.v3;

/// <summary>
/// Represents a test class from xUnit.net v3 based on reflection.
/// </summary>
public interface IXunitTestClass : ICoreTestClass
{
	/// <summary>
	/// Gets the <see cref="IBeforeAfterTestAttribute"/>s attached to the test class (and
	/// the test collection and test assembly).
	/// </summary>
	IReadOnlyCollection<IBeforeAfterTestAttribute> BeforeAfterTestAttributes { get; }

	/// <summary>
	/// Gets a list of class fixture types associated with the test class (and the test collection).
	/// </summary>
	IReadOnlyCollection<Type> ClassFixtureTypes { get; }

	/// <summary>
	/// Gets the public constructors on the test class. If the test class is static, will return <see langword="null"/>.
	/// </summary>
	IReadOnlyCollection<ConstructorInfo>? Constructors { get; }

	/// <summary>
	/// Gets the <see cref="MemberInfo.MetadataToken"/> for the test class.
	/// </summary>
	int MetadataToken { get; }

	/// <summary>
	/// Gets the public methods on the test class.
	/// </summary>
	IReadOnlyCollection<MethodInfo> Methods { get; }

	/// <summary>
	/// Gets the test collection this test class belongs to.
	/// </summary>
	new IXunitTestCollection TestCollection { get; }
}
