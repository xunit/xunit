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

	public object Deserialize(Type type, string serializedValue)
	{
		// We need to look for the Parse method, and while it's definitely static, it might be public
		// or private, depending on whether the type wanted to hide the implementation of IParseable
		var parse =
			type.GetMethod("Parse", BindingFlags.Static | BindingFlags.Public, null, [typeof(string), typeof(IFormatProvider)], null) ??
			type.GetMethod($"System.IParsable<{type.FullName}>.Parse", BindingFlags.Static | BindingFlags.NonPublic, null, [typeof(string), typeof(IFormatProvider)], null) ??
			throw new InvalidOperationException($"Could not find Parse method for IParsable<{type.FullName}>");

		return
			parse.Invoke(null, [serializedValue, CultureInfo.InvariantCulture])
			?? throw new InvalidOperationException($"Call to IParsable<{type.FullName}>.Parse(\"{serializedValue}\") returned null");
	}

	public bool IsSerializable(Type type, object? value, [NotNullWhen(false)] out string? failureReason)
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
