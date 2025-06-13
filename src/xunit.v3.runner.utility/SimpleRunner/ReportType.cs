using System;
using System.Globalization;

namespace Xunit.SimpleRunner;

/// <summary>
/// Represents report formats that are available for <see cref="AssemblyRunnerOptions"/>.
/// </summary>
public enum ReportType
{
	/// <summary>
	/// Generates a report in Common Test Reporter Format (CTRF).
	/// </summary>
	/// <remarks>
	/// For more information, see <see href="https://ctrf.io/"/>.
	/// </remarks>
	CTRF = 1,

	/// <summary>
	/// Generates a report in HTML format.
	/// </summary>
	HTML = 2,

	/// <summary>
	/// Generates a report in JUnit format.
	/// </summary>
	/// <remarks>
	/// For more information, see <see href="https://github.com/testmoapp/junitxml"/>.
	/// </remarks>
	JUnit = 3,

	/// <summary>
	/// Generates a report in NUnit 2.5 format.
	/// </summary>
	NUnit_2_5 = 4,

	/// <summary>
	/// Generates a report in Visual Studio Test Results File (TRX) format.
	/// </summary>
	TRX = 5,

	/// <summary>
	/// Generates a report in the XML format for xUnit.net v1.
	/// </summary>
	/// <remarks>
	/// For more information, see <see href="https://xunit.net/docs/format-xml-v1"/>.
	/// </remarks>
	XMLv1 = 6,

	/// <summary>
	/// Generates a report in the XML format for xUnit.net v2 and v3.
	/// </summary>
	/// <remarks>
	/// For more information, see <see href="https://xunit.net/docs/format-xml-v2"/>.
	/// </remarks>
	XMLv2 = 7,
}

static class ReportTypeExtensions
{
	public static string ToKey(this ReportType reportType) =>
		reportType switch
		{
			ReportType.CTRF => "ctrf",
			ReportType.HTML => "html",
			ReportType.JUnit => "junit",
			ReportType.NUnit_2_5 => "nunit",
			ReportType.TRX => "trx",
			ReportType.XMLv1 => "xmlv1",
			ReportType.XMLv2 => "xml",
			_ => throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "Invalid report type '{0}'", reportType), nameof(reportType)),
		};
}
