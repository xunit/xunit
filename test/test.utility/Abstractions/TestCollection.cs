using System;
using System.Linq;
using Xunit.Abstractions;

public class TestCollection : ITestCollection
{
    public ITypeInfo CollectionDefinition { get; set; }
    public string DisplayName { get; set; }
}