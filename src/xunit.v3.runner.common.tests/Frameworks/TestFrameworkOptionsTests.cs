using Xunit;
using Xunit.Runner.Common;
using Xunit.Sdk;

public static class TestFrameworkOptionsTests
{
	public static class DiscoveryOptions
	{
		public static class Serialization
		{
			[Fact]
			public static void DefaultOptions_EmptyJson()
			{
				ITestFrameworkDiscoveryOptions options = TestFrameworkOptions.Empty();

				var result = options.ToJson();

				Assert.Equal("{}", result);
			}

			[Fact]
			public static void EmptyJson_DeserializesToDefaultValues()
			{
				var result = TestFrameworkOptions.ForDiscoveryFromSerialization("{}");

				Assert.Null(result.GetCulture());
				Assert.Null(result.GetDiagnosticMessages());
				Assert.Null(result.GetIncludeSourceInformation());
				Assert.Null(result.GetInternalDiagnosticMessages());
				Assert.Null(result.GetMethodDisplay());
				Assert.Null(result.GetMethodDisplayOptions());
				Assert.Null(result.GetPreEnumerateTheories());
				Assert.Null(result.GetPrintMaxEnumerableLength());
				Assert.Null(result.GetPrintMaxObjectDepth());
				Assert.Null(result.GetPrintMaxObjectMemberCount());
				Assert.Null(result.GetPrintMaxStringLength());
				Assert.Null(result.GetSynchronousMessageReporting());
			}

			[Fact]
			public static void SettingValue_RoundTripsValue()
			{
				// Set one of each known supported CLR type
				var overrideEnumValue = TestMethodDisplayOptions.ReplaceUnderscoreWithSpace | TestMethodDisplayOptions.UseEscapeSequences;
				ITestFrameworkDiscoveryOptions options = TestFrameworkOptions.Empty();
				options.SetCulture("foo");
				options.SetDiagnosticMessages(true);
				options.SetMethodDisplayOptions(overrideEnumValue);
				options.SetPrintMaxEnumerableLength(2112);
				var serialized = options.ToJson();

				var deserialized = TestFrameworkOptions.ForDiscoveryFromSerialization(serialized);

				Assert.Equal("foo", deserialized.GetCulture());
				Assert.True(deserialized.GetDiagnosticMessages());
				Assert.Equal(overrideEnumValue, deserialized.GetMethodDisplayOptions());
				Assert.Equal(2112, deserialized.GetPrintMaxEnumerableLength());
			}
		}
	}

	public static class ExecutionOptions
	{
		public static class Serialization
		{
			[Fact]
			public static void DefaultOptions_EmptyJson()
			{
				ITestFrameworkExecutionOptions options = TestFrameworkOptions.Empty();

				var result = options.ToJson();

				Assert.Equal("{}", result);
			}

			[Fact]
			public static void EmptyJson_DeserializesToDefaultValues()
			{
				var result = TestFrameworkOptions.ForExecutionFromSerialization("{}");

				Assert.Null(result.GetAssertEquivalentMaxDepth());
				Assert.Null(result.GetCulture());
				Assert.Null(result.GetDiagnosticMessages());
				Assert.Null(result.GetInternalDiagnosticMessages());
				Assert.Null(result.GetMaxParallelThreads());
				Assert.Null(result.GetParallelAlgorithm());
				Assert.Null(result.GetParallelMode());
				Assert.Null(result.GetPrintMaxEnumerableLength());
				Assert.Null(result.GetPrintMaxObjectDepth());
				Assert.Null(result.GetPrintMaxObjectMemberCount());
				Assert.Null(result.GetPrintMaxStringLength());
				Assert.Null(result.GetSynchronousMessageReporting());
			}

			[Fact]
			public static void SettingValue_RoundTripsValue()
			{
				// Set one of each known supported CLR type
				ITestFrameworkExecutionOptions options = TestFrameworkOptions.Empty();
				options.SetCulture("foo");
				options.SetDiagnosticMessages(true);
				options.SetMaxParallelThreads(42);
				options.SetParallelMode(ParallelMode.All);
				options.SetPrintMaxEnumerableLength(2112);
				var serialized = options.ToJson();

				var deserialized = TestFrameworkOptions.ForExecutionFromSerialization(serialized);

				Assert.Equal("foo", deserialized.GetCulture());
				Assert.True(deserialized.GetDiagnosticMessages());
				Assert.Equal(42, deserialized.GetMaxParallelThreads());
				Assert.Equal(ParallelMode.All, deserialized.GetParallelMode());
				Assert.Equal(2112, deserialized.GetPrintMaxEnumerableLength());
			}
		}
	}
}
