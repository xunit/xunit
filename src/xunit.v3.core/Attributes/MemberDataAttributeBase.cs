using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Provides a base class for attributes that will provide member data.
/// </summary>
/// <param name="memberName">
/// The name of the public static member on the test class that will provide the test data
/// It is recommended to use the <c>nameof</c> operator to ensure compile-time safety, e.g., <c>nameof(SomeMemberName)</c>.
/// </param>
/// <param name="arguments">The arguments to be passed to the member (only supported for methods; ignored for everything else)</param>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public abstract class MemberDataAttributeBase(
	string memberName,
	object?[] arguments) :
		DataAttribute, ITypeAwareDataAttribute
{
	static readonly Lazy<string> supportedDataSignatures;

	static MemberDataAttributeBase() =>
		supportedDataSignatures = new(() =>
		{
			var dataSignatures = new List<string>(18);

			foreach (var enumerable in new[] { "IEnumerable<{0}>", "IAsyncEnumerable<{0}>" })
				foreach (var dataType in new[] { "ITheoryDataRow", "object[]", "Tuple<...>" })
					foreach (var wrapper in new[] { "- {0}", "- Task<{0}>", "- ValueTask<{0}>" })
						dataSignatures.Add(string.Format(CultureInfo.CurrentCulture, wrapper, string.Format(CultureInfo.CurrentCulture, enumerable, dataType)));

			return string.Join(Environment.NewLine, dataSignatures);
		});

	/// <summary>
	/// Gets or sets the arguments passed to the member. Only supported for static methods.
	/// </summary>
	public object?[] Arguments { get; } = Guard.ArgumentNotNull(arguments);

	/// <summary>
	/// Returns <c>true</c> if the data attribute wants to skip enumerating data during discovery.
	/// This will cause the theory to yield a single test case for all data, and the data discovery
	/// will be during test execution instead of discovery.
	/// </summary>
	public bool DisableDiscoveryEnumeration { get; set; }

	/// <summary>
	/// Gets the member name.
	/// </summary>
	public string MemberName { get; } = Guard.ArgumentNotNull(memberName);

	/// <summary>
	/// Gets or sets the type to retrieve the member from. If not set, then the member will be
	/// retrieved from the unit test class.
	/// </summary>
	public Type? MemberType { get; set; }

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
					"Member '{0}' on '{1}' yielded an item of type '{2}' which is not an 'object?[]', 'Xunit.ITheoryDataRow' or 'System.Runtime.CompilerServices.ITuple'",
					MemberName,
					MemberType?.SafeName(),
					dataRow.GetType().SafeName()
				),
				nameof(dataRow)
			);
		}
	}

	/// <inheritdoc/>
	public override ValueTask<IReadOnlyCollection<ITheoryDataRow>> GetData(
		MethodInfo testMethod,
		DisposalTracker disposalTracker)
	{
		if (MemberType is null)
			return new([]);

		var accessor =
			GetPropertyAccessor(MemberType)
				?? GetFieldAccessor(MemberType)
				?? GetMethodAccessor(MemberType)
				?? throw new ArgumentException(
					string.Format(
						CultureInfo.CurrentCulture,
						"Could not find public static member (property, field, or method) named '{0}' on '{1}'{2}",
						MemberName,
						MemberType.SafeName(),
						Arguments.Length > 0 ? string.Format(CultureInfo.CurrentCulture, " with parameter types: {0}", string.Join(", ", Arguments.Select(p => p?.GetType().SafeName() ?? "(null)"))) : ""
					)
				);

		var returnValue =
			accessor()
				?? throw new ArgumentException(
					string.Format(
						CultureInfo.CurrentCulture,
						"Member '{0}' on '{1}' returned null when queried for test data",
						MemberName,
						MemberType.SafeName()
					)
				);

		if (returnValue is IEnumerable dataItems)
		{
			var result = new List<ITheoryDataRow>();

			foreach (var dataItem in dataItems)
				if (dataItem is not null)
					result.Add(ConvertDataRow(dataItem));

			return new(result.CastOrToReadOnlyCollection());
		}

		return GetDataAsync(returnValue, MemberType);
	}

	async ValueTask<IReadOnlyCollection<ITheoryDataRow>> GetDataAsync(
		object? returnValue,
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
					result.Add(ConvertDataRow(dataItem));

			return result.CastOrToReadOnlyCollection();
		}

		// Duplicate from GetData(), but it's hard to avoid since we need to support Task/ValueTask
		// of IEnumerable (and not just IAsyncEnumerable).
		if (returnValue is IEnumerable dataItems)
		{
			var result = new List<ITheoryDataRow>();

			foreach (var dataItem in dataItems)
				if (dataItem is not null)
					result.Add(ConvertDataRow(dataItem));

			return result.CastOrToReadOnlyCollection();
		}

		throw new ArgumentException(
			string.Format(
				CultureInfo.CurrentCulture,
				"Member '{0}' on '{1}' must return data in one of the following formats:{2}{3}",
				MemberName,
				type.SafeName(),
				Environment.NewLine,
				supportedDataSignatures.Value
			)
		);
	}

	Func<object?>? GetFieldAccessor(Type? type)
	{
		FieldInfo? fieldInfo = null;
		foreach (var reflectionType in GetTypesForMemberResolution(type, includeInterfaces: false))
		{
			fieldInfo = reflectionType.GetRuntimeField(MemberName);
			if (fieldInfo is not null)
				break;
		}

		return
			fieldInfo is not null && fieldInfo.IsStatic
				? (() => fieldInfo.GetValue(null))
				: null;
	}

	Func<object?>? GetMethodAccessor(Type? type)
	{
		MethodInfo? methodInfo = null;
		var argumentTypes = Arguments is null ? [] : Arguments.Select(p => p?.GetType()).ToArray();
		foreach (var reflectionType in GetTypesForMemberResolution(type, includeInterfaces: true))
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

		var completedArguments = Arguments ?? [];
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
		foreach (var reflectionType in GetTypesForMemberResolution(type, includeInterfaces: true))
		{
			propInfo = reflectionType.GetRuntimeProperty(MemberName);
			if (propInfo is not null)
				break;
		}

		return
			propInfo is not null && propInfo.GetMethod is not null && propInfo.GetMethod.IsStatic
				? (() => propInfo.GetValue(null, null))
				: null;
	}

	static IEnumerable<Type> GetTypesForMemberResolution(
		Type? typeToInspect,
		bool includeInterfaces)
	{
		HashSet<Type> interfaces = [];

		for (var reflectionType = typeToInspect; reflectionType is not null; reflectionType = reflectionType.BaseType)
		{
			yield return reflectionType;

			if (includeInterfaces)
				foreach (var @interface in reflectionType.GetInterfaces())
					interfaces.Add(@interface);
		}

		foreach (var @interface in interfaces)
			yield return @interface;
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

	/// <inheritdoc/>
	public override bool SupportsDiscoveryEnumeration() =>
		!DisableDiscoveryEnumeration;
}
