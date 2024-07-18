using System.Collections.Generic;
using Xunit.Internal;

namespace Xunit.Runners;

/// <summary>
/// Represents a test that was skipped.
/// </summary>
public class TestSkippedInfo : TestInfo
{
	/// <summary/>
	public TestSkippedInfo(
		string typeName,
		string methodName,
		Dictionary<string, HashSet<string>>? traits,
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
