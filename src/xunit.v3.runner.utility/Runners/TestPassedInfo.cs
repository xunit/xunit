using System;
using System.Collections.Generic;

namespace Xunit.Runners;

/// <summary>
/// Represents a test that passed.
/// </summary>
[Obsolete("Please use the TestPassedInfo class from the Xunit.SimpleRunner namespace. This class will be removed in the next major release.")]
public class TestPassedInfo(
	string typeName,
	string methodName,
	IReadOnlyDictionary<string, IReadOnlyCollection<string>>? traits,
	string testDisplayName,
	string testCollectionDisplayName,
	decimal executionTime,
	string? output) :
		TestExecutedInfo(typeName, methodName, traits, testDisplayName, testCollectionDisplayName, executionTime, output)
{ }
