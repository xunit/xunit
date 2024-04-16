using System;

namespace Xunit.Internal;

/// <summary>
/// INTERNAL TYPE. DO NOT USE.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class JsonTypeIDAttribute : Attribute
{
	/// <summary/>
	public JsonTypeIDAttribute(string id) =>
		ID = id;

	/// <summary/>
	public string ID { get; }
}
