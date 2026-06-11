using System.Collections;
using Microsoft.Build.Framework;

public class MockBuildEngine(bool includeSourceInformation = false) :
	IBuildEngine
{
	public List<string> Messages = [];

	public bool ContinueOnError => true;
	public int LineNumberOfTaskNode => 0;
	public int ColumnNumberOfTaskNode => 0;
	public string ProjectFileOfTaskNode => "unknown";

	public bool BuildProjectFile(string projectFileName, string[] targetNames, IDictionary globalProperties, IDictionary targetOutputs) => throw new NotImplementedException();
	public void LogCustomEvent(CustomBuildEventArgs e) => throw new NotImplementedException();
	public void LogErrorEvent(BuildErrorEventArgs e)
	{
		if (includeSourceInformation)
			Messages.Add($"ERROR: [FILE {e.File}][LINE {e.LineNumber}] {e.Message}");
		else
			Messages.Add($"ERROR: {e.Message}");
	}
	public void LogMessageEvent(BuildMessageEventArgs e) => Messages.Add($"MESSAGE[{e.Importance}]: {e.Message}");
	public void LogWarningEvent(BuildWarningEventArgs e) => Messages.Add($"WARNING: {e.Message}");
}
