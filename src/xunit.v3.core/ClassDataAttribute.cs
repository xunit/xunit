#pragma warning disable CA1813 // This attribute is unsealed because it's an extensibility point

using System.Collections;
using System.Reflection;
using Xunit.Sdk;
using Xunit.v3;

namespace Xunit;

/// <summary>
/// Provides a data source for a data theory, with the data coming from a class
/// which must implement <see cref="IEnumerable{T}"/> or <see cref="IAsyncEnumerable{T}"/>
/// of one of:
/// <list type="bullet">
/// <item><c><see cref="object"/>?[]</c></item>
/// <item><c><see cref="ITheoryDataRow"/></c></item>
/// <item><c><see cref="T:System.Runtime.CompilerServices.ITuple"/></c></item>
/// </list>
/// </summary>
/// <param name="class">The class that provides the data.</param>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public class ClassDataAttribute(Type @class) : DataAttribute
{
	/// <summary>
	/// Gets the type of the class that provides the data.
	/// </summary>
	public Type Class { get; } = @class;

	/// <inheritdoc/>
	protected override ITheoryDataRow ConvertDataRow(object dataRow)
	{
		Guard.ArgumentNotNull(dataRow);

		try
		{
			return base.ConvertDataRow(dataRow);
		}
		catch (ArgumentException)
		{
			throw new ArgumentException(
				string.Format(
					CultureInfo.CurrentCulture,
					"Class '{0}' yielded an item of type '{1}' which is not an 'object?[]', 'Xunit.ITheoryDataRow' or 'System.Runtime.CompilerServices.ITuple'",
					Class.FullName,
					dataRow?.GetType().SafeName()
				),
				nameof(dataRow)
			);
		}
	}

	/// <inheritdoc/>
	public override async ValueTask<IReadOnlyCollection<ITheoryDataRow>> GetData(
		MethodInfo testMethod,
		DisposalTracker disposalTracker)
	{
		Guard.ArgumentNotNull(disposalTracker);

		var classInstance = Activator.CreateInstance(Class);
		disposalTracker.Add(classInstance);

		if (classInstance is IAsyncLifetime classLifetime)
			await classLifetime.InitializeAsync();

		if (classInstance is IEnumerable dataItems)
		{
			var result = new List<ITheoryDataRow>();

			foreach (var dataItem in dataItems)
				if (dataItem is not null)
					result.Add(ConvertDataRow(dataItem));

			return result.CastOrToReadOnlyCollection();
		}

		if (classInstance is IAsyncEnumerable<object?> asyncDataItems)
		{
			var result = new List<ITheoryDataRow>();

			await foreach (var dataItem in asyncDataItems)
				if (dataItem is not null)
					result.Add(ConvertDataRow(dataItem));

			return result.CastOrToReadOnlyCollection();
		}

		throw new ArgumentException(
			string.Format(
				CultureInfo.CurrentCulture,
				"'{0}' must implement one of the following interfaces to be used as ClassData:{1}- IEnumerable<ITheoryDataRow>{1}- IEnumerable<object[]>{1}- IAsyncEnumerable<ITheoryDataRow>{1}- IAsyncEnumerable<object[]>",
				Class.FullName,
				Environment.NewLine
			)
		);
	}

	/// <inheritdoc/>
	public override bool SupportsDiscoveryEnumeration() =>
		!typeof(IDisposable).IsAssignableFrom(Class) && !typeof(IAsyncDisposable).IsAssignableFrom(Class);
}

/// <summary>
/// Provides a data source for a data theory, with the data coming from a class
/// which must implement <see cref="IEnumerable{T}"/> or <see cref="IAsyncEnumerable{T}"/>
/// of one of:
/// <list type="bullet">
/// <item><c><see cref="object"/>?[]</c></item>
/// <item><c><see cref="ITheoryDataRow"/></c></item>
/// <item><c><see cref="T:System.Runtime.CompilerServices.ITuple"/></c></item>
/// </list>
/// </summary>
/// <typeparam name="TClass">The class that provides the data.</typeparam>
/// <remarks>
/// .NET Framework does not support generic attributes. Please use the non-generic <see cref="ClassDataAttribute"/>
/// when targeting .NET Framework.
/// </remarks>
public class ClassDataAttribute<TClass>() :
	ClassDataAttribute(typeof(TClass))
{ }
