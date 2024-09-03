#if NETFRAMEWORK

using System;
using System.IO;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Security;
using System.Security.Permissions;
using System.Threading;
using Xunit.Internal;
using Xunit.Runner.Common;
using Xunit.Sdk;

namespace Xunit;

sealed class AppDomainManager_AppDomain : IAppDomainManager
{
	readonly IMessageSink diagnosticMessageSink;

	public AppDomainManager_AppDomain(
		string assemblyFileName,
		string? configFileName,
		bool shadowCopy,
		string? shadowCopyFolder,
		IMessageSink diagnosticMessageSink)
	{
		Guard.ArgumentNotNullOrEmpty(assemblyFileName);

		assemblyFileName = Path.GetFullPath(assemblyFileName);
		Guard.FileExists(assemblyFileName);

		configFileName ??= GetDefaultConfigFile(assemblyFileName);

		if (configFileName is not null)
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

		return AppDomain.CreateDomain(Path.GetFileNameWithoutExtension(assemblyFilename), AppDomain.CurrentDomain.Evidence, setup, new PermissionSet(PermissionState.Unrestricted));
	}

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
			var unwrappedObject = AppDomain.CreateInstanceFromAndUnwrap(assemblyLocation, typeName, false, 0, null, args, null, null);
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
		Guard.ArgumentNotNull(assemblyName);
		Guard.ArgumentNotNullOrEmpty(typeName);

		try
		{
			var unwrappedObject = AppDomain.CreateInstanceAndUnwrap(assemblyName.FullName, typeName, false, 0, null, args, null, null);
			return (TObject?)unwrappedObject;
		}
		catch (TargetInvocationException ex)
		{
			ExceptionDispatchInfo.Capture(ex.InnerException!).Throw();
			return default;
		}
	}

	public void Dispose()
	{
		if (AppDomain is not null)
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
					diagnosticMessageSink.OnMessage(new InternalDiagnosticMessage("AppDomain.Unload for '{0}' timed out", AssemblyFileName));
				else
				{
					if (cachePath is not null)
						Directory.Delete(cachePath, true);
				}
			}
			catch (Exception ex)
			{
#pragma warning disable CA1508 // failure can be set in CleanupThread
				if (failure is null)
#pragma warning restore CA1508
					failure = ex;
				else
					failure = new AggregateException(failure, ex);
			}

			if (failure is not null)
				diagnosticMessageSink.OnMessage(new InternalDiagnosticMessage("AppDomain.Unload for '{0}' failed: {1}", AssemblyFileName, failure));
		}
	}

	static string? GetDefaultConfigFile(string assemblyFile)
	{
		var configFilename = assemblyFile + ".config";

		return
			File.Exists(configFilename)
				? configFilename
				: null;
	}
}

#endif
