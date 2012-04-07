using System;
using System.Collections;
using Microsoft.Build.Framework;

public class StubBuildEngine : IBuildEngine
{
    public bool BuildProjectFile(string projectFileName, string[] targetNames, IDictionary globalProperties, IDictionary targetOutputs)
    {
        throw new NotImplementedException();
    }

    public int ColumnNumberOfTaskNode
    {
        get { throw new NotImplementedException(); }
    }

    public bool ContinueOnError
    {
        get { return true; }
    }

    public int LineNumberOfTaskNode
    {
        get { throw new NotImplementedException(); }
    }

    public void LogCustomEvent(CustomBuildEventArgs e)
    {
    }

    public void LogErrorEvent(BuildErrorEventArgs e)
    {
    }

    public void LogMessageEvent(BuildMessageEventArgs e)
    {
    }

    public void LogWarningEvent(BuildWarningEventArgs e)
    {
    }

    public string ProjectFileOfTaskNode
    {
        get { throw new NotImplementedException(); }
    }
}