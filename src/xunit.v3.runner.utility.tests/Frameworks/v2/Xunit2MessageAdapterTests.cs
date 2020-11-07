using System;
using System.IO;
using System.Runtime.InteropServices;
using Xunit;
using Xunit.Runner.v2;
using Xunit.v3;
using v2Mocks = Xunit.Runner.v2.Mocks;

public class Xunit2MessageAdapterTests
{
	static readonly string osSpecificAssemblyPath;

	static Xunit2MessageAdapterTests()
	{
		if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			osSpecificAssemblyPath = @"C:\Users\bradwilson\assembly.dll";
		else
			osSpecificAssemblyPath = "/home/bradwilson/assembly.dll";
	}

	[Fact]
	public void TestAssemblyFinished()
	{
		var v2Message = v2Mocks.TestAssemblyFinished(
			testsRun: 2112,
			testsFailed: 42,
			testsSkipped: 6,
			executionTime: 123.4567m
		);

		var adapted = Xunit2MessageAdapter.Adapt(v2Message);

		var v3Message = Assert.IsType<_TestAssemblyFinished>(adapted);
		Assert.NotEmpty(v3Message.AssemblyUniqueID);
		Assert.Equal(123.4567m, v3Message.ExecutionTime);
		Assert.Equal(42, v3Message.TestsFailed);
		Assert.Equal(2112, v3Message.TestsRun);
		Assert.Equal(6, v3Message.TestsSkipped);
	}

	public static TheoryData<string, string?, string> TestAssemblyStartingData()
	{
		var osSpecificConfigPath = osSpecificAssemblyPath + ".json";
		var osSpecificUniqueID = UniqueIDGenerator.ForAssembly("assembly", osSpecificAssemblyPath, osSpecificConfigPath);

		return new TheoryData<string, string?, string>
		{
			{ "asm-path", null, "dded4d854ffcc191c6cd019b8c26cd68190c43122fef8a8d812e9e46ab6d640d" },
			{ "asm-path", "config-path", "7c90ea022d32916680dfa4fb9546e2690a9716c6e8a8b867d62b3c4da5833a91" },
			{ osSpecificAssemblyPath, osSpecificConfigPath, osSpecificUniqueID }
		};
	}

	[Theory]
	[MemberData(nameof(TestAssemblyStartingData))]
	public void TestAssemblyStarting(
		string assemblyPath,
		string? configFilePath,
		string expectedUniqueID)
	{
		var v2Message = v2Mocks.TestAssemblyStarting(assemblyPath, configFilePath);

		var adapted = Xunit2MessageAdapter.Adapt(v2Message);

		var v3Message = Assert.IsType<_TestAssemblyStarting>(adapted);
		Assert.Equal(Path.GetFileNameWithoutExtension(assemblyPath), v3Message.AssemblyName);
		Assert.Equal(assemblyPath, v3Message.AssemblyPath);
		Assert.Equal(expectedUniqueID, v3Message.AssemblyUniqueID);
		Assert.Equal(configFilePath, v3Message.ConfigFilePath);
		Assert.Equal(new DateTimeOffset(2020, 11, 3, 17, 55, 0, TimeSpan.Zero), v3Message.StartTime);
		Assert.Equal("target-framework", v3Message.TargetFramework);
		Assert.Equal("test-env", v3Message.TestEnvironment);
		Assert.Equal("test-framework", v3Message.TestFrameworkDisplayName);
	}

	[Fact]
	public void TestCollectionStarting()
	{
		var definition = v2Mocks.TypeInfo();
		var v2Message = v2Mocks.TestCollectionStarting(collectionDefinition: definition, displayName: "My display name");
		var assemblyUniqueID = UniqueIDGenerator.ForAssembly(
			v2Message.TestAssembly.Assembly.Name,
			v2Message.TestAssembly.Assembly.AssemblyPath,
			v2Message.TestAssembly.ConfigFileName
		);
		var expectedUniqueID = UniqueIDGenerator.ForTestCollection(
			assemblyUniqueID,
			"My display name",
			definition.Name
		);

		var adapted = Xunit2MessageAdapter.Adapt(v2Message);

		var v3Message = Assert.IsType<_TestCollectionStarting>(adapted);
		Assert.Equal(definition.Name, v3Message.TestCollectionClass);
		Assert.Equal("My display name", v3Message.TestCollectionDisplayName);
		Assert.Equal(expectedUniqueID, v3Message.TestCollectionUniqueID);
	}
}
