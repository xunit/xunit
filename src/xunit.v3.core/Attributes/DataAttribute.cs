#pragma warning disable CA1033  // This type cannot be sealed because it's abstract

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Threading.Tasks;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Abstract attribute which represents a based implementation of <see cref="IDataAttribute"/>.
/// Data source providers derive from this attribute and implement <see cref="GetData"/>
/// to return the data for the theory.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public abstract class DataAttribute : Attribute, IDataAttribute
{
	static readonly MethodInfo? tupleIndexerGetter;
	static readonly MethodInfo? tupleLengthGetter;
	static readonly Type? tupleType;

	static DataAttribute()
	{
		tupleType = Type.GetType("System.Runtime.CompilerServices.ITuple");
		if (tupleType is null)
			return;

		tupleIndexerGetter = tupleType.GetProperty("Item")?.GetMethod;
		tupleLengthGetter = tupleType.GetProperty("Length")?.GetMethod;
	}

	/// <summary>
	/// Sets a value that determines whether the data rows provided by this data
	/// provider should be considered explicit or not. If <c>true</c>, then the data
	/// rows will all be considered explicit; if <c>false</c>, then the data rows
	/// will all be considered not explicit; if unset, then the data rows will
	/// inherit their explicitness from <see cref="IFactAttribute.Explicit"/>.
	/// </summary>
	public bool Explicit
	{
		[Obsolete("Use ExplicitAsNullable instead")]
		get => ExplicitAsNullable ?? throw new InvalidOperationException("Explicit is unset");
		set => ExplicitAsNullable = value;
	}

	bool? IDataAttribute.Explicit => ExplicitAsNullable;

	/// <summary>
	/// Gettable as a nullable value since .NET Framework does not permit attributes to
	/// have nullable value types for settable properties.
	/// </summary>
	protected bool? ExplicitAsNullable { get; set; }

	/// <inheritdoc/>
	public string? Skip { get; set; }

	// TODO: We don't have SkipType/SkipUnless/SkipWhen here, should we? We'd need to plumb
	// them into IXunitTest since an override during delay enumeration has to be reflected into
	// the test, and can't live just in the test case.

	/// <inheritdoc/>
	public string? TestDisplayName { get; set; }

	/// <summary>
	/// Sets a value to determine if the data rows provided by this data provider should
	/// include a timeout (in milliseconds). If greater than zero, the data rows will have
	/// the given timeout; if zero or less, the data rows will not have a timeout; if unset,
	/// the data rows will inherit their timeout from <see cref="IFactAttribute.Timeout"/>.
	/// </summary>
	public int Timeout
	{
		[Obsolete("Use TimeoutAsNullable instead")]
		get => TimeoutAsNullable ?? throw new InvalidOperationException("Timeout is unset");
		set => TimeoutAsNullable = value;
	}

	int? IDataAttribute.Timeout => TimeoutAsNullable;

	/// <summary>
	/// Gettable as a nullable value since .NET Framework does not permit attributes to
	/// have nullable value types for settable properties.
	/// </summary>
	protected int? TimeoutAsNullable { get; set; }

	/// <inheritdoc/>
	public string[]? Traits { get; set; }

	/// <summary>
	/// Converts an item yielded by the data attribute to an <see cref="ITheoryDataRow"/>, for return
	/// from <see cref="GetData"/>. Items yielded will typically be <c>object[]</c>, <see cref="ITheoryDataRow"/>,
	/// or <see cref="T:System.Runtime.CompilerServices.ITuple"/>, but this override will allow derived
	/// attribute types to support additional data items. If the data item cannot be converted, this method
	/// will throw <see cref="ArgumentException"/>.
	/// </summary>
	/// <param name="dataRow">An item yielded from the data member.</param>
	/// <returns>An <see cref="ITheoryDataRow"/> suitable for return from <see cref="GetData"/>.</returns>
	/// <exception cref="ArgumentException">Thrown when <paramref name="dataRow"/> does not point to a valid data
	/// row (must be compatible with <c>object[]</c> or <see cref="ITheoryDataRow"/>).</exception>
	protected virtual ITheoryDataRow ConvertDataRow(object dataRow)
	{
		Guard.ArgumentNotNull(dataRow);

		if (dataRow is ITheoryDataRow theoryDataRow)
		{
			var dataRowTraits = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);

			if (theoryDataRow.Traits is not null)
				foreach (var kvp in theoryDataRow.Traits)
					dataRowTraits.AddOrGet(kvp.Key).AddRange(kvp.Value);

			MergeTraitsInto(dataRowTraits);

			return new TheoryDataRow(theoryDataRow.GetData())
			{
				Explicit = theoryDataRow.Explicit ?? ExplicitAsNullable,
				Skip = theoryDataRow.Skip ?? Skip,
				TestDisplayName = theoryDataRow.TestDisplayName ?? TestDisplayName,
				Timeout = theoryDataRow.Timeout ?? TimeoutAsNullable,
				Traits = dataRowTraits,
			};
		}

		var traits = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
		MergeTraitsInto(traits);

		if (dataRow is object?[] array)
			return new TheoryDataRow(array)
			{
				Explicit = ExplicitAsNullable,
				Skip = Skip,
				TestDisplayName = TestDisplayName,
				Timeout = TimeoutAsNullable,
				Traits = traits,
			};

		if (tupleType is not null && tupleIndexerGetter is not null && tupleLengthGetter is not null)
		{
			if (tupleType.IsAssignableFrom(dataRow.GetType()))
			{
				var countObj = tupleLengthGetter.Invoke(dataRow, null);
				if (countObj is not null)
				{
					var count = (int)countObj;
					var data = new object?[count];
					for (var idx = 0; idx < count; ++idx)
						data[idx] = tupleIndexerGetter.Invoke(dataRow, [idx]);

					return new TheoryDataRow(data)
					{
						Explicit = ExplicitAsNullable,
						Skip = Skip,
						TestDisplayName = TestDisplayName,
						Timeout = TimeoutAsNullable,
						Traits = traits,
					};
				}
			}
		}

		throw new ArgumentException(
			string.Format(
				CultureInfo.CurrentCulture,
				"Data row of type '{0}' is not an 'object?[]', 'Xunit.ITheoryDataRow' or 'System.Runtime.CompilerServices.ITuple'",
				dataRow.GetType().SafeName()
			),
			nameof(dataRow)
		);
	}

	/// <inheritdoc/>
	public abstract ValueTask<IReadOnlyCollection<ITheoryDataRow>> GetData(
		MethodInfo testMethod,
		DisposalTracker disposalTracker);

	void MergeTraitsInto(Dictionary<string, HashSet<string>> traits) =>
		TestIntrospectionHelper.MergeTraitsInto(traits, Traits);

	/// <inheritdoc/>
	public abstract bool SupportsDiscoveryEnumeration();
}
