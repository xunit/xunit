using System.IO;
using System.Reflection;
using Xunit.v3;

public class PreserveWorkingDirectoryAttribute : BeforeAfterTestAttribute
{
	string? workingDirectory;

	public override void Before(
		MethodInfo methodUnderTest,
		IXunitTest test)
	{
		workingDirectory = Directory.GetCurrentDirectory();
	}

	public override void After(
		MethodInfo methodUnderTest,
		IXunitTest test)
	{
		if (workingDirectory is not null)
			Directory.SetCurrentDirectory(workingDirectory);
	}
}
