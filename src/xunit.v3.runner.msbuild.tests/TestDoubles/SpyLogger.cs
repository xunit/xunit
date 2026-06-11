using Microsoft.Build.Utilities;

public class SpyLogger(
	MockBuildEngine buildEngine,
	string taskName) :
		TaskLoggingHelper(buildEngine, taskName)
{
	public List<string> Messages => buildEngine.Messages;

	public static SpyLogger Create(
		string taskName = "MyTask",
		bool includeSourceInformation = false) =>
			new(new(includeSourceInformation), taskName);
}
