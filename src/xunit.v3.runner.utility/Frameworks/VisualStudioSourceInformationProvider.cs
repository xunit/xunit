#if !NETSTANDARD

using System;
using Xunit.Internal;
using Xunit.v3;

namespace Xunit
{
	/// <summary>
	/// An implementation of <see cref="_ISourceInformationProvider"/> that will provide source information
	/// when running inside of Visual Studio (via the DiaSession class).
	/// </summary>
	public class VisualStudioSourceInformationProvider : _ISourceInformationProvider
	{
		static readonly _SourceInformation EmptySourceInformation = new _SourceInformation();

		bool disposed;
		readonly DiaSessionWrapper session;

		/// <summary>
		/// Initializes a new instance of the <see cref="VisualStudioSourceInformationProvider" /> class.
		/// </summary>
		/// <param name="assemblyFileName">The assembly file name.</param>
		/// <param name="diagnosticMessageSink">The message sink which receives <see cref="_DiagnosticMessage"/> messages.</param>
		public VisualStudioSourceInformationProvider(
			string assemblyFileName,
			_IMessageSink diagnosticMessageSink)
		{
			Guard.ArgumentNotNullOrEmpty(nameof(assemblyFileName), assemblyFileName);

			session = new DiaSessionWrapper(assemblyFileName, diagnosticMessageSink);
		}

		/// <inheritdoc/>
		public _ISourceInformation GetSourceInformation(
			string? testClassName,
			string? testMethodName)
		{
			var navData = default(DiaNavigationData?);

			if (testClassName != null && testMethodName != null)
				navData = session.GetNavigationData(testClassName, testMethodName);

			if (navData == null)
				return EmptySourceInformation;

			return new _SourceInformation
			{
				FileName = navData.FileName,
				LineNumber = navData.LineNumber
			};
		}

		/// <inheritdoc/>
		public void Dispose()
		{
			if (disposed)
				throw new ObjectDisposedException(GetType().FullName);

			disposed = true;

			if (session != null)
				session.Dispose();
		}
	}
}

#endif
