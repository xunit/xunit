﻿using System;
using System.Collections.Generic;
using Xunit.Internal;

namespace Xunit.Runner.MSBuild;

public class TraitParser
{
	static readonly char[] TraitSeparator = { ';' };
	static readonly char[] KeyValueSeparator = { '=' };

	readonly Action<string>? warningHandler;

	public TraitParser(Action<string>? warningHandler = null)
	{
		this.warningHandler = warningHandler;
	}

	public void Parse(
		string? traits,
		Dictionary<string, List<string>> traitsDictionary)
	{
		if (!string.IsNullOrEmpty(traits))
		{
			foreach (var trait in traits.Split(TraitSeparator, StringSplitOptions.RemoveEmptyEntries))
			{
				var pieces = trait.Split(KeyValueSeparator, 2);

				if (pieces.Length != 2 || string.IsNullOrEmpty(pieces[0]) || string.IsNullOrEmpty(pieces[1]))
				{
					OnWarning($"Invalid trait '{trait}'. The format should be 'name=value'. This trait will be ignored.");
					continue;
				}

				traitsDictionary.Add(pieces[0].Trim(), pieces[1].Trim());
			}
		}
	}

	protected virtual void OnWarning(string message)
	{
		Guard.ArgumentNotNullOrEmpty(message);

		warningHandler?.Invoke(message);
	}
}
