using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using Xunit.Runner.Common;

namespace Xunit;

/// <summary>
/// An implementation of <see cref="ISourceInformationProvider"/> backed by <c>Mono.Cecil</c>.
/// </summary>
public sealed class CecilSourceInformationProvider : ISourceInformationProvider
{
	// 0xFEEFEE marks a "hidden" line, per https://mono-cecil.narkive.com/gFuvydFp/trouble-with-sequencepoint
	const int SEQUENCE_POINT_HIDDEN_LINE = 0xFEEFEE;

	readonly static HashSet<byte[]> PublicKeyTokensToSkip = new(
	[
		[0x50, 0xce, 0xbf, 0x1c, 0xce, 0xb9, 0xd0, 0x5e],  // Mono
		[0x8d, 0x05, 0xb1, 0xbb, 0x7a, 0x6f, 0xdb, 0x6c],  // xUnit.net
	], ByteArrayComparer.Instance);
	readonly static DefaultSymbolReaderProvider SymbolProvider = new(throwIfNoSymbol: false);

	readonly ConcurrentBag<ModuleDefinition> moduleDefinitions = [];
	readonly ConcurrentDictionary<string, TypeDefinition> typeDefinitions = [];

	internal CecilSourceInformationProvider(string assemblyFileName)
	{
		AppDomain.CurrentDomain.AssemblyLoad += OnAssemblyLoad;

		AddAssembly(assemblyFileName);

		foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
			AddAssembly(assembly);
	}

	void AddAssembly(string assemblyFileName)
	{
		try
		{
			if (!File.Exists(assemblyFileName))
				return;

			var moduleDefinition = ModuleDefinition.ReadModule(assemblyFileName);

			// Exclude non-.NET assemblies
			if (moduleDefinition.Assembly is null)
				return;

			// Exclude things with known public keys
			var name = moduleDefinition.Assembly.Name;
			if (name.HasPublicKey && PublicKeyTokensToSkip.Contains(name.PublicKeyToken))
				return;

			using var symbolReader = SymbolProvider.GetSymbolReader(moduleDefinition, moduleDefinition.FileName);
			if (symbolReader is null)
				return;

			moduleDefinition.ReadSymbols(symbolReader, throwIfSymbolsAreNotMaching: false);
			if (!moduleDefinition.HasSymbols)
				return;

			moduleDefinitions.Add(moduleDefinition);
			foreach (var typeDefinition in moduleDefinition.Types.Where(t => t.IsPublic))
				typeDefinitions.TryAdd(typeDefinition.FullName, typeDefinition);
		}
		catch { }
	}

	void AddAssembly(Assembly assembly)
	{
		if (!assembly.IsDynamic)
			AddAssembly(assembly.Location);
	}

	/// <summary>
	/// Creates a source provider for the given test assembly.
	/// </summary>
	/// <param name="assemblyFileName">The test assembly filename</param>
	/// <remarks>
	/// This may return an instance of <see cref="NullSourceInformationProvider"/> if source information
	/// collection is turned off, or if the provided assembly does not exist on disk.
	/// </remarks>
	public static ISourceInformationProvider Create(string? assemblyFileName)
	{
		if (!RunSettingsUtility.CollectSourceInformation)
			return NullSourceInformationProvider.Instance;

		if (!File.Exists(assemblyFileName))
			return NullSourceInformationProvider.Instance;

		return new CecilSourceInformationProvider(assemblyFileName);
	}

	/// <inheritdoc/>
	public ValueTask DisposeAsync()
	{
		AppDomain.CurrentDomain.AssemblyLoad -= OnAssemblyLoad;

		foreach (var moduleDefinition in moduleDefinitions.Distinct())
			moduleDefinition.SafeDispose();

		return default;
	}

	/// <inheritdoc/>
	public SourceInformation GetSourceInformation(
		string? testClassName,
		string? testMethodName)
	{
		if (testClassName is null || testMethodName is null)
			return SourceInformation.Null;

		try
		{
			var testClassNamePieces = testClassName.Split('+');

			if (typeDefinitions.TryGetValue(testClassNamePieces[0], out var typeDefinition))
			{
				foreach (var nestedClassName in testClassNamePieces.Skip(1))
				{
					typeDefinition = typeDefinition.NestedTypes.FirstOrDefault(t => t.Name == nestedClassName);
					if (typeDefinition is null)
						return SourceInformation.Null;
				}

				var methodDefinitions = typeDefinition.GetMethods().Where(m => m.Name == testMethodName && m.IsPublic).ToList();
				if (methodDefinitions.Count == 1)
				{
					var debugInformation = typeDefinition.Module.SymbolReader.Read(methodDefinitions[0]);
					var sequencePoint = debugInformation.SequencePoints.FirstOrDefault(sp => sp.StartLine != SEQUENCE_POINT_HIDDEN_LINE);
					if (sequencePoint is not null)
						return new(sequencePoint.Document.Url, sequencePoint.StartLine);
				}
			}
		}
		catch { }

		return SourceInformation.Null;
	}

	void OnAssemblyLoad(
		object? sender,
		AssemblyLoadEventArgs args) =>
			AddAssembly(args.LoadedAssembly);

	sealed class ByteArrayComparer : IEqualityComparer<byte[]>
	{
		public static ByteArrayComparer Instance { get; } = new();

		public bool Equals(byte[]? x, byte[]? y)
		{
			if (x is null)
				return y is null;
			if (y is null)
				return false;
			if (x.Length != y.Length)
				return false;

			return ((IStructuralEquatable)x).Equals(y, EqualityComparer<byte>.Default);
		}

		public int GetHashCode(byte[] obj) =>
			((IStructuralEquatable)obj).GetHashCode(EqualityComparer<byte>.Default);
	}
}
