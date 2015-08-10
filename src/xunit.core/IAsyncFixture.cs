using System;
using System.Threading.Tasks;

namespace Xunit
{
    /// <summary>
    /// Used to provide asynchronous setup and teardown for <see cref="IClassFixture{TFixture}"/>
    /// and <see cref="ICollectionFixture{TFixture}"/>.
    /// </summary>
    /// <remarks>
    /// If the type implementing IAsyncFixture also implements <see cref="IDisposable"/> then
    /// <see cref="TeardownAsync"/> will be called first followed by <see cref="IDisposable.Dispose"/>.
    /// </remarks>
    public interface IAsyncFixture
    {
        /// <summary>
        /// Implement to complete any asynchronous setup of the fixture before any tests are run.
        /// </summary>
        /// <returns>A task that completes when the setup has been completed.</returns>
        Task SetupAsync();

        /// <summary>
        /// Implement to complete any asynchronous teardown after the fixture has been completed.
        /// </summary>
        /// <returns>A task that completes when the test teardown has been completed.</returns>
        Task TeardownAsync();
    }
}
