#if NETFRAMEWORK

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Xunit.Internal;
using Xunit.Runner.v1;

static class XmlNodeExtensions
{
	public static Xunit1TestCase? ToTestCase(
		this XmlNode xml,
		Xunit1TestClass testClass)
	{
		Guard.ArgumentNotNull(nameof(xml), xml);
		Guard.ArgumentNotNull(nameof(testClass), testClass);

		if (xml.Name != "method")
			return null;

		// The "name" attribute was introduced in 1.5, so it's not set for 1.1
		string? displayName = null;
		var displayNameAttribute = xml.Attributes["name"];
		if (displayNameAttribute != null)
			displayName = displayNameAttribute.Value;

		var method = xml.Attributes["method"].Value;

		string? skipReason = null;
		var skipReasonAttribute = xml.Attributes["skip"];
		if (skipReasonAttribute != null)
			skipReason = skipReasonAttribute.Value;

		var traits = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
		foreach (var traitNode in xml.SelectNodes("traits/trait").Cast<XmlNode>())
			traits.Add(traitNode.Attributes["name"].Value, traitNode.Attributes["value"].Value);

		return new Xunit1TestCase(testClass, method, displayName, traits, skipReason);
	}
}

#endif
