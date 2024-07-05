using Xunit.Internal;

namespace Xunit;

/// <summary>
/// Extension methods for <see cref="TheoryDataRow"/>.
/// </summary>
public static class TheoryDataRowExtensions
{
	/// <summary>
	/// Sets the explicit flag on a theory data row.
	/// </summary>
	/// <param name="theoryDataRow">The theory data row</param>
	/// <param name="explicit">The value to use for whether this row is explicit</param>
	public static TDerived WithExplicit<TDerived>(
		this TDerived theoryDataRow,
		bool? @explicit)
			where TDerived : TheoryDataRow
	{
		Guard.ArgumentNotNull(theoryDataRow).Explicit = @explicit;
		return theoryDataRow;
	}

	/// <summary>
	/// Sets the skip reason on a theory data row.
	/// </summary>
	/// <param name="theoryDataRow">The theory data row</param>
	/// <param name="skip">The value to use for the skip reason; pass <c>null</c> to not skip</param>
	public static TDerived WithSkip<TDerived>(
		this TDerived theoryDataRow,
		string? skip)
			where TDerived : TheoryDataRow
	{
		Guard.ArgumentNotNull(theoryDataRow).Skip = skip;
		return theoryDataRow;
	}

	/// <summary>
	/// Sets the test display name on a theory data row.
	/// </summary>
	/// <param name="theoryDataRow">The theory data row</param>
	/// <param name="testDisplayName">The value to use for the test display name; pass <c>null</c>
	/// to use the default display name</param>
	public static TDerived WithTestDisplayName<TDerived>(
		this TDerived theoryDataRow,
		string? testDisplayName)
			where TDerived : TheoryDataRow
	{
		Guard.ArgumentNotNull(theoryDataRow).TestDisplayName = testDisplayName;
		return theoryDataRow;
	}

	/// <summary>
	/// Sets the timeout on a theory data row.
	/// </summary>
	/// <param name="theoryDataRow">The theory data row</param>
	/// <param name="timeout">The value to use for timeout, in seconds; pass <c>null</c> to not
	/// have a timeout</param>
	public static TDerived WithTimeout<TDerived>(
		this TDerived theoryDataRow,
		int? timeout)
			where TDerived : TheoryDataRow
	{
		Guard.ArgumentNotNull(theoryDataRow).Timeout = timeout;
		return theoryDataRow;
	}

	/// <summary>
	/// Adds a single trait name/value pair to the theory data row.
	/// </summary>
	/// <param name="theoryDataRow">The theory data row</param>
	/// <param name="name">The name of the trait</param>
	/// <param name="value">The value of the trait</param>
	public static TDerived WithTrait<TDerived>(
		this TDerived theoryDataRow,
		string name,
		string value)
			where TDerived : TheoryDataRow
	{
		var list = Guard.ArgumentNotNull(theoryDataRow).Traits.AddOrGet(name, () => []);
		list.Add(value);
		return theoryDataRow;
	}
}
