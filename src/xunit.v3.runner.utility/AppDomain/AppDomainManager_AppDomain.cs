#if NETFRAMEWORK

using System;
using System.IO;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Security;
using System.Security.Permissions;
using System.Threading;
using Xunit.Abstractions;
using Xunit.Internal;
using Xunit.Runner.v2;

namespace Xunit
{
	class AppDomainManager_AppDomain : IAppDomainManager
	{
		readonly IMessageSink diagnosticMessageSink;

		public AppDomainManager_AppDomain(
			string assemblyFileName,
			string? configFileName,
			bool shadowCopy,
			string? shadowCopyFolder,
			IMessageSink diagnosticMessageSink)
		{
			Guard.ArgumentNotNullOrEmpty(nameof(assemblyFileName), assemblyFileName);

			assemblyFileName = Path.GetFullPath(assemblyFileName);
			Guard.FileExists(nameof(assemblyFileName), assemblyFileName);

			if (configFileName == null)
				configFileName = GetDefaultConfigFile(assemblyFileName);

			if (configFileName != null)
				configFileName = Path.GetFullPath(configFileName);

			AssemblyFileName = assemblyFileName;
			ConfigFileName = configFileName;
			this.diagnosticMessageSink = diagnosticMessageSink;
			AppDomain = CreateAppDomain(assemblyFileName, configFileName, shadowCopy, shadowCopyFolder);
		}

		public AppDomain AppDomain { get; }

		public string AssemblyFileName { get; }

		public string? ConfigFileName { get; }

		public bool HasAppDomain => true;

		static AppDomain CreateAppDomain(
			string assemblyFilename,
			string? configFilename,
			bool shadowCopy,
			string? shadowCopyFolder)
		{
			var setup = new AppDomainSetup
			{
				ApplicationBase = Path.GetDirectoryName(assemblyFilename),
				ApplicationName = Guid.NewGuid().ToString()
			};

			if (shadowCopy)
			{
				setup.ShadowCopyFiles = "true";
				setup.ShadowCopyDirectories = setup.ApplicationBase;
				setup.CachePath = shadowCopyFolder ?? Path.Combine(Path.GetTempPath(), setup.ApplicationName);
			}

			setup.ConfigurationFile = configFilename;

			var result = AppDomain.CreateDomain(Path.GetFileNameWithoutExtension(assemblyFilename), AppDomain.CurrentDomain.Evidence, setup, new PermissionSet(PermissionState.Unrestricted));
			if (result == null)
				throw new InvalidOperationException("Could not create App Domain");

			return result;
		}

		public TObject? CreateObjectFrom<TObject>(
			string assemblyLocation,
			string typeName,
			params object?[]? args)
				where TObject : class
		{
			Guard.ArgumentNotNullOrEmpty(nameof(assemblyLocation), assemblyLocation);
			Guard.ArgumentNotNullOrEmpty(nameof(typeName), typeName);

			try
			{
#pragma warning disable CS0618
				var unwrappedObject = AppDomain.CreateInstanceFromAndUnwrap(assemblyLocation, typeName, false, 0, null, args, null, null, null);
#pragma warning restore CS0618
				return (TObject?)unwrappedObject;
			}
			catch (TargetInvocationException ex)
			{
				ExceptionDispatchInfo.Capture(ex.InnerException!).Throw();
				return default;
			}
		}

		public TObject? CreateObject<TObject>(
			AssemblyName assemblyName,
			string typeName,
			params object?[]? args)
				where TObject : class
		{
			Guard.ArgumentNotNull(nameof(assemblyName), assemblyName);
			Guard.ArgumentNotNullOrEmpty(nameof(typeName), typeName);

			try
			{
#pragma warning disable CS0618
				var unwrappedObject = AppDomain.CreateInstanceAndUnwrap(assemblyName.FullName, typeName, false, 0, null, args, null, null, null);
#pragma warning restore CS0618
				return (TObject?)unwrappedObject;
			}
			catch (TargetInvocationException ex)
			{
				ExceptionDispatchInfo.Capture(ex.InnerException!).Throw();
				return default;
			}
		}

		public virtual void Dispose()
		{
			if (AppDomain != null)
			{
				Exception? failure = null;

				var cachePath = AppDomain.SetupInformation.CachePath;

				try
				{
					void CleanupThread()
					{
						try
						{
							AppDomain.Unload(AppDomain);
						}
						catch (Exception ex)
						{
							failure = ex;
						}
					}

					var thread = new Thread(CleanupThread);
					thread.Start();

					if (!thread.Join(TimeSpan.FromSeconds(10)))
						diagnosticMessageSink.OnMessage(new DiagnosticMessage($"AppDomain.Unload for '{AssemblyFileName}' timed out"));
					else
					{
						if (cachePath != null)
							Directory.Delete(cachePath, true);
					}
				}
				catch (Exception ex)
				{
					if (failure == null)
						failure = ex;
					else
						failure = new AggregateException(failure, ex);
				}

				if (failure != null)
					diagnosticMessageSink.OnMessage(new DiagnosticMessage($"AppDomain.Unload for '{AssemblyFileName}' failed: {failure}"));
			}
		}

		static string? GetDefaultConfigFile(string assemblyFile)
		{
			var configFilename = assemblyFile + ".config";

			if (File.Exists(configFilename))
				return configFilename;

			return null;
		}
	}
}

#endif
