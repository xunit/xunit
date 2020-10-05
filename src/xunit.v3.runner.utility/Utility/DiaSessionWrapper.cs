#nullable disable  // TODO: This code is moving to the VSTest adapter

#if NETFRAMEWORK

using System;
using Xunit.Abstractions;
using Xunit.Internal;

namespace Xunit
{
	// This class wraps DiaSession, and uses DiaSessionWrapperHelper in the testing app domain to help us
	// discover when a test is an async test (since that requires special handling by DIA).
	class DiaSessionWrapper : IDisposable
	{
		readonly AppDomainManager_AppDomain appDomainManager;
		bool disposed;
		readonly DiaSessionWrapperHelper helper;
		readonly DiaSession session;

		public DiaSessionWrapper(
			string assemblyFilename,
			IMessageSink diagnosticMessageSink)
		{
			session = new DiaSession(assemblyFilename);

			var assemblyFileName = typeof(DiaSessionWrapperHelper).Assembly.GetLocalCodeBase();

			appDomainManager = new AppDomainManager_AppDomain(assemblyFileName, null, true, null, diagnosticMessageSink);
			helper = appDomainManager.CreateObject<DiaSessionWrapperHelper>(typeof(DiaSessionWrapperHelper).Assembly.GetName(), typeof(DiaSessionWrapperHelper).FullName, assemblyFilename);
		}

		public DiaNavigationData GetNavigationData(string typeName, string methodName)
		{
			var owningAssemblyFilename = session.AssemblyFileName;
			helper.Normalize(ref typeName, ref methodName, ref owningAssemblyFilename);
			return session.GetNavigationData(typeName, methodName, owningAssemblyFilename);
		}

		public void Dispose()
		{
			if (disposed)
				throw new ObjectDisposedException(GetType().FullName);

			disposed = true;

			session.Dispose();
			appDomainManager.Dispose();
		}
	}
}

#endif
