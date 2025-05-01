using System;
using System.Collections;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Xunit.Runner.Common;

internal static class RunSettingsUtility
{
	static bool? collectSourceInformation;

	public static bool CollectSourceInformation
	{
		get
		{
			if (!collectSourceInformation.HasValue)
			{
				try
				{
					var runSettings = Environment.GetEnvironmentVariable("TESTINGPLATFORM_EXPERIMENTAL_VSTEST_RUNSETTINGS");
					if (runSettings is not null)
					{
						var doc = XDocument.Parse(runSettings);
						if (doc.Root?.XPathEvaluate("/RunSettings/RunConfiguration/CollectSourceInformation") is IEnumerable enumerable)
							if (enumerable.OfType<XElement>().FirstOrDefault() is XElement element)
								collectSourceInformation = bool.Parse(element.Value);
					}
				}
				catch { }

				collectSourceInformation ??= true;
			}

			return collectSourceInformation.Value;
		}
	}
}
