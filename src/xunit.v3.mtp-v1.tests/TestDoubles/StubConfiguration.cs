namespace Microsoft.Testing.Platform.Configurations;

public class StubConfiguration : IConfiguration
{
	public string? this[string key]
	{
		get
		{
			if (key == "platformOptions:resultDirectory")
				return "/path/to/results";

			return null;
		}
	}
}
