using System;
using System.Collections.Generic;
using Xunit.Internal;

namespace Xunit.Runners;

/// <summary>
/// Represents a test that was skipped.
/// </summary>
[Obsolete("Please use the TestSkippedInfo class from the Xunit.SimpleRunner namespace. This class will be removed in the next major release.")]
public class TestSkippedInfo : TestInfo
{
	/// <summary/>
	public TestSkippedInfo(
		string typeName,
		string methodName,
		IReadOnlyDictionary<string, IReadOnlyCollection<string>>? traits,
		string testDisplayName,
		string testCollectionDisplayName,
		string skipReason)
			: base(typeName, methodName, traits, testDisplayName, testCollectionDisplayName)
	{
		Guard.ArgumentNotNull(skipReason);

		SkipReason = skipReason;
	}

	/// <summary>
	/// Gets the reason that was given for skipping the test.
	/// </summary>
	public string SkipReason { get; }
}
