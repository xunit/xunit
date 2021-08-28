using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit
{
	/// <summary>
	/// Provides a base class for attributes that will provide member data. The member data must return
	/// something compatible with <see cref="IEnumerable"/>.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
	public abstract class MemberDataAttributeBase : DataAttribute
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="MemberDataAttributeBase"/> class.
		/// </summary>
		/// <param name="memberName">The name of the public static member on the test class that will provide the test data</param>
		/// <param name="parameters">The parameters for the member (only supported for methods; ignored for everything else)</param>
		protected MemberDataAttributeBase(
			string memberName,
			object?[] parameters)
		{
			MemberName = Guard.ArgumentNotNull(nameof(memberName), memberName);
			Parameters = Guard.ArgumentNotNull(nameof(parameters), parameters);
		}

		/// <summary>
		/// Returns <c>true</c> if the data attribute wants to skip enumerating data during discovery.
		/// This will cause the theory to yield a single test case for all data, and the data discovery
		/// will be during test execution instead of discovery.
		/// </summary>
		public bool DisableDiscoveryEnumeration { get; set; }

		/// <summary>
		/// Gets the member name.
		/// </summary>
		public string MemberName { get; }

		/// <summary>
		/// Gets or sets the type to retrieve the member from. If not set, then the property will be
		/// retrieved from the unit test class.
		/// </summary>
		public Type? MemberType { get; set; }

		/// <summary>
		/// Gets or sets the parameters passed to the member. Only supported for static methods.
		/// </summary>
		public object?[] Parameters { get; }

		/// <inheritdoc/>
		protected override ITheoryDataRow ConvertDataItem(MethodInfo testMethod, object? item) =>
			base.ConvertDataItem(testMethod, item)
				?? throw new ArgumentException($"Member '{MemberName}' on '{MemberType ?? testMethod.DeclaringType}' yielded an item that is not an 'ITheoryDataRow' or 'object?[]'");

		/// <inheritdoc/>
		public override ValueTask<IReadOnlyCollection<ITheoryDataRow>?> GetData(MethodInfo testMethod)
		{
			Guard.ArgumentNotNull("testMethod", testMethod);

			var type = MemberType ?? testMethod.DeclaringType;
			if (type == null)
				return new(default(IReadOnlyCollection<ITheoryDataRow>));

			var accessor = GetPropertyAccessor(type) ?? GetFieldAccessor(type) ?? GetMethodAccessor(type);
			if (accessor == null)
			{
				var parameterText = Parameters?.Length > 0 ? $" with parameter types: {string.Join(", ", Parameters.Select(p => p?.GetType().FullName ?? "(null)"))}" : "";
				throw new ArgumentException($"Could not find public static member (property, field, or method) named '{MemberName}' on {type.FullName}{parameterText}");
			}

			var returnValue = accessor();
			if (returnValue is null)
				return new(default(IReadOnlyCollection<ITheoryDataRow>));

			if (returnValue is IEnumerable dataItems)
			{
				var result = new List<ITheoryDataRow>();
				foreach (var dataItem in dataItems)
					result.Add(ConvertDataItem(testMethod, dataItem));
				return new(result.CastOrToReadOnlyCollection());
			}

			return GetDataAsync(returnValue, testMethod, type);
		}

		async ValueTask<IReadOnlyCollection<ITheoryDataRow>?> GetDataAsync(
			object? returnValue,
			MethodInfo testMethod,
			Type type)
		{
			var taskAwaitable = returnValue.AsTask();
			if (taskAwaitable != null)
				returnValue = await taskAwaitable;

			if (returnValue is IAsyncEnumerable<object?> asyncDataItems)
			{
				var result = new List<ITheoryDataRow>();
				await foreach (var dataItem in asyncDataItems)
					result.Add(ConvertDataItem(testMethod, dataItem));
				return result.CastOrToReadOnlyCollection();
			}

			// Duplicate from GetData(), but it's hard to avoid since we need to support Task/ValueTask
			// of IEnumerable (and not just IAsyncEnumerable).
			if (returnValue is IEnumerable dataItems)
			{
				var result = new List<ITheoryDataRow>();
				foreach (var dataItem in dataItems)
					result.Add(ConvertDataItem(testMethod, dataItem));
				return result.CastOrToReadOnlyCollection();
			}

			throw new ArgumentException(
				$"Member '{MemberName}' on '{type.FullName}' must return data in one of the following formats:" + Environment.NewLine +
				"- IEnumerable<ITheoryDataRow>" + Environment.NewLine +
				"- Task<IEnumerable<ITheoryDataRow>>" + Environment.NewLine +
				"- ValueTask<IEnumerable<ITheoryDataRow>>" + Environment.NewLine +
				"- IEnumerable<object[]>" + Environment.NewLine +
				"- Task<IEnumerable<object[]>>" + Environment.NewLine +
				"- ValueTask<IEnumerable<object[]>>" + Environment.NewLine +
				"- IAsyncEnumerable<ITheoryDataRow>" + Environment.NewLine +
				"- Task<IAsyncEnumerable<ITheoryDataRow>>" + Environment.NewLine +
				"- ValueTask<IAsyncEnumerable<ITheoryDataRow>>" + Environment.NewLine +
				"- IAsyncEnumerable<object[]>" + Environment.NewLine +
				"- Task<IAsyncEnumerable<object[]>>" + Environment.NewLine +
				"- ValueTask<IAsyncEnumerable<object[]>>"
			);
		}

		Func<object?>? GetFieldAccessor(Type? type)
		{
			FieldInfo? fieldInfo = null;
			for (var reflectionType = type; reflectionType != null; reflectionType = reflectionType.BaseType)
			{
				fieldInfo = reflectionType.GetRuntimeField(MemberName);
				if (fieldInfo != null)
					break;
			}

			if (fieldInfo == null || !fieldInfo.IsStatic)
				return null;

			return () => fieldInfo.GetValue(null);
		}

		Func<object?>? GetMethodAccessor(Type? type)
		{
			MethodInfo? methodInfo = null;
			var parameterTypes = Parameters == null ? new Type[0] : Parameters.Select(p => p?.GetType()).ToArray();
			for (var reflectionType = type; reflectionType != null; reflectionType = reflectionType.BaseType)
			{
				methodInfo =
					reflectionType
						.GetRuntimeMethods()
						.FirstOrDefault(m => m.Name == MemberName && ParameterTypesCompatible(m.GetParameters(), parameterTypes));

				if (methodInfo != null)
					break;
			}

			if (methodInfo == null || !methodInfo.IsStatic)
				return null;

			return () => methodInfo.Invoke(null, Parameters);
		}

		Func<object?>? GetPropertyAccessor(Type? type)
		{
			PropertyInfo? propInfo = null;
			for (var reflectionType = type; reflectionType != null; reflectionType = reflectionType.BaseType)
			{
				propInfo = reflectionType.GetRuntimeProperty(MemberName);
				if (propInfo != null)
					break;
			}

			if (propInfo == null || propInfo.GetMethod == null || !propInfo.GetMethod.IsStatic)
				return null;

			return () => propInfo.GetValue(null, null);
		}

		static bool ParameterTypesCompatible(
			ParameterInfo[]? parameters,
			Type?[] parameterTypes)
		{
			if (parameters?.Length != parameterTypes.Length)
				return false;

			for (var idx = 0; idx < parameters.Length; ++idx)
				if (parameterTypes[idx] != null && !parameters[idx].ParameterType.IsAssignableFrom(parameterTypes[idx]!))
					return false;

			return true;
		}
	}
}
