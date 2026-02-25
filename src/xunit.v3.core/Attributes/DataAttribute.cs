namespace Xunit.v3;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public abstract partial class DataAttribute : Attribute
{
	public partial bool Explicit
	{
		[Obsolete("Use ExplicitAsNullable instead")]
		get => ExplicitAsNullable ?? throw new InvalidOperationException("Explicit is unset");
		set => ExplicitAsNullable = value;
	}

	/// <summary>
	/// Gets the label to use for the data row. This value is used to help format the display name
	/// of the test.
	/// </summary>
	/// <remarks>
	/// * If the value is <see langword="null"/> (or not set), use the default behavior: <c>MethodName(...argument list...)</c><br/>
	/// * If the value is an empty string, use just the method name: <c>MethodName</c><br/>
	/// * For any other values, appends the label: <c>MethodName [label]</c>
	/// </remarks>
	public string? Label { get; set; }

	/// <summary>
	/// Gets the type to retrieve <see cref="SkipUnless"/> or <see cref="SkipWhen"/> from. If not set,
	/// then the property will be retrieved from the unit test class.
	/// </summary>
	public Type? SkipType { get; set; }

	/// <summary>
	/// Gets the name of a public static property on the test class which returns <see cref="bool"/>
	/// to indicate whether the test should be skipped (<see langword="false"/>) or not (<see langword="true"/>).
	/// </summary>
	/// <remarks>
	/// This property cannot be set if <see cref="SkipWhen"/> is set. Setting both will
	/// result in a failed test.
	/// To ensure compile-time safety and easier refactoring, use the <see langword="nameof"/> operator,
	/// e.g., <c>SkipUnless = nameof(IsConditionMet)</c>.
	/// </remarks>
	public string? SkipUnless { get; set; }

	/// <summary>
	/// Gets the name of a public static property on the test class which returns <see cref="bool"/>
	/// to indicate whether the test should be skipped (<see langword="true"/>) or not (<see langword="false"/>).
	/// </summary>
	/// <remarks>
	/// This property cannot be set if <see cref="SkipUnless"/> is set. Setting both will
	/// result in a failed test.
	/// To ensure compile-time safety and easier refactoring, use the <see langword="nameof"/> operator
	/// e.g., <c>SkipWhen = nameof(IsConditionMet)</c>.
	/// </remarks>
	public string? SkipWhen { get; set; }

	public partial int Timeout
	{
		[Obsolete("Use TimeoutAsNullable instead")]
		get => TimeoutAsNullable ?? throw new InvalidOperationException("Timeout is unset");
		set => TimeoutAsNullable = value;
	}

	/// <summary>
	/// Gets a set of traits for the associated data. The data is provided as an array
	/// of string values that are alternating keys and values (e.g.,
	/// <c>["key1", "value1", "key2", "value2"]</c>).
	/// </summary>
	/// <remarks>
	/// This is structured as an array because attribute initializers don't support dictionaries. Note:
	/// Setting an odd number of values will throw away the unmatched key at the end of the list. If you
	/// seem to be missing your a key/value pair or have misaligned keys and values, make sure you have
	/// an even number of strings alternating between keys and values.
	/// </remarks>
	public string[]? Traits { get; set; }
}
