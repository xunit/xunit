namespace Xunit.v3;

/// <summary>
/// Used to decorate an assembly, test collection, test class, or test method to allow the use of a
/// custom test case orderer. Only one may exist on a given element.
/// </summary>
/// <remarks>
/// The type returned from <see cref="ITestOrdererAttribute.OrdererType"/> must implement <see cref="ITestClassOrderer"/>.
/// </remarks>
public interface ITestCaseOrdererAttribute : ITestOrdererAttribute
{ }
