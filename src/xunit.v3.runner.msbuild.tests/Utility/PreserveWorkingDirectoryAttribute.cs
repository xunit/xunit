using System.IO;
using System.Reflection;
using Xunit.Abstractions;
using Xunit.Sdk;

public class PreserveWorkingDirectoryAttribute : BeforeAfterTestAttribute
{
	string? workingDirectory;

	public override void Before(MethodInfo methodUnderTest, ITest test)
	{
		workingDirectory = Directory.GetCurrentDirectory();
	}

	public override void After(MethodInfo methodUnderTest, ITest test)
	{
		Directory.SetCurrentDirectory(workingDirectory);
	}
}
