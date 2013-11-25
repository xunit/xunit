using System;
using Xunit.Abstractions;

namespace Xunit
{
    /// <summary>
    /// An implementation of <see cref="ISourceInformationProvider"/> that will provide source information
    /// when running inside of Visual Studio (via the DiaSession class).
    /// </summary>
    public class VisualStudioSourceInformationProvider : LongLivedMarshalByRefObject, ISourceInformationProvider
    {
        static readonly SourceInformation EmptySourceInformation = new SourceInformation();

        readonly DiaSessionWrapper session;

        /// <summary>
        /// Initializes a new instance of the <see cref="VisualStudioSourceInformationProvider" /> class.
        /// </summary>
        /// <param name="assemblyFileName">The assembly file name.</param>
        public VisualStudioSourceInformationProvider(string assemblyFileName)
        {
            session = new DiaSessionWrapper(assemblyFileName);
        }

        /// <inheritdoc/>
        public SourceInformation GetSourceInformation(ITestCase testCase)
        {
            var navData = session.GetNavigationData(testCase.Class.Name, testCase.Method.Name);
            if (navData == null)
                return EmptySourceInformation;

            return new SourceInformation
            {
                FileName = navData.FileName,
                LineNumber = navData.LineNumber
            };
        }

        /// <inheritdoc/>
        public override void Dispose()
        {
            if (session != null)
                session.Dispose();

            base.Dispose();
        }
    }
}
