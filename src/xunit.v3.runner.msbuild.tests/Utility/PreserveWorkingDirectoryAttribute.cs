using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Xunit.v3;

public class PreserveWorkingDirectoryAttribute : BeforeAfterTestAttribute
{
	string? workingDirectory;

	public override ValueTask Before(
		MethodInfo methodUnderTest,
		IXunitTest test)
	{
		workingDirectory = Directory.GetCurrentDirectory();
		return default;
	}

	public override ValueTask After(
		MethodInfo methodUnderTest,
		IXunitTest test)
	{
		if (workingDirectory is not null)
			Directory.SetCurrentDirectory(workingDirectory);

		return default;
	}
}
