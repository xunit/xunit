#pragma warning disable CA1813 // This attribute is unsealed because it's an extensibility point

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Threading.Tasks;
using Xunit.Internal;
using Xunit.Sdk;
using Xunit.v3;

namespace Xunit;

/// <summary>
/// Provides a data source for a data theory, with the data coming from a class
/// which must implement IEnumerable&lt;object?[]&gt;.
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
