using Xunit.Internal;
using Xunit.v3;

namespace Xunit
{
	static class AppDomainManagerFactory
	{
		internal static IAppDomainManager Create(
			bool useAppDomain,
			string assemblyFileName,
			string? configFileName,
			bool shadowCopy,
			string? shadowCopyFolder,
			_IMessageSink diagnosticMessageSink)
		{
			Guard.ArgumentNotNullOrEmpty(nameof(assemblyFileName), assemblyFileName);
			Guard.ArgumentNotNull(nameof(diagnosticMessageSink), diagnosticMessageSink);

#if NETFRAMEWORK
			if (useAppDomain)
				return new AppDomainManager_AppDomain(assemblyFileName, configFileName, shadowCopy, shadowCopyFolder, diagnosticMessageSink);
#endif

			return new AppDomainManager_NoAppDomain();
		}
	}
}
