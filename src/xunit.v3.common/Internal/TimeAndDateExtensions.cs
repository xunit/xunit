using System;
using System.Globalization;

namespace Xunit.Internal;

/// <summary>
/// INTERNAL CLASS. DO NOT USE.
/// </summary>
public static class TimeAndDateExtensions
{
	/// <summary/>
	public static string ToRtf(this DateTime dateTime) =>
		dateTime.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture);

	/// <summary/>
	public static string ToRtf(this DateTimeOffset dateTime) =>
		dateTime.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture);

	/// <summary/>
	public static string ToTimespanRtf(this decimal time) =>
		TimeSpan.FromSeconds((double)time).ToString(@"hh\:mm\:ss\.fffffff", CultureInfo.InvariantCulture);
}
