#if !NETSTANDARD1_1

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
        public ISourceInformation GetSourceInformation(ITestCase testCase)
        {
            var testMethod = testCase.TestMethod;
            var testClass = testMethod.TestClass.Class;

#if NET35 || NET452
            var navData = session.GetNavigationData(testClass.Name, testMethod.Method.Name, testClass.Assembly.AssemblyPath);
#else
            var navData = session.GetNavigationData(testClass.Name, testMethod.Method.Name);
#endif

            if (navData == null)
                return EmptySourceInformation;

            return new SourceInformation
            {
                FileName = navData.FileName,
                LineNumber = navData.LineNumber
            };
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (session != null)
                session.Dispose();
        }
    }
}

#endif
