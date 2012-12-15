using Xunit;

public class MockFactAttribute : CustomAttributeData<FactAttribute>
{
    public MockFactAttribute(string displayName = null, string skip = null, int timeout = 0)
    {
        AddNamedArgument("DisplayName", displayName);
        AddNamedArgument("Skip", skip);
        AddNamedArgument("Timeout", timeout);
    }
}