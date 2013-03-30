using System;
using System.Collections.Generic;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Moq;

public class SpyLogger : TaskLoggingHelper
{
    public List<string> Messages = new List<string>();

    private SpyLogger(Mock<IBuildEngine> buildEngine, string taskName)
        : base(buildEngine.Object, taskName)
    {
        buildEngine.Setup(e => e.LogMessageEvent(It.IsAny<BuildMessageEventArgs>()))
                   .Callback<BuildMessageEventArgs>(Log);
        buildEngine.Setup(e => e.LogWarningEvent(It.IsAny<BuildWarningEventArgs>()))
                   .Callback<BuildWarningEventArgs>(Log);
        buildEngine.Setup(e => e.LogErrorEvent(It.IsAny<BuildErrorEventArgs>()))
                   .Callback<BuildErrorEventArgs>(Log);
    }

    public static SpyLogger Create(string taskName = "MyTask")
    {
        return new SpyLogger(new Mock<IBuildEngine>(), taskName);
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