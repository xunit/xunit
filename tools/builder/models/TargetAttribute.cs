using System;

namespace Xunit.BuildTools.Models;

[AttributeUsage(AttributeTargets.Class)]
public class TargetAttribute : Attribute
{
	public TargetAttribute(
		string targetName,
		params string[] dependentTargets)
	{
		TargetName = targetName;
		DependentTargets = dependentTargets ?? Array.Empty<string>();
	}

	public string TargetName { get; }

	public string[] DependentTargets { get; }
}
