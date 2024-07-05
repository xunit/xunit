using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit;

static class AppDomainManagerFactory
{
	internal static IAppDomainManager Create(
		bool useAppDomain,
		string assemblyFileName,
		string? configFileName,
		bool shadowCopy,
		string? shadowCopyFolder,
		IMessageSink diagnosticMessageSink)
	{
		Guard.ArgumentNotNullOrEmpty(assemblyFileName);
		Guard.ArgumentNotNull(diagnosticMessageSink);

#if NETFRAMEWORK
		if (useAppDomain)
			return new AppDomainManager_AppDomain(assemblyFileName, configFileName, shadowCopy, shadowCopyFolder, diagnosticMessageSink);
#endif

		return new AppDomainManager_NoAppDomain();
	}
}
