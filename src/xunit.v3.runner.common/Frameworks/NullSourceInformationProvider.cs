using Xunit.Abstractions;
using Xunit.Sdk;

namespace Xunit
{
    /// <summary>
    /// An implementation of <see cref="ISourceInformationProvider"/> that always returns no
    /// source information. Useful for test runners which don't need or cannot provide source
    /// information during discovery.
    /// </summary>
    public class NullSourceInformationProvider : LongLivedMarshalByRefObject, ISourceInformationProvider
    {
        /// <inheritdoc/>
        public ISourceInformation GetSourceInformation(ITestCase testCase)
        {
            return new SourceInformation();
        }

        /// <inheritdoc/>
        public void Dispose() { }
    }
}
