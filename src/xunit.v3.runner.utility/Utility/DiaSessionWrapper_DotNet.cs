#nullable disable  // TODO: This code is moving to the VSTest adapter

#if NETSTANDARD

using System;

namespace Xunit
{
	// This class wraps DiaSession, and uses DiaSessionWrapperHelper  discover when a test is an async test
	// (since that requires special handling by DIA).
	class DiaSessionWrapper : IDisposable
	{
		bool disposed;
		readonly DiaSessionWrapperHelper helper;
		readonly DiaSession session;

		public DiaSessionWrapper(string assemblyFilename)
		{
			session = new DiaSession(assemblyFilename);

			helper = new DiaSessionWrapperHelper(assemblyFilename);
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
		}
	}
}

#endif
