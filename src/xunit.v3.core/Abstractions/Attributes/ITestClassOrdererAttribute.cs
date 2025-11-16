namespace Xunit.v3;

/// <summary>
/// Used to decorate an assembly or test collection to allow the use of a custom test class orderer.
/// Only one may exist on a given element.
/// </summary>
/// <remarks>
/// The type returned from <see cref="ITestOrdererAttribute.OrdererType"/> must implement <see cref="ITestClassOrderer"/>.
/// </remarks>
public interface ITestClassOrdererAttribute : ITestOrdererAttribute
{ }
