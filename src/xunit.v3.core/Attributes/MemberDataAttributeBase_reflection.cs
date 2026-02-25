using System.Collections;
using System.Reflection;
using Xunit.Sdk;

namespace Xunit.v3;

partial class MemberDataAttributeBase : ITypeAwareDataAttribute
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
			methodInfo = methodInfoArray.FirstOrDefault(m => m.GetParameters().Length == argumentTypes.Length);
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
