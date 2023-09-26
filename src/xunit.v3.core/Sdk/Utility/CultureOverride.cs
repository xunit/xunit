using System;
using System.Globalization;

namespace Xunit.Sdk;

class CultureOverride : IDisposable
{
	readonly CultureInfo lastCulture;
	readonly CultureInfo lastUICulture;

	public CultureOverride(string? culture)
	{
		lastCulture = CultureInfo.CurrentCulture;
		lastUICulture = CultureInfo.CurrentUICulture;

		if (culture is not null)
		{
			var newCulture = new CultureInfo(culture, useUserOverride: false);
			CultureInfo.CurrentCulture = newCulture;
			CultureInfo.CurrentUICulture = newCulture;
		}
	}

	public void Dispose()
	{
		CultureInfo.CurrentCulture = lastCulture;
		CultureInfo.CurrentUICulture = lastUICulture;
	}
}
