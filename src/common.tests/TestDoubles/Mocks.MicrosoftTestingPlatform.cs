using Microsoft.Testing.Platform.Configurations;

// This file manufactures mocks of Microsoft Testing Platform interfaces
partial class Mocks
{
	public static class MicrosoftTestingPlatform
	{
		public static IConfiguration Configuration(string? resultsPath = null) =>
			new MockConfiguration(resultsPath);

		class MockConfiguration(string? resultsPath) :
			IConfiguration
		{
			public string? this[string key] =>
				key switch
				{
					PlatformConfigurationConstants.PlatformResultDirectory => resultsPath ?? Path.GetTempPath(),
					_ => null,
				};
		}

		// Copied from MTP, because their version is internal rather than public
		static class PlatformConfigurationConstants
		{
			public const string PlatformResultDirectory = "platformOptions:resultDirectory";
		}
	}
}
