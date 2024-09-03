using System;
using System.Collections.Generic;
using System.Globalization;
using Xunit.Internal;

namespace Xunit.Runner.MSBuild;

/// <summary/>
public class TraitParser(Action<string>? warningHandler = null)
{
	static readonly char[] TraitSeparator = [';'];
	static readonly char[] KeyValueSeparator = ['='];

	/// <summary/>
	public void Parse(
		string? traits,
		Dictionary<string, HashSet<string>> traitsDictionary)
	{
		if (!string.IsNullOrEmpty(traits))
		{
			foreach (var trait in traits.Split(TraitSeparator, StringSplitOptions.RemoveEmptyEntries))
			{
				var pieces = trait.Split(KeyValueSeparator, 2);

				if (pieces.Length != 2 || string.IsNullOrEmpty(pieces[0]) || string.IsNullOrEmpty(pieces[1]))
				{
					OnWarning(string.Format(CultureInfo.CurrentCulture, "Invalid trait '{0}'. The format should be 'name=value'. This trait will be ignored.", trait));
					continue;
				}

				traitsDictionary.Add(pieces[0].Trim(), pieces[1].Trim());
			}
		}
	}

	/// <summary/>
	protected virtual void OnWarning(string message)
	{
		Guard.ArgumentNotNullOrEmpty(message);

		warningHandler?.Invoke(message);
	}
}
