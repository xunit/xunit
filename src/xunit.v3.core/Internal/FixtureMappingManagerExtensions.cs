using System;
using System.Collections.Generic;
using Xunit.v3;

namespace Xunit.Internal;

/// <summary>
/// INTERNAL CLASS. DO NOT USE.
/// </summary>
public static class FixtureMappingManagerExtensions
{
	/// <summary>
	/// INTERNAL METHOD. DO NOT USE.
	/// </summary>
	public static IReadOnlyDictionary<Type, object> GetFixtureCache(this FixtureMappingManager manager) =>
		Guard.ArgumentNotNull(manager).FixtureCache;
}
