using System;
using Xunit.Abstractions;

public class TestCollection : ITestCollection
{
    public TestCollection()
    {
        ID = Guid.NewGuid();
    }

    public ITypeInfo CollectionDefinition { get; set; }
    public string DisplayName { get; set; }
    public Guid ID { get; set; }
}