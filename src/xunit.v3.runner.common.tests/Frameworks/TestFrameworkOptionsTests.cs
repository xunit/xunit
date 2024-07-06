using Xunit;
using Xunit.Runner.Common;
using Xunit.Sdk;

public class TestFrameworkOptionsTests
{
	public class DiscoveryOptions
	{
		public class Serialization
		{
			[Fact]
			public void DefaultOptions_EmptyJson()
			{
				ITestFrameworkDiscoveryOptions options = TestFrameworkOptions.Empty();

				var result = options.ToJson();

				Assert.Equal("{}", result);
			}

			[Fact]
			public void EmptyJson_DeserializesToDefaultValues()
			{
				var result = TestFrameworkOptions.ForDiscoveryFromSerialization("{}");

				Assert.Null(result.GetCulture());
				Assert.Null(result.GetDiagnosticMessages());
				Assert.Null(result.GetIncludeSourceInformation());
				Assert.Null(result.GetInternalDiagnosticMessages());
				Assert.Null(result.GetMethodDisplay());
				Assert.Null(result.GetMethodDisplayOptions());
				Assert.Null(result.GetPreEnumerateTheories());
				Assert.Null(result.GetSynchronousMessageReporting());
			}

			[Fact]
			public void SettingValue_RoundTripsValue()
			{
				// Set one of each known supported CLR type
				var overrideEnumValue = TestMethodDisplayOptions.ReplaceUnderscoreWithSpace | TestMethodDisplayOptions.UseEscapeSequences;
				ITestFrameworkDiscoveryOptions options = TestFrameworkOptions.Empty();
				options.SetCulture("foo");
				options.SetDiagnosticMessages(true);
				options.SetMethodDisplayOptions(overrideEnumValue);
				var serialized = options.ToJson();

				var deserialized = TestFrameworkOptions.ForDiscoveryFromSerialization(serialized);

				Assert.Equal("foo", deserialized.GetCulture());
				Assert.True(deserialized.GetDiagnosticMessages());
				Assert.Equal(overrideEnumValue, deserialized.GetMethodDisplayOptions());
			}
		}
	}

	public class ExecutionOptions
	{
		public class Serialization
		{
			[Fact]
			public void DefaultOptions_EmptyJson()
			{
				ITestFrameworkExecutionOptions options = TestFrameworkOptions.Empty();

				var result = options.ToJson();

				Assert.Equal("{}", result);
			}

			[Fact]
			public void EmptyJson_DeserializesToDefaultValues()
			{
				var result = TestFrameworkOptions.ForExecutionFromSerialization("{}");

				Assert.Null(result.GetCulture());
				Assert.Null(result.GetDiagnosticMessages());
				Assert.Null(result.GetDisableParallelization());
				Assert.Null(result.GetInternalDiagnosticMessages());
				Assert.Null(result.GetMaxParallelThreads());
				Assert.Null(result.GetSynchronousMessageReporting());
			}

			[Fact]
			public void SettingValue_RoundTripsValue()
			{
				// Set one of each known supported CLR type
				ITestFrameworkExecutionOptions options = TestFrameworkOptions.Empty();
				options.SetCulture("foo");
				options.SetDiagnosticMessages(true);
				options.SetMaxParallelThreads(42);
				var serialized = options.ToJson();

				var deserialized = TestFrameworkOptions.ForExecutionFromSerialization(serialized);

				Assert.Equal("foo", deserialized.GetCulture());
				Assert.True(deserialized.GetDiagnosticMessages());
				Assert.Equal(42, deserialized.GetMaxParallelThreads());
			}
		}
	}
}
