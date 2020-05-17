using System;
using System.Threading.Tasks;

namespace Xunit
{
    /// <summary>
    /// Used to provide asynchronous lifetime functionality. Currently supported:
    /// - Test classes
    /// - Classes used in <see cref="IClassFixture{TFixture}"/>
    /// - Classes used in <see cref="ICollectionFixture{TFixture}"/>.
    /// </summary>
    public interface IAsyncLifetime
    {
        /// <summary>
        /// Called immediately after the class has been created, before it is used.
        /// </summary>
        Task InitializeAsync();

        /// <summary>
        /// Called when an object is no longer needed. Called just before <see cref="IDisposable.Dispose"/>
        /// if the class also implements that.
        /// </summary>
        Task DisposeAsync();
    }
}
