#if NETFRAMEWORK

using System;
using System.Collections.Concurrent;
using System.IO;
using System.Reflection;
using System.Security;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit;

/// <summary>
/// This class provides assistance with assembly resolution for missing assemblies.
/// </summary>
public class AssemblyHelper : MarshalByRefObject, IDisposable
{
	static readonly string[] Extensions = { ".dll", ".exe" };

	readonly string directory;
	bool disposed;
	readonly IMessageSink? diagnosticMessageSink;
	readonly ConcurrentDictionary<string, Assembly?> lookupCache = new();

	/// <summary>
	/// Constructs an instance using the given <paramref name="directory"/> for resolution.
	/// </summary>
	/// <param name="directory">The directory to use for resolving assemblies.</param>
	public AssemblyHelper(string directory)
		: this(directory, null)
	{ }

	/// <summary>
	/// Constructs an instance using the given <paramref name="directory"/> for resolution.
	/// </summary>
	/// <param name="directory">The directory to use for resolving assemblies.</param>
	/// <param name="diagnosticMessageSink">The message sink to send diagnostics messages to.</param>
	public AssemblyHelper(
		string directory,
		IMessageSink? diagnosticMessageSink)
	{
		this.directory = Guard.ArgumentNotNull(directory);
		this.diagnosticMessageSink = diagnosticMessageSink;

		AppDomain.CurrentDomain.AssemblyResolve += Resolve;
	}

	/// <inheritdoc/>
	public void Dispose()
	{
		if (disposed)
			return;

		disposed = true;

		GC.SuppressFinalize(this);

		AppDomain.CurrentDomain.AssemblyResolve -= Resolve;
	}

	/// <inheritdoc/>
	[SecurityCritical]
	public override sealed object InitializeLifetimeService() => null!;

	Assembly? LoadAssembly(AssemblyName assemblyName)
	{
		if (assemblyName.Name is null)
			return null;

		if (lookupCache.TryGetValue(assemblyName.Name, out var result))
			return result;

		var path = Path.Combine(directory, assemblyName.Name);
		result = ResolveAndLoadAssembly(path, out var resolvedAssemblyPath);

		if (diagnosticMessageSink is not null)
		{
			if (result is null)
				diagnosticMessageSink.OnMessage(new InternalDiagnosticMessage("[AssemblyHelper_Desktop.LoadAssembly] Resolution for '{0}' failed, passed down to next resolver", assemblyName.Name));
			else
				diagnosticMessageSink.OnMessage(new InternalDiagnosticMessage("[AssemblyHelper_Desktop.LoadAssembly] Resolved '{0}' to '{1}'", assemblyName.Name, resolvedAssemblyPath));
		}

		lookupCache[assemblyName.Name] = result;
		return result;
	}

	Assembly? Resolve(
		object? sender,
		ResolveEventArgs args)
	{
		if (args.Name is null)
			return null;

		return LoadAssembly(new AssemblyName(args.Name));
	}

	static Assembly? ResolveAndLoadAssembly(
		string pathWithoutExtension,
		out string? resolvedAssemblyPath)
	{
		foreach (var extension in Extensions)
		{
			resolvedAssemblyPath = pathWithoutExtension + extension;

			try
			{
				if (File.Exists(resolvedAssemblyPath))
					return Assembly.LoadFrom(resolvedAssemblyPath);
			}
			catch { }
		}

		resolvedAssemblyPath = null;
		return null;
	}

	/// <summary>
	/// Subscribes to the appropriate assembly resolution event, to provide automatic assembly resolution for
	/// an assembly and any of its dependencies. Depending on the target platform, this may include the use
	/// of the .deps.json file generated during the build process.
	/// </summary>
	/// <returns>An object which, when disposed, un-subscribes.</returns>
	public static IDisposable? SubscribeResolveForAssembly(
		string assemblyFileName,
		IMessageSink? diagnosticMessageSink = null)
	{
		Guard.ArgumentNotNull(assemblyFileName);

		return new AssemblyHelper(Path.GetDirectoryName(Path.GetFullPath(assemblyFileName))!, diagnosticMessageSink);
	}

	/// <summary>
	/// Subscribes to the appropriate assembly resolution event, to provide automatic assembly resolution for
	/// an assembly and any of its dependencies. Depending on the target platform, this may include the use
	/// of the .deps.json file generated during the build process.
	/// </summary>
	/// <returns>An object which, when disposed, un-subscribes.</returns>
	public static IDisposable? SubscribeResolveForAssembly(
		Type typeInAssembly,
		IMessageSink? diagnosticMessageSink = null)
	{
		Guard.ArgumentNotNull(typeInAssembly);

		return new AssemblyHelper(Path.GetDirectoryName(typeInAssembly.Assembly.Location)!, diagnosticMessageSink);
	}
}

#endif
