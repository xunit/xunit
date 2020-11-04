using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Xunit;
using Xunit.Abstractions;
using Xunit.Internal;
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
		var v2TestAssemblyStarting = TestableTestAssemblyStarting.Create(
			assemblyPath,
			configFilePath
		);

		var adapted = Xunit2MessageAdapter.Adapt(v2TestAssemblyStarting);

		var v3TestAssemblyStarting = Assert.IsType<_TestAssemblyStarting>(adapted);
		Assert.Equal(Path.GetFileNameWithoutExtension(assemblyPath), v3TestAssemblyStarting.AssemblyName);
		Assert.Equal(assemblyPath, v3TestAssemblyStarting.AssemblyPath);
		Assert.Equal<object>(expectedUniqueID, v3TestAssemblyStarting.AssemblyUniqueID);
		Assert.Equal(configFilePath, v3TestAssemblyStarting.ConfigFilePath);
		Assert.Equal(new DateTimeOffset(2020, 11, 3, 17, 55, 0, TimeSpan.Zero), v3TestAssemblyStarting.StartTime);
		Assert.Equal("target-framework", v3TestAssemblyStarting.TargetFramework);
		Assert.Equal("test-env", v3TestAssemblyStarting.TestEnvironment);
		Assert.Equal("test-framework", v3TestAssemblyStarting.TestFrameworkDisplayName);
	}

	class TestableTestAssemblyStarting : TestAssemblyMessage, ITestAssemblyStarting
	{
		TestableTestAssemblyStarting(
			IEnumerable<ITestCase> testCases,
			ITestAssembly testAssembly,
			DateTime startTime,
			string testEnvironment,
			string testFrameworkDisplayName)
				: base(testCases, testAssembly)
		{
			Guard.ArgumentNotNull(nameof(testCases), testCases);
			Guard.ArgumentNotNull(nameof(testAssembly), testAssembly);
			Guard.ArgumentNotNull(nameof(testEnvironment), testEnvironment);
			Guard.ArgumentNotNull(nameof(testFrameworkDisplayName), testFrameworkDisplayName);

			StartTime = startTime;
			TestEnvironment = testEnvironment;
			TestFrameworkDisplayName = testFrameworkDisplayName;
		}

		/// <inheritdoc/>
		public DateTime StartTime { get; }

		/// <inheritdoc/>
		public string TestEnvironment { get; }

		/// <inheritdoc/>
		public string TestFrameworkDisplayName { get; }

		public static TestableTestAssemblyStarting Create(
			string? assemblyPath = null,
			string? configFilePath = null,
			DateTime? startTime = null,
			string targetFramework = "target-framework",
			string testEnvironment = "test-env",
			string testFrameworkDisplayName = "test-framework")
		{
			var attr = v2Mocks.TargetFrameworkAttribute(targetFramework);
			var attrs = new[] { attr };
			return new TestableTestAssemblyStarting(
				Enumerable.Empty<ITestCase>(),
				v2Mocks.TestAssembly(assemblyPath ?? osSpecificAssemblyPath, configFilePath, attributes: attrs),
				startTime ?? new DateTime(2020, 11, 3, 17, 55, 0, DateTimeKind.Utc),
				testEnvironment,
				testFrameworkDisplayName
			);
		}
	}
}
