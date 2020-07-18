#if !NETSTANDARD

using Xunit.Abstractions;
using Xunit.Sdk;

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
			Guard.ArgumentNotNullOrEmpty(nameof(assemblyFileName), assemblyFileName);

			session = new DiaSessionWrapper(assemblyFileName);
		}

		/// <inheritdoc/>
		public ISourceInformation GetSourceInformation(ITestCase? testCase)
		{
			var navData = default(DiaNavigationData?);

			if (testCase != null)
				navData = session.GetNavigationData(testCase.TestMethod.TestClass.Class.Name, testCase.TestMethod.Method.Name);

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
