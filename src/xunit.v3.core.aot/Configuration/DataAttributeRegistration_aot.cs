using System.Collections;
using System.Runtime.CompilerServices;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// This class acts as a container for the common properties from <see cref="DataAttribute"/>, and can
/// manufacture instances of <see cref="ITheoryDataRow"/>.
/// </summary>
public class DataAttributeRegistration
{
	/// <summary>
	/// Gets an empty (default) data attribute registration.
	/// </summary>
	public static DataAttributeRegistration Empty { get; } = new();

	/// <summary>
	/// Gets a flag that indicates that the data row should only be run explicitly. If the value is <see langword="null"/>,
	/// then it inherits its explicitness from the value of <see cref="FactAttribute"/>.Explicit.
	/// </summary>
	public bool? Explicit { get; set; }

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
	/// Gets the skip reason for the test. When <see langword="null"/> is returned, the test is
	/// not skipped.
	/// </summary>
	/// <remarks>
	/// Skipping is conditional based on whether <see cref="SkipWhen"/> or <see cref="SkipUnless"/>
	/// is set.
	/// </remarks>
	public string? Skip { get; set; }

	/// <summary>
	/// Gets a function which indicates whether the test should be skipped (<see langword="false"/>)
	/// or not (<see langword="true"/>).
	/// </summary>
	/// <remarks>
	/// This property cannot be set if <see cref="SkipWhen"/> is set. Setting both will
	/// result in a failed test.
	/// </remarks>
	public Func<bool>? SkipUnless { get; set; }

	/// <summary>
	/// Gets a function which indicates whether the test should be skipped (<see langword="true"/>)
	/// or not (<see langword="false"/>).
	/// </summary>
	/// <remarks>
	/// This property cannot be set if <see cref="SkipUnless"/> is set. Setting both will
	/// result in a failed test.
	/// </remarks>
	public Func<bool>? SkipWhen { get; set; }

	/// <summary>
	/// Gets the display name for the test (replacing the default behavior, which would be to
	/// use <see cref="DataAttribute.TestDisplayName"/> or <see cref="FactAttribute"/>.DisplayName,
	/// or falling back to the default test display name based on <see cref="TestMethodDisplay"/>
	/// and <see cref="TestMethodDisplayOptions"/> in the configuration file).
	/// </summary>
	public string? TestDisplayName { get; set; }

	/// <summary>
	/// A value greater than zero marks the test as having a timeout, and gets or sets the
	/// timeout (in milliseconds). A non-<see langword="null"/> value here overrides any inherited value
	/// from the <see cref="DataAttribute"/> or the <see cref="TheoryAttribute"/>.
	/// </summary>
	/// <remarks>
	/// WARNING: Using this with parallelization turned on will result in undefined behavior.
	/// Timeout is only supported when parallelization is disabled, either globally or with
	/// a parallelization-disabled test collection.
	/// </remarks>
	public int? Timeout { get; set; }

	/// <summary>
	/// Gets the trait values associated with this theory data row. If there are none, you may either
	/// return a <see langword="null"/> or empty dictionary.
	/// </summary>
	public IReadOnlyDictionary<string, IReadOnlyCollection<string>>? Traits { get; set; }

	/// <summary>
	/// Attempts to create a data row from an object of unknown provenance. Used to support older
	/// data sources that might just return non-generic <see cref="IEnumerable"/> or
	/// <see cref="IEnumerable{T}"/> of <see cref="object"/>, because they return rows of varying
	/// types (mixed <c><see cref="object"/>?[]</c>, <c><see cref="ITheoryDataRow"/></c>,
	/// and/or <c><see cref="ITuple"/></c>).
	/// </summary>
	[OverloadResolutionPriority(-1)]
	public ITheoryDataRow CreateDataRow(object? data)
	{
		if (data is ITheoryDataRow dataRow)
			return CreateDataRow(dataRow);
		if (data is ITuple tuple)
			return CreateDataRow(tuple);
		if (data is object?[] array)
			return CreateDataRow(array);

		if (data is null)
			throw new TestPipelineException("Null data returned from data row (must be one of object?[], ITheoryDataRow, or ITuple)");

		throw new TestPipelineException(
			string.Format(
				CultureInfo.CurrentCulture,
				"Unknown data type returned from data row '{0}' (must be one of object?[], ITheoryDataRow, or ITuple)",
				data.GetType().SafeName()
			)
		);
	}

	/// <summary>
	/// Creates a data row from the given data values.
	/// </summary>
	public ITheoryDataRow CreateDataRow(object?[] data)
	{
		Guard.ArgumentNotNull(data);

		var traits = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
		MergeTraitsInto(traits);

		return new TheoryDataRow(data)
		{
			Explicit = Explicit,
			Label = Label,
			Skip = Skip,
			SkipUnless = SkipUnless,
			SkipWhen = SkipWhen,
			TestDisplayName = TestDisplayName,
			Timeout = Timeout,
			Traits = traits,
		};
	}

	/// <summary>
	/// Creates a data row from the given data values.
	/// </summary>
	public ITheoryDataRow CreateDataRow(ITheoryDataRow dataRow)
	{
		Guard.ArgumentNotNull(dataRow);

		var traits = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);

		if (dataRow.Traits is not null)
			foreach (var kvp in dataRow.Traits)
				traits.AddOrGet(kvp.Key).AddRange(kvp.Value);

		MergeTraitsInto(traits);

		return new TheoryDataRow(dataRow.GetData())
		{
			Explicit = dataRow.Explicit ?? Explicit,
			Label = dataRow.Label ?? Label,
			Skip = dataRow.Skip ?? Skip,
			SkipUnless = dataRow.SkipUnless ?? SkipUnless,
			SkipWhen = dataRow.SkipWhen ?? SkipWhen,
			TestDisplayName = dataRow.TestDisplayName ?? TestDisplayName,
			Timeout = dataRow.Timeout ?? Timeout,
			Traits = traits,
		};
	}

	/// <summary>
	/// Creates a data row from the given data values.
	/// </summary>
	public ITheoryDataRow CreateDataRow(ITuple tuple)
	{
		Guard.ArgumentNotNull(tuple);

		var count = tuple.Length;
		var data = new object?[count];
		for (var idx = 0; idx < count; ++idx)
			data[idx] = tuple[idx];

		return CreateDataRow(data);
	}

	void MergeTraitsInto(Dictionary<string, HashSet<string>> traits)
	{
		if (Traits is not null)
			foreach (var kvp in Traits)
				foreach (var value in kvp.Value)
					traits.AddOrGet(kvp.Key).Add(value);
	}
}
