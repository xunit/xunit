using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit
{
	/// <summary>
	/// Provides a data source for a data theory, with the data coming from a class
	/// which must implement IEnumerable&lt;object?[]&gt;.
	/// </summary>
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
		public Type Class { get; private set; }

		/// <inheritdoc/>
		protected override ITheoryDataRow ConvertDataItem(MethodInfo testMethod, object? item) =>
			base.ConvertDataItem(testMethod, item)
				?? throw new ArgumentException($"Class '{Class.FullName}' yielded an item that is not an 'ITheoryDataRow' or 'object?[]'");

		/// <inheritdoc/>
		public override ValueTask<IReadOnlyCollection<ITheoryDataRow>?> GetData(MethodInfo testMethod)
		{
			var classInstance = Activator.CreateInstance(Class);

			if (classInstance is IEnumerable dataItems)
			{
				var result = new List<ITheoryDataRow>();
				foreach (var dataItem in dataItems)
					result.Add(ConvertDataItem(testMethod, dataItem));
				return new(result.CastOrToReadOnlyCollection());
			}

			return GetDataAsync(classInstance, testMethod);
		}

		// Split into a separate method to avoid the async machinery when we don't have async results
		async ValueTask<IReadOnlyCollection<ITheoryDataRow>?> GetDataAsync(
			object? classInstance,
			MethodInfo testMethod)
		{
			if (classInstance is IAsyncEnumerable<object?> asyncDataItems)
			{
				var result = new List<ITheoryDataRow>();
				await foreach (var dataItem in asyncDataItems)
					result.Add(ConvertDataItem(testMethod, dataItem));
				return result.CastOrToReadOnlyCollection();
			}

			throw new ArgumentException(
				$"'{Class.FullName}' must implement one of the following interfaces to be used as ClassData for the test method named '{testMethod.Name}' on '{testMethod.DeclaringType?.FullName}':" + Environment.NewLine +
				"- IEnumerable<ITheoryDataRow>" + Environment.NewLine +
				"- IEnumerable<object[]>" + Environment.NewLine +
				"- IAsyncEnumerable<ITheoryDataRow>" + Environment.NewLine +
				"- IAsyncEnumerable<object[]>"
			);
		}
	}
}
