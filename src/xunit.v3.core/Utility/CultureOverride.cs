namespace Xunit.v3;

sealed class CultureOverride : IDisposable
{
	readonly CultureInfo? lastCulture;
	readonly CultureInfo? lastUICulture;

	public CultureOverride(string? culture)
	{
		if (culture is null)
			return;

		lastCulture = CultureInfo.CurrentCulture;
		lastUICulture = CultureInfo.CurrentUICulture;

		var newCulture =
			culture.Length == 0
				? CultureInfo.InvariantCulture
				: new CultureInfo(culture, useUserOverride: false);

		CultureInfo.CurrentCulture = newCulture;
		CultureInfo.CurrentUICulture = newCulture;
	}

	public static async ValueTask Call(
		string? culture,
		object? parameter,
		Func<object?, ValueTask> callback)
	{
		using var @override = new CultureOverride(culture);
		await Guard.ArgumentNotNull(callback)(parameter);
	}

	public void Dispose()
	{
		if (lastCulture is not null)
			CultureInfo.CurrentCulture = lastCulture;
		if (lastUICulture is not null)
			CultureInfo.CurrentUICulture = lastUICulture;
	}
}
