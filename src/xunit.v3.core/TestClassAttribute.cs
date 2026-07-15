namespace Xunit;

/// <summary>
/// Optional attribute that is applied to a test class.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public sealed partial class TestClassAttribute : Attribute
{ }
