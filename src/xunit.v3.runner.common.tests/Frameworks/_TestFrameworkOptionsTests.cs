using Xunit;
using Xunit.Runner.Common;
using Xunit.v3;

public class _TestFrameworkOptionsTests
{
	public class DiscoveryOptions
	{
		public class Serialization
		{
			[Fact]
			public void DefaultOptions_EmptyJson()
			{
				var options = _TestFrameworkOptions.ForDiscovery();

				var result = options.ToJson();

				Assert.Equal("{}", result);
			}

			[Fact]
			public void EmptyJson_DeserializesToDefaultValues()
			{
				var result = _TestFrameworkOptions.ForDiscovery("{}");

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
				var options = _TestFrameworkOptions.ForDiscovery();
				options.SetDiagnosticMessages(true);
				options.SetMethodDisplayOptions(overrideEnumValue);
				var serialized = options.ToJson();

				var deserialized = _TestFrameworkOptions.ForDiscovery(serialized);

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
				var options = _TestFrameworkOptions.ForExecution();

				var result = options.ToJson();

				Assert.Equal("{}", result);
			}

			[Fact]
			public void EmptyJson_DeserializesToDefaultValues()
			{
				var result = _TestFrameworkOptions.ForExecution("{}");

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
				var options = _TestFrameworkOptions.ForExecution();
				options.SetDiagnosticMessages(true);
				options.SetMaxParallelThreads(42);
				var serialized = options.ToJson();

				var deserialized = _TestFrameworkOptions.ForExecution(serialized);

				Assert.True(deserialized.GetDiagnosticMessages());
				Assert.Equal(42, deserialized.GetMaxParallelThreads());
			}
		}
	}
}
