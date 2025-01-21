using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

#if NET7_0_OR_GREATER
using System.Reflection;
#endif

namespace Xunit.Sdk;

internal sealed class FormattableAndParsableSerializer : IXunitSerializer
{
#if NET7_0_OR_GREATER
	public static bool IsSupported => true;
	static Type typeParsable = typeof(IParsable<>);
	static T Parse<T>(string s) where T : IParsable<T>
		=> T.Parse(s, CultureInfo.InvariantCulture);
	public object Deserialize(Type type, string serializedValue)
	{
		// We need to look for the Parse method, and while it's definitely static, it might be public
		// or private, depending on whether the type wanted to hide the implementation of IParseable
		var rawParse =
			typeof(FormattableAndParsableSerializer)
			.GetMethod(nameof(Parse), BindingFlags.Static | BindingFlags.NonPublic, null, [typeof(string)], null) ??
			throw new InvalidOperationException($"Could not find Parse method for IParsable<{type.FullName}>");

		MethodInfo parse;
		try
		{
			parse = rawParse.MakeGenericMethod(type);
		}
		catch (ArgumentException)
		{
			throw new InvalidOperationException($"Type '{type.FullName}' must implement IParsable<> to be deserialized");
		}

		return
			parse.Invoke(null, [serializedValue])
			?? throw new InvalidOperationException($"Call to IParsable<{type.FullName}>.Parse(\"{serializedValue}\") returned null");
	}
#else
	public static bool IsSupported => false;
	public object Deserialize(Type type, string serializedValue)
		=> throw new InvalidOperationException($"Could not find Parse method for IParsable<{type.FullName}>");
#endif

	public bool IsSerializable(Type type, object? value, [NotNullWhen(false)] out string? failureReason)
	{
#if NET7_0_OR_GREATER
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
#else
		failureReason = "Type IParsable<> is not supported on the current platform";
		return false;
#endif
	}

	public string Serialize(object value) =>
		((IFormattable)value).ToString(default, CultureInfo.InvariantCulture);
}
