using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit;

/// <summary>
/// Provides a base class for attributes that will provide member data. The member data must return
/// something compatible with <see cref="IEnumerable"/>.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public abstract class MemberDataAttributeBase : DataAttribute
{
	static readonly Lazy<string> supportedDataSignatures;

	static MemberDataAttributeBase()
	{
		supportedDataSignatures = new(() =>
		{
			var dataSignatures = new List<string>(18);

			foreach (var enumerable in new[] { "IEnumerable<{0}>", "IAsyncEnumerable<{0}>" })
				foreach (var dataType in new[] { "ITheoryDataRow", "object[]", "Tuple<...>" })
					foreach (var wrapper in new[] { "- {0}", "- Task<{0}>", "- ValueTask<{0}>" })
						dataSignatures.Add(string.Format(CultureInfo.CurrentCulture, wrapper, string.Format(CultureInfo.CurrentCulture, enumerable, dataType)));

			return string.Join(Environment.NewLine, dataSignatures);
		});
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="MemberDataAttributeBase"/> class.
	/// </summary>
	/// <param name="memberName">The name of the public static member on the test class that will provide the test data</param>
	/// <param name="arguments">The arguments to be passed to the member (only supported for methods; ignored for everything else)</param>
	protected MemberDataAttributeBase(
		string memberName,
		object?[] arguments)
	{
		MemberName = Guard.ArgumentNotNull(memberName);
		Arguments = Guard.ArgumentNotNull(arguments);
	}

	/// <summary>
	/// Gets or sets the arguments passed to the member. Only supported for static methods.
	/// </summary>
	public object?[] Arguments { get; }

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
					"Member '{0}' on '{1}' yielded an item of type '{2}' which is not an 'object?[]', 'Xunit.ITheoryDataRow' or 'System.Runtime.CompilerServices.ITuple'",
					MemberName,
					MemberType ?? testMethod.DeclaringType,
					dataRow.GetType().SafeName()
				),
				nameof(dataRow)
			);
		}
	}

	/// <inheritdoc/>
	public override ValueTask<IReadOnlyCollection<ITheoryDataRow>?> GetData(
		MethodInfo testMethod,
		DisposalTracker disposalTracker)
	{
		Guard.ArgumentNotNull(testMethod);

		var type = MemberType ?? testMethod.DeclaringType;
		if (type is null)
			return new(default(IReadOnlyCollection<ITheoryDataRow>));

		var accessor = GetPropertyAccessor(type) ?? GetFieldAccessor(type) ?? GetMethodAccessor(type);
		if (accessor is null)
			throw new ArgumentException(
				string.Format(
					CultureInfo.CurrentCulture,
					"Could not find public static member (property, field, or method) named '{0}' on {1}{2}",
					MemberName,
					type.FullName,
					Arguments.Length > 0 ? string.Format(CultureInfo.CurrentCulture, " with parameter types: {0}", string.Join(", ", Arguments.Select(p => p?.GetType().FullName ?? "(null)"))) : ""
				)
			);

		var returnValue = accessor();
		if (returnValue is null)
			return new(default(IReadOnlyCollection<ITheoryDataRow>));

		if (returnValue is IEnumerable dataItems)
		{
			var result = new List<ITheoryDataRow>();

			foreach (var dataItem in dataItems)
				if (dataItem is not null)
					result.Add(ConvertDataRow(testMethod, dataItem));

			return new(result.CastOrToReadOnlyCollection());
		}

		return GetDataAsync(returnValue, testMethod, type);
	}

	async ValueTask<IReadOnlyCollection<ITheoryDataRow>?> GetDataAsync(
		object? returnValue,
		MethodInfo testMethod,
		Type type)
	{
		var taskAwaitable = returnValue.AsValueTask();
		if (taskAwaitable.HasValue)
			returnValue = await taskAwaitable.Value;

		if (returnValue is IAsyncEnumerable<object?> asyncDataItems)
		{
			var result = new List<ITheoryDataRow>();

			await foreach (var dataItem in asyncDataItems)
				if (dataItem is not null)
					result.Add(ConvertDataRow(testMethod, dataItem));

			return result.CastOrToReadOnlyCollection();
		}

		// Duplicate from GetData(), but it's hard to avoid since we need to support Task/ValueTask
		// of IEnumerable (and not just IAsyncEnumerable).
		if (returnValue is IEnumerable dataItems)
		{
			var result = new List<ITheoryDataRow>();

			foreach (var dataItem in dataItems)
				if (dataItem is not null)
					result.Add(ConvertDataRow(testMethod, dataItem));

			return result.CastOrToReadOnlyCollection();
		}

		throw new ArgumentException(
			string.Format(
				CultureInfo.CurrentCulture,
				"Member '{0}' on '{1}' must return data in one of the following formats:{2}{3}",
				MemberName,
				type.FullName,
				Environment.NewLine,
				supportedDataSignatures.Value
			)
		);
	}

	Func<object?>? GetFieldAccessor(Type? type)
	{
		FieldInfo? fieldInfo = null;
		for (var reflectionType = type; reflectionType is not null; reflectionType = reflectionType.BaseType)
		{
			fieldInfo = reflectionType.GetRuntimeField(MemberName);
			if (fieldInfo is not null)
				break;
		}

		if (fieldInfo is null || !fieldInfo.IsStatic)
			return null;

		return () => fieldInfo.GetValue(null);
	}

	Func<object?>? GetMethodAccessor(Type? type)
	{
		MethodInfo? methodInfo = null;
		var argumentTypes = Arguments is null ? Array.Empty<Type>() : Arguments.Select(p => p?.GetType()).ToArray();
		for (var reflectionType = type; reflectionType is not null; reflectionType = reflectionType.BaseType)
		{
			var methodInfoArray =
				reflectionType
					.GetRuntimeMethods()
					.Where(m => m.Name == MemberName && ParameterTypesCompatible(m.GetParameters(), argumentTypes))
					.ToArray();
			if (methodInfoArray.Length == 0)
				continue;
			if (methodInfoArray.Length == 1)
			{
				methodInfo = methodInfoArray[0];
				break;
			}
			methodInfo = methodInfoArray.Where(m => m.GetParameters().Length == argumentTypes.Length).FirstOrDefault();
			if (methodInfo is not null)
				break;

			throw new ArgumentException(
				string.Format(
					CultureInfo.CurrentCulture,
					"The call to method '{0}.{1}' is ambigous between {2} different options for the given arguments.",
					type!.SafeName(),
					MemberName,
					methodInfoArray.Length
				),
				nameof(type)
			);
		}

		if (methodInfo is null || !methodInfo.IsStatic)
			return null;

		var completedArguments = Arguments ?? Array.Empty<object?>();
		var finalMethodParameters = methodInfo.GetParameters();

		completedArguments =
			completedArguments.Length == finalMethodParameters.Length
				? completedArguments
				: completedArguments.Concat(finalMethodParameters.Skip(completedArguments.Length).Select(pi => pi.DefaultValue)).ToArray();

		return () => methodInfo.Invoke(null, completedArguments);
	}

	Func<object?>? GetPropertyAccessor(Type? type)
	{
		PropertyInfo? propInfo = null;
		for (var reflectionType = type; reflectionType is not null; reflectionType = reflectionType.BaseType)
		{
			propInfo = reflectionType.GetRuntimeProperty(MemberName);
			if (propInfo is not null)
				break;
		}

		if (propInfo is null || propInfo.GetMethod is null || !propInfo.GetMethod.IsStatic)
			return null;

		return () => propInfo.GetValue(null, null);
	}

	static bool ParameterTypesCompatible(
		ParameterInfo[] parameters,
		Type?[] argumentTypes)
	{
		if (parameters.Length < argumentTypes.Length)
			return false;

		var idx = 0;
		for (; idx < argumentTypes.Length; ++idx)
			if (argumentTypes[idx] is not null && !parameters[idx].ParameterType.IsAssignableFrom(argumentTypes[idx]!))
				return false;

		for (; idx < parameters.Length; ++idx)
			if (!parameters[idx].IsOptional)
				return false;

		return true;
	}
}
