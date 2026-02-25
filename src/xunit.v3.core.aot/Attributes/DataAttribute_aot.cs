#pragma warning disable CA1813  // This attribute is unsealed because it's an extensibility point

using System.ComponentModel;
using System.Reflection;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Abstract attribute which is a base class for all other data attributes.
/// </summary>
/// <remarks>
/// Unlike reflection-based data attributes, these must be discovered and data collected during
/// code generation, so any supported attribute must be able to get its data without using
/// reflection.
/// </remarks>
partial class DataAttribute
{
	/// <summary>
	/// Sets a value that determines whether the data rows provided by this data
	/// provider should be considered explicit or not. If <see langword="true"/>, then the data
	/// rows will all be considered explicit; if <see langword="false"/>, then the data rows
	/// will all be considered not explicit; if <see langword="null"/>, then the data rows will
	/// inherit their explicitness from <see cref="FactAttribute"/>.Explicit.
	/// </summary>
	public partial bool Explicit { get; set; }

	/// <summary>
	/// Sets a value that determines whether the data rows provided by this data
	/// provider should be considered explicit or not. If <see langword="true"/>, then the data
	/// rows will all be considered explicit; if <see langword="false"/>, then the data rows
	/// will all be considered not explicit; if unset, then the data rows will
	/// inherit their explicitness from <see cref="FactAttribute"/>.Explicit.
	/// </summary>
	/// <remarks>
	/// .NET does not permit attributes to have nullable value types for settable properties,
	/// so this property is here to be able to detect unset values. Getting <see cref="Explicit"/>
	/// when a value hasn't been set will throw an <see cref="InvalidOperationException"/>.
	/// </remarks>
	protected bool? ExplicitAsNullable { get; set; }

	/// <summary>
	/// Gets a skip reason for all the data rows provided by this data provider. If
	/// not <see langword="null"/>, then all rows will be skipped with the given reason; if <see langword="null"/>,
	/// then the rows will inherit their skip reason from <see cref="FactAttribute"/>.Skip.
	/// </summary>
	public string? Skip { get; set; }

	/// <summary>
	/// Gets the test display name for the test (replacing the default behavior, which
	/// would be to use <see cref="FactAttribute"/>.DisplayName, or falling back to
	/// generating display names based on <see cref="TestMethodDisplay"/> and
	/// <see cref="TestMethodDisplayOptions"/>).
	/// </summary>
	public string? TestDisplayName { get; set; }

	/// <summary>
	/// Sets a value to determine if the data rows provided by this data provider should
	/// include a timeout (in milliseconds). If greater than zero, the data rows will have
	/// the given timeout; if zero or less, the data rows will not have a timeout; if <see langword="null"/>,
	/// the data rows will inherit their timeout from <see cref="FactAttribute"/>.Timeout.
	/// </summary>
	public partial int Timeout { get; set; }

	/// <summary>
	/// Gets or sets a value to determine if the data rows provided by this data provider should
	/// include a timeout (in milliseconds). If greater than zero, the data rows will have
	/// the given timeout; if zero or less, the data rows will not have a timeout; if unset,
	/// the data rows will inherit their timeout from <see cref="FactAttribute"/>.Timeout.
	/// </summary>
	/// <remarks>
	/// .NET Framework does not permit attributes to have nullable value types for settable properties,
	/// so this property is also here to be able to detect unset values. Getting <see cref="Timeout"/>
	/// when a value hasn't been set will throw an <see cref="InvalidOperationException"/>.
	/// </remarks>
	protected int? TimeoutAsNullable { get; set; }

	/// <summary>
	/// Data conversion is done in code generation in Native AOT
	/// </summary>
	[Obsolete("Data conversion is done in code generation in Native AOT", error: true)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected virtual ITheoryDataRow ConvertDataRow(object dataRow) =>
		throw new PlatformNotSupportedException("Data conversion is done in code generation in Native AOT");

	/// <summary>
	/// Data retrieval is performed in code generation in Native AOT
	/// </summary>
	[Obsolete("Data retrieval is performed in code generation in Native AOT", error: true)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public virtual ValueTask<IReadOnlyCollection<ITheoryDataRow>> GetData(
		MethodInfo testMethod,
		DisposalTracker disposalTracker) =>
			throw new PlatformNotSupportedException("Data retrieval is performed in code generation in Native AOT");

	/// <summary>
	/// Data retrieval is performed in code generation in Native AOT
	/// </summary>
	[Obsolete("Data retrieval is performed in code generation in Native AOT", error: true)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public virtual bool SupportsDiscoveryEnumeration() =>
		throw new PlatformNotSupportedException("Data retrieval is performed in code generation in Native AOT");
}
