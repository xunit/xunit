using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;

namespace Xunit.Sdk;

internal sealed class FormattableAndParsableSerializer : IXunitSerializer
{
	static Type? typeParsable = Type.GetType("System.IParsable`1");

	public static bool IsSupported =>
		typeParsable is not null;

	public object Deserialize(
		Type type,
		string serializedValue)
	{
		// We need to look for the Parse/TryParse methods, and while they're definitely static, they might be public
		// or private, depending on whether the type wanted to hide the implementation of IParseable
		var byRefType = type.MakeByRefType();
		var tryParse =
			type.GetMethod("TryParse", BindingFlags.Static | BindingFlags.Public, null, [typeof(string), typeof(IFormatProvider), byRefType], null) ??
			type.GetMethod($"System.IParsable<{type.FullName?.Replace("+", ".")}>.TryParse", BindingFlags.Static | BindingFlags.NonPublic, null, [typeof(string), typeof(IFormatProvider), byRefType], null);

		if (tryParse is not null)
		{
			var arguments = new object?[] { serializedValue, CultureInfo.InvariantCulture, null };
			var success = tryParse.Invoke(null, arguments);
			if (success is not true)
				throw new InvalidOperationException($"Call to IParsable<{type.FullName}>.TryParse(\"{serializedValue}\") returned false");

			return
				arguments[2] ??
				throw new InvalidOperationException($"Call to IParsable<{type.FullName}>.TryParse(\"{serializedValue}\") returned a null value");
		}

		var parse =
			type.GetMethod("Parse", BindingFlags.Static | BindingFlags.Public, null, [typeof(string), typeof(IFormatProvider)], null) ??
			type.GetMethod($"System.IParsable<{type.FullName?.Replace("+", ".")}>.Parse", BindingFlags.Static | BindingFlags.NonPublic, null, [typeof(string), typeof(IFormatProvider)], null) ??
			throw new InvalidOperationException($"Could not find Parse or TryParse method for IParsable<{type.FullName}>");

		return
			parse.Invoke(null, [serializedValue, CultureInfo.InvariantCulture])
			?? throw new InvalidOperationException($"Call to IParsable<{type.FullName}>.Parse(\"{serializedValue}\") returned null");
	}

	public bool IsSerializable(
		Type type,
		object? value,
		[NotNullWhen(false)] out string? failureReason)
	{
		if (typeParsable is null)
		{
			failureReason = "Type IParsable<> is not supported on the current platform";
			return false;
		}

		var isParsable = false;

		// We wrap this in a try/catch because instantiating the generic may throw because
		// of constraint violations.
		try
		{
			isParsable = typeParsable.MakeGenericType(type).IsAssignableFrom(type);
		}
		catch { }

		if (!isParsable)
		{
			failureReason = $"Type '{type.FullName}' must implement both IFormattable and IParsable<> to be serialized";
			return false;
		}

		failureReason = null;
		return true;
	}

	public string Serialize(object value) =>
		((IFormattable)value).ToString(default, CultureInfo.InvariantCulture);
}
