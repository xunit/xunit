#pragma warning disable CA1813 // This attribute is unsealed because it's an extensibility point

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Threading.Tasks;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit;

/// <summary>
/// Provides a data source for a data theory, with the data coming from a class
/// which must implement IEnumerable&lt;object?[]&gt;.
/// </summary>
[DataDiscoverer(typeof(ClassDataDiscoverer))]
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public class ClassDataAttribute : DataAttribute
{
	/// <summary>
	/// Initializes a new instance of the <see cref="ClassDataAttribute"/> class.
	/// </summary>
	/// <param name="class">The class that provides the data.</param>
	public ClassDataAttribute(Type @class)
	{
		Class = @class;
	}

	/// <summary>
	/// Gets the type of the class that provides the data.
	/// </summary>
	public Type Class { get; }

	/// <inheritdoc/>
	protected override ITheoryDataRow ConvertDataRow(
		MethodInfo testMethod,
		object dataRow)
	{
		Guard.ArgumentNotNull(testMethod);
		Guard.ArgumentNotNull(dataRow);

		try
		{
			return base.ConvertDataRow(testMethod, dataRow);
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
	public override async ValueTask<IReadOnlyCollection<ITheoryDataRow>?> GetData(
		MethodInfo testMethod,
		DisposalTracker disposalTracker)
	{
		Guard.ArgumentNotNull(testMethod);
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
					result.Add(ConvertDataRow(testMethod, dataItem));

			return result.CastOrToReadOnlyCollection();
		}

		if (classInstance is IAsyncEnumerable<object?> asyncDataItems)
		{
			var result = new List<ITheoryDataRow>();

			await foreach (var dataItem in asyncDataItems)
				if (dataItem is not null)
					result.Add(ConvertDataRow(testMethod, dataItem));

			return result.CastOrToReadOnlyCollection();
		}

		throw new ArgumentException(
			string.Format(
				CultureInfo.CurrentCulture,
				"'{0}' must implement one of the following interfaces to be used as ClassData for the test method named '{1}' on '{2}':{3}- IEnumerable<ITheoryDataRow>{3}- IEnumerable<object[]>{3}- IAsyncEnumerable<ITheoryDataRow>{3}- IAsyncEnumerable<object[]>",
				Class.FullName,
				testMethod.Name,
				testMethod.DeclaringType?.FullName,
				Environment.NewLine
			)
		);
	}
}
