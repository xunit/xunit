namespace Xunit.v3;

/// <summary>
/// Used to decorate an assembly, test collection, or test class to allow the use of a custom
/// test method orderer. Only one may exist on a given element.
/// </summary>
/// <remarks>
/// The type returned from <see cref="ITestOrdererAttribute.OrdererType"/> must implement <see cref="ITestMethodOrderer"/>.
/// </remarks>
public interface ITestMethodOrdererAttribute : ITestOrdererAttribute
{ }
