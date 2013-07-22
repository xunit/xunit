using Xunit.Abstractions;

namespace Xunit
{
    /// <summary>
    /// An implementation of <see cref="ISourceInformationProvider"/> that will provide source information
    /// when running inside of Visual Studio (via the DiaSession class).
    /// </summary>
    public class VisualStudioSourceInformationProvider : LongLivedMarshalByRefObject, ISourceInformationProvider
    {
        /// <inheritdoc/>
        public SourceInformation GetSourceInformation(ITestCase testCase)
        {
            return new SourceInformation();

            // TODO: Load DiaSession dynamically, since it's only available when running inside of Visual Studio.
            //       Or look at the CCI2 stuff from the Rx framework: https://github.com/Reactive-Extensions/IL2JS/tree/master/CCI2/PdbReader
        }
    }
}
