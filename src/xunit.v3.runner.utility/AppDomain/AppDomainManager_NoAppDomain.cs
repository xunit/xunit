using System;
using System.Reflection;
using System.Runtime.ExceptionServices;
using Xunit.Internal;

namespace Xunit
{
	class AppDomainManager_NoAppDomain : IAppDomainManager
	{
		public bool HasAppDomain => false;

		public TObject? CreateObject<TObject>(
			AssemblyName assemblyName,
			string typeName,
			params object?[]? args)
				where TObject : class
		{
			Guard.ArgumentNotNull(assemblyName);
			Guard.ArgumentNotNullOrEmpty(typeName);

			try
			{
#if NETFRAMEWORK
				var type = Assembly.Load(assemblyName).GetType(typeName, throwOnError: true);
#else
				var type = Type.GetType($"{typeName}, {assemblyName.FullName}", throwOnError: true);
#endif
				if (type == null)
					return default;

				return (TObject?)Activator.CreateInstance(type, args);
			}
			catch (TargetInvocationException ex)
			{
				ExceptionDispatchInfo.Capture(ex.InnerException!).Throw();
				return default;
			}
		}

#if NETFRAMEWORK
		public TObject? CreateObjectFrom<TObject>(
			string assemblyLocation,
			string typeName,
			params object?[]? args)
				where TObject : class
		{
			Guard.ArgumentNotNullOrEmpty(assemblyLocation);
			Guard.ArgumentNotNullOrEmpty(typeName);

			try
			{
				var type = Assembly.LoadFrom(assemblyLocation).GetType(typeName, throwOnError: true);
				if (type == null)
					return default;

				return (TObject?)Activator.CreateInstance(type, args);
			}
			catch (TargetInvocationException ex)
			{
				ExceptionDispatchInfo.Capture(ex.InnerException!).Throw();
				return default;
			}
		}
#endif

		public void Dispose()
		{ }
	}
}
