using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Mono.Cecil;
using Mono.Cecil.Rocks;
using Xunit.Internal;
using Xunit.Runner.Common;

namespace Xunit;

/// <summary>
/// An implementation of <see cref="ISourceInformationProvider"/> backed by <c>Mono.Cecil</c>.
/// </summary>
public sealed class CecilSourceInformationProvider : ISourceInformationProvider
{
	readonly List<ModuleDefinition> moduleDefinitions;
	readonly Dictionary<string, TypeDefinition> typeDefinitions = [];

	CecilSourceInformationProvider(List<ModuleDefinition> moduleDefinitions)
	{
		this.moduleDefinitions = moduleDefinitions;

		foreach (var moduleDefinition in moduleDefinitions)
			foreach (var typeDefinition in moduleDefinition.Types.Where(t => t.IsPublic))
				typeDefinitions[typeDefinition.FullName] = typeDefinition;
	}

	/// <summary>
	/// Creates a source provider for the given test assembly, and any <c>*.dll</c> file that exists
	/// in the same folder.
	/// </summary>
	/// <remarks>
	/// If the symbols are valid and readable, this will return an instance of <see cref="CecilSourceInformationProvider"/>.
	/// If there are no symbols, or the symbols do not match the binary, then this will return an
	/// instance of <see cref="NullSourceInformationProvider"/>.
	/// </remarks>
	/// <param name="assemblyFileName">The test assembly filename</param>
	public static ISourceInformationProvider Create(string? assemblyFileName)
	{
		if (!RunSettingsUtility.CollectSourceInformation)
			return NullSourceInformationProvider.Instance;

		var folder = Path.GetDirectoryName(assemblyFileName);
		if (folder is null)
			return NullSourceInformationProvider.Instance;

		try
		{
			var moduleDefinitions =
				Directory
					.GetFiles(folder, "*.dll")
					.Concat([assemblyFileName])
					.Distinct()
					.Select(file =>
					{
						try
						{
							var moduleDefinition = ModuleDefinition.ReadModule(file, new() { ReadSymbols = true });
							moduleDefinition.ReadSymbols();

							if (moduleDefinition.HasSymbols)
								return moduleDefinition;
						}
						catch { }

						return null;
					})
					.WhereNotNull()
					.ToList();

			if (moduleDefinitions.Count != 0)
				return new CecilSourceInformationProvider(moduleDefinitions);
		}
		catch { }

		return NullSourceInformationProvider.Instance;
	}

	/// <inheritdoc/>
	public ValueTask DisposeAsync()
	{
		foreach (var moduleDefinition in moduleDefinitions)
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
					// 0xFEEFEE marks a "hidden" line, per https://mono-cecil.narkive.com/gFuvydFp/trouble-with-sequencepoint
					var sequencePoint = debugInformation.SequencePoints.FirstOrDefault(sp => sp.StartLine != 0xFEEFEE);
					if (sequencePoint is not null)
						return new(sequencePoint.Document.Url, sequencePoint.StartLine);
				}
			}
		}
		catch { }

		return SourceInformation.Null;
	}
}
