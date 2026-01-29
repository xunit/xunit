#pragma warning disable CS8618 // Properties are set by MSBuild

using System.Security.Cryptography;
using System.Text;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using MSBuildTask = Microsoft.Build.Utilities.Task;

namespace Xunit.v3.MSBuildTasks;

// Adapted from https://github.com/microsoft/testfx/blob/a047150c9fcedb16d29d5da35a459352d80c7818/src/Platform/Microsoft.Testing.Platform.MSBuild/Tasks/TestingPlatformEntryPointTask.cs

public sealed class XunitGenerateEntryPoint : MSBuildTask
{
	const string LanguageCS = "C#";
	const string LanguageFS = "F#";
	const string LanguageVB = "VB";

	readonly HashSet<string> languages = new(StringComparer.OrdinalIgnoreCase) { LanguageCS, LanguageFS, LanguageVB };
	string? rootNamespace;

	[Required]
	public string Language { get; set; }

	public string RootNamespace
	{
		get => rootNamespace ?? string.Empty;
		set => rootNamespace = NamespaceHelpers.ToSafeNamespace(value ?? string.Empty);
	}

	[Required]
	public ITaskItem SourcePath { get; set; }

	[Required]
	public string SuffixSeed { get; set; }

	public string? UseMicrosoftTestingPlatformRunner { get; set; }

	public override bool Execute()
	{
		Log.LogMessage(MessageImportance.Normal, $"SourcePath: '{SourcePath.ItemSpec}'");
		Log.LogMessage(MessageImportance.Normal, $"Language: '{Language}'");
		Log.LogMessage(MessageImportance.Normal, $"RootNamespace: '{RootNamespace}'");
		Log.LogMessage(MessageImportance.Normal, $"SuffixSeed: '{SuffixSeed}'");

		if (!languages.Contains(Language))
			Log.LogError($"Language '{Language}' is not supported.");
		else
			GenerateEntryPoint(Language, RootNamespace, UseMicrosoftTestingPlatformRunner ?? "FALSE", SuffixSeed, SourcePath.ItemSpec, Log);

		return !Log.HasLoggedErrors;
	}

	static void GenerateEntryPoint(
		string language,
		string rootNamespace,
		string useMicrosoftTestingPlatformRunner,
		string suffixSeed,
		string sourcePath,
		TaskLoggingHelper log)
	{
		var selfRegisteredExtensionsNamespace =
			rootNamespace.Length == 0
				? string.Empty
				: rootNamespace + ".";

		var entryPointSource = (language.ToUpperInvariant(), useMicrosoftTestingPlatformRunner.ToUpperInvariant()) switch
		{
			(LanguageCS, "FALSE") => XunitEntryPointGenerator.GetXunitEntryPointCSharp(GetNamespaceSuffix(suffixSeed), selfRegisteredExtensionsNamespace),
			(LanguageCS, "TRUE") => XunitEntryPointGenerator.GetMTPEntryPointCSharp(GetNamespaceSuffix(suffixSeed), selfRegisteredExtensionsNamespace),
			(LanguageCS, "OFF") => XunitEntryPointGenerator.GetNonMTPEntryPointCSharp(GetNamespaceSuffix(suffixSeed)),
			(LanguageFS, "FALSE") => XunitEntryPointGenerator.GetXunitEntryPointFSharp(GetNamespaceSuffix(suffixSeed), selfRegisteredExtensionsNamespace),
			(LanguageFS, "TRUE") => XunitEntryPointGenerator.GetMTPEntryPointFSharp(GetNamespaceSuffix(suffixSeed), selfRegisteredExtensionsNamespace),
			(LanguageFS, "OFF") => XunitEntryPointGenerator.GetNonMTPEntryPointFSharp(GetNamespaceSuffix(suffixSeed)),
			(LanguageVB, "FALSE") => XunitEntryPointGenerator.GetXunitEntryPointVB(selfRegisteredExtensionsNamespace),
			(LanguageVB, "TRUE") => XunitEntryPointGenerator.GetMTPEntryPointVB(selfRegisteredExtensionsNamespace),
			(LanguageVB, "OFF") => XunitEntryPointGenerator.GetNonMTPEntryPointVB(),
			_ => throw new InvalidOperationException($"Unsupported language '{language}'"),
		};

		log.LogMessage(MessageImportance.Normal, $"Entrypoint source:{Environment.NewLine}'{entryPointSource}'");

		File.WriteAllText(sourcePath, entryPointSource);
	}

	static string GetNamespaceSuffix(string suffixSeed)
	{
#if NETFRAMEWORK
		using var hasher = SHA256.Create();
		return
			Convert
				.ToBase64String(hasher.ComputeHash(Encoding.UTF8.GetBytes(suffixSeed)))
#else
		return
			Convert
				.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(suffixSeed)))
#endif
				.Substring(0, 9)
				.Replace('+', 'à')
				.Replace('/', 'á')
				.Replace('=', 'â');
	}
}
