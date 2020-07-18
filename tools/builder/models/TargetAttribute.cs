using System;
using System.Linq;

[AttributeUsage(AttributeTargets.Class)]
public class TargetAttribute : Attribute
{
	public TargetAttribute(BuildTarget targetName, params BuildTarget[] dependentTargets)
	{
		TargetName = targetName.ToString();
		DependentTargets = (dependentTargets ?? Enumerable.Empty<BuildTarget>()).Select(x => x.ToString()).ToArray();
	}

	public string TargetName { get; }

	public string[] DependentTargets { get; }
}
