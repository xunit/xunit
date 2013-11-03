using System;
using System.Collections.Generic;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using NSubstitute;

public class SpyLogger : TaskLoggingHelper
{
    public List<string> Messages = new List<string>();

    private SpyLogger(IBuildEngine buildEngine, string taskName)
        : base(buildEngine, taskName)
    {
        buildEngine.WhenAny(e => e.LogMessageEvent(null))
                   .Do<BuildMessageEventArgs>(Log);
        buildEngine.WhenAny(e => e.LogWarningEvent(null))
                   .Do<BuildWarningEventArgs>(Log);
        buildEngine.WhenAny(e => e.LogErrorEvent(null))
                   .Do<BuildErrorEventArgs>(Log);
    }

    public static SpyLogger Create(string taskName = "MyTask")
    {
        return new SpyLogger(Substitute.For<IBuildEngine>(), taskName);
    }

    private void Log(BuildMessageEventArgs eventArgs)
    {
        Messages.Add(String.Format("MESSAGE[{0}]: {1}", eventArgs.Importance, eventArgs.Message));
    }

    private void Log(BuildWarningEventArgs eventArgs)
    {
        Messages.Add(String.Format("WARNING: {0}", eventArgs.Message));
    }

    private void Log(BuildErrorEventArgs eventArgs)
    {
        Messages.Add(String.Format("ERROR: {0}", eventArgs.Message));
    }
}