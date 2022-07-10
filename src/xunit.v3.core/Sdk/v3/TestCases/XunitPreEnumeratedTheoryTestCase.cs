using System;
using System.Collections.Generic;

namespace Xunit.v3;

/// <summary>
/// Represents a test case which runs a single row of pre-enumerated data as a single test.
/// </summary>
public class XunitPreEnumeratedTheoryTestCase : XunitTestCase
{
	/// <summary>
	/// Called by the de-serializer; should only be called by deriving classes for de-serialization purposes
	/// </summary>
	[Obsolete("Called by the de-serializer; should only be called by deriving classes for de-serialization purposes")]
	public XunitPreEnumeratedTheoryTestCase()
	{ }

	/// <summary>
	/// Initializes a new instance of the <see cref="XunitPreEnumeratedTheoryTestCase"/> class.
	/// </summary>
	/// <param name="defaultMethodDisplay">Default method display to use (when not customized).</param>
	/// <param name="defaultMethodDisplayOptions">Default method display options to use (when not customized).</param>
	/// <param name="testMethod">The test method this test case belongs to.</param>
	/// <param name="testMethodArguments">The arguments for the test method.</param>
	/// <param name="skipReason">The optional reason for skipping the test; if not provided, will be read from the <see cref="FactAttribute"/>.</param>
	/// <param name="explicit">Indicates whether the test case was marked as explicit.</param>
	/// <param name="traits">The optional traits list; if not provided, will be read from trait attributes.</param>
	/// <param name="timeout">The optional timeout (in milliseconds); if not provided, will be read from the <see cref="FactAttribute"/>.</param>
	/// <param name="uniqueID">The optional unique ID for the test case; if not provided, will be calculated.</param>
	/// <param name="displayName">The optional display name for the test</param>
	public XunitPreEnumeratedTheoryTestCase(
		TestMethodDisplay defaultMethodDisplay,
		TestMethodDisplayOptions defaultMethodDisplayOptions,
		_ITestMethod testMethod,
		object?[] testMethodArguments,
		string? skipReason = null,
		bool? @explicit = null,
		Dictionary<string, List<string>>? traits = null,
		int? timeout = null,
		string? uniqueID = null,
		string? displayName = null)
			: base(defaultMethodDisplay, defaultMethodDisplayOptions, testMethod, testMethodArguments, skipReason, @explicit, traits, timeout, uniqueID, displayName)
	{ }
}
