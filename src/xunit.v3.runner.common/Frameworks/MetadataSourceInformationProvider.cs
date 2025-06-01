using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Threading.Tasks;
using Xunit.Runner.Common;

namespace Xunit;

/// <summary>
/// An implementation of <see cref="ISourceInformationProvider"/> backed by <c>System.Reflection.Metadata</c>.
/// </summary>
public sealed class MetadataSourceInformationProvider : ISourceInformationProvider
{
	static readonly byte[] PublicKeyXunit = [0, 36, 0, 0, 4, 128, 0, 0, 148, 0, 0, 0, 6, 2, 0, 0, 0, 36, 0, 0, 82, 83, 65, 49, 0, 4, 0, 0, 1, 0, 1, 0, 37, 46, 4, 154, 221, 234, 135, 243, 15, 153, 214, 237, 142, 188, 24, 155, 192, 91, 140, 145, 104, 118, 93, 240, 143, 134, 224, 33, 68, 113, 220, 137, 132, 79, 31, 75, 156, 74, 38, 137, 77, 2, 148, 101, 132, 135, 113, 188, 117, 143, 237, 32, 55, 18, 128, 237, 162, 35, 169, 246, 74, 224, 95, 72, 179, 32, 228, 240, 226, 12, 66, 130, 221, 112, 30, 152, 87, 17, 188, 51, 181, 185, 230, 171, 63, 175, 171, 108, 183, 142, 34, 14, 226, 184, 225, 85, 5, 115, 224, 63, 138, 214, 101, 192, 81, 198, 63, 188, 83, 89, 212, 149, 212, 177, 198, 16, 36, 239, 118, 237, 156, 30, 187, 71, 31, 237, 89, 201];

	readonly Dictionary<string, (FileStream, PEReader, MetadataReader)> assemblies = [];
	readonly Dictionary<string, (MetadataReader, TypeDefinitionHandle)> types = [];

	internal MetadataSourceInformationProvider(string assemblyFileName)
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
			if (!File.Exists(assemblyFileName) || assemblies.ContainsKey(assemblyFileName))
				return;

			var stream = new FileStream(assemblyFileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
			var peReader = new PEReader(stream);
			var metadataReader = peReader.GetMetadataReader();

			// Skip anything signed with our public key
			var publicKeyBytes = metadataReader.GetBlobBytes(metadataReader.GetAssemblyDefinition().PublicKey);
			if (ByteArrayComparer.Instance.Equals(publicKeyBytes, PublicKeyXunit))
			{
				peReader.Dispose();
				stream.Dispose();
				return;
			}

			//// If there's no debug metadata, there's no reason to enumerate
			//if (metadataReader.DebugMetadataHeader is null)
			//{
			//	peReader.Dispose();
			//	stream.Dispose();
			//	return;
			//}

			assemblies[assemblyFileName] = (stream, peReader, metadataReader);

			foreach (var typeDefinitionHandle in metadataReader.TypeDefinitions)
				processTypeDefinition(metadataReader, typeDefinitionHandle, string.Empty);
		}
		catch { }

		void processTypeDefinition(
			MetadataReader metadataReader,
			TypeDefinitionHandle typeDefinitionHandle,
			string typeNamePrefix)
		{
			var typeDefinition = metadataReader.GetTypeDefinition(typeDefinitionHandle);
			if (typeDefinition.IsNested && typeNamePrefix.Length == 0)
				return;

			var visibility = typeDefinition.Attributes & TypeAttributes.VisibilityMask;
			if (visibility != TypeAttributes.Public && visibility != TypeAttributes.NestedPublic)
				return;

			var name = metadataReader.GetString(typeDefinition.Name);

			// Nested class already have the namespace in the prefix
			if (typeNamePrefix.Length != 0)
				name = typeNamePrefix + name;
			else
			{
				var @namespace = metadataReader.GetString(typeDefinition.Namespace);
				if (!string.IsNullOrWhiteSpace(@namespace))
					name = @namespace + "." + name;
			}

			types[name] = (metadataReader, typeDefinitionHandle);

			foreach (var nestedTypeDefinitionHandle in typeDefinition.GetNestedTypes())
				processTypeDefinition(metadataReader, nestedTypeDefinitionHandle, name + "+");
		}
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

		return new MetadataSourceInformationProvider(assemblyFileName);
	}

	/// <inheritdoc/>
	public ValueTask DisposeAsync()
	{
		AppDomain.CurrentDomain.AssemblyLoad -= OnAssemblyLoad;

		foreach (var (stream, peReader, _) in assemblies.Values)
		{
			peReader.SafeDispose();
			stream.SafeDispose();
		}

		return default;
	}

	/// <inheritdoc/>
	public SourceInformation GetSourceInformation(
		string? testClassName,
		string? testMethodName)
	{
		if (testClassName is null || testMethodName is null)
			return SourceInformation.Null;

		if (!types.TryGetValue(testClassName, out var tuple))
			return SourceInformation.Null;

		var (metadataReader, typeDefinitionHandle) = tuple;

		foreach (var methodDebugInformationHandle in metadataReader.MethodDebugInformation)
		{
			var methodDefinition = metadataReader.GetMethodDefinition(methodDebugInformationHandle.ToDefinitionHandle());
			if (methodDefinition.GetDeclaringType() != typeDefinitionHandle)
				continue;

			var name = metadataReader.GetString(methodDefinition.Name);
			if (name != testMethodName)
				continue;

			var methodDebugInformation = metadataReader.GetMethodDebugInformation(methodDebugInformationHandle);
			var sequencePoint = methodDebugInformation.GetSequencePoints().FirstOrDefault(sp => !sp.IsHidden);
			if (sequencePoint.Document.IsNil)
				break;

			var document = metadataReader.GetDocument(sequencePoint.Document);
			return new SourceInformation(metadataReader.GetString(document.Name), sequencePoint.StartLine);
		}

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
