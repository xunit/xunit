﻿using System.Collections.Generic;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using NSubstitute;

public class SpyLogger : TaskLoggingHelper
{
	readonly bool includeSourceInformation;

	public List<string> Messages = new();

	SpyLogger(
		IBuildEngine buildEngine,
		string taskName,
		bool includeSourceInformation)
			: base(buildEngine, taskName)
	{
		this.includeSourceInformation = includeSourceInformation;

		buildEngine
			.WhenAny(e => e.LogMessageEvent(null))
			.Do<BuildMessageEventArgs>(Log);

		buildEngine
			.WhenAny(e => e.LogWarningEvent(null))
			.Do<BuildWarningEventArgs>(Log);

		buildEngine
			.WhenAny(e => e.LogErrorEvent(null))
			.Do<BuildErrorEventArgs>(Log);
	}

	public static SpyLogger Create(
		string taskName = "MyTask",
		bool includeSourceInformation = false) =>
			new(Substitute.For<IBuildEngine>(), taskName, includeSourceInformation);

	void Log(BuildMessageEventArgs eventArgs) =>
		Messages.Add($"MESSAGE[{eventArgs.Importance}]: {eventArgs.Message}");

	void Log(BuildWarningEventArgs eventArgs) =>
		Messages.Add($"WARNING: {eventArgs.Message}");

	void Log(BuildErrorEventArgs eventArgs)
	{
		if (includeSourceInformation)
			Messages.Add($"ERROR: [FILE {eventArgs.File}][LINE {eventArgs.LineNumber}] {eventArgs.Message}");
		else
			Messages.Add($"ERROR: {eventArgs.Message}");
	}
}
