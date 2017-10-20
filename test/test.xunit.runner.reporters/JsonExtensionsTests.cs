using System;
using System.Collections.Generic;
using Xunit;
using Xunit.Runner.Reporters;

public class JsonExtensionsTests
{
    [CulturedFact]
    public void SimpleValues()
    {
        var data = new Dictionary<string, object>
        {
            { "string", "bar" },
            { "int32", 42 },
            { "int64", 42L },
            { "single", 21.12F },
            { "double", 21.12 },
            { "decimal", 21.12M },
            { "boolean", true },
            { "guid", Guid.Empty },
            { "stringWithQuote", "\"bar\"" },
        };

        var result = JsonExtentions.ToJson(data);

        Assert.Equal(@"{""string"":""bar"",""int32"":42,""int64"":42,""single"":21.12,""double"":21.12,""decimal"":21.12,""boolean"":true,""guid"":""00000000-0000-0000-0000-000000000000"",""stringWithQuote"":""\""bar\""""}", result);
    }

    [Fact]
    public void EscapeValues()
    {
        var data = new Dictionary<string, object>
        {
            { "foo", "\x00 \x1f \t \r \n \\ \"Hello!\"" }
        };

        var result = JsonExtentions.ToJson(data);

        Assert.Equal(@"{""foo"":""\u0000 \u001F \t \r \n \\ \""Hello!\""""}", result);
    }
}
