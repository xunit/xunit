using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// This interface is intended to be implemented by components which generate test collections.
    /// End users specify the desired test collection factory by applying <see cref="CollectionBehaviorAttribute"/>
    /// at the assembly level. Classes which implement this interface must have a constructor
    /// that takes <see cref="ITestAssembly"/> and <see cref="IMessageSink"/>.
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
        ITestCollection Get(ITypeInfo testClass);
    }
}
