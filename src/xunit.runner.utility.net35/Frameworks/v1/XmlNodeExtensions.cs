﻿using System;
using System.Collections.Generic;
using System.Xml;
using Xunit;

internal static class XmlNodeExtensions
{
    public static Xunit1TestCase ToTestCase(this XmlNode xml, string assemblyFileName, string configFileName)
    {
        if (xml.Name != "method")
            return null;

        var displayName = xml.Attributes["name"].Value;
        var type = xml.Attributes["type"].Value;
        var method = xml.Attributes["method"].Value;
        string skipReason = null;
        var traits = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

        var skipReasonAttribute = xml.Attributes["skip"];
        if (skipReasonAttribute != null)
            skipReason = skipReasonAttribute.Value;

        foreach (XmlNode traitNode in xml.SelectNodes("traits/trait"))
            traits.Add(traitNode.Attributes["name"].Value, traitNode.Attributes["value"].Value);

        return new Xunit1TestCase(assemblyFileName, configFileName, type, method, displayName, traits, skipReason);
    }
}