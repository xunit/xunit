using System.IO;
using System.Reflection;
using Xunit.Sdk;
using Xunit.v3;

public class PreserveWorkingDirectoryAttribute : BeforeAfterTestAttribute
{
	string? workingDirectory;

	public override void Before(
		MethodInfo methodUnderTest,
		_ITest test)
	{
		workingDirectory = Directory.GetCurrentDirectory();
	}

	public override void After(
		MethodInfo methodUnderTest,
		_ITest test)
	{
		if (workingDirectory is not null)
			Directory.SetCurrentDirectory(workingDirectory);
	}
}
