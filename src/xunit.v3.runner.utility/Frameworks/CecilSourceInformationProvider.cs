using System;
using System.Linq;
using System.Threading.Tasks;
using Mono.Cecil;
using Mono.Cecil.Rocks;
using Xunit.Runner.Common;

namespace Xunit;

/// <summary>
/// An implementation of <see cref="ISourceInformationProvider"/> backed by <c>Mono.Cecil</c>.
/// </summary>
public sealed class CecilSourceInformationProvider : ISourceInformationProvider
{
	readonly ModuleDefinition moduleDefinition;

	CecilSourceInformationProvider(ModuleDefinition moduleDefinition) =>
		this.moduleDefinition = moduleDefinition;

	/// <summary>
	/// Creates a source provider for the given test assembly.
	/// </summary>
	/// <remarks>
	/// If the symbols are valid and readable, this will return an instance of <see cref="CecilSourceInformationProvider"/>.
	/// If there are no symbols, or the symbols do not match the binary, then this will return an
	/// instance of <see cref="NullSourceInformationProvider"/>.
	/// </remarks>
	/// <param name="assemblyFileName">The test assembly filename</param>
	public static ISourceInformationProvider Create(string? assemblyFileName)
	{
		if (assemblyFileName is not null)
			try
			{
				var moduleDefinition = ModuleDefinition.ReadModule(assemblyFileName, new() { ReadSymbols = true });
				moduleDefinition.ReadSymbols();

				if (moduleDefinition.HasSymbols)
					return new CecilSourceInformationProvider(moduleDefinition);
			}
			catch { }

		return NullSourceInformationProvider.Instance;
	}

	/// <inheritdoc/>
	public ValueTask DisposeAsync()
	{
		moduleDefinition.SafeDispose();

		return default;
	}

	/// <inheritdoc/>
	public SourceInformation GetSourceInformation(
		string? testClassName,
		string? testMethodName)
	{
		try
		{
			var typeDefinition = moduleDefinition.GetType(testClassName);
			var methodDefinitions = typeDefinition.GetMethods().Where(m => m.Name == testMethodName && m.IsPublic).ToList();
			if (methodDefinitions.Count == 1)
			{
				var debugInformation = moduleDefinition.SymbolReader.Read(methodDefinitions[0]);
				var sequencePoint = debugInformation.SequencePoints.FirstOrDefault();
				if (sequencePoint is not null)
					return new(sequencePoint.Document.Url, sequencePoint.StartLine);
			}
		}
		catch { }

		return SourceInformation.Null;
	}
}
