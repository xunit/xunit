#if NETFRAMEWORK

using System.IO;
using System.Reflection;
using Xunit.Sdk;

public class PreserveWorkingDirectoryAttribute : BeforeAfterTestAttribute
{
    string workingDirectory;

    public override void Before(MethodInfo methodUnderTest)
    {
        workingDirectory = Directory.GetCurrentDirectory();
    }

    public override void After(MethodInfo methodUnderTest)
    {
        Directory.SetCurrentDirectory(workingDirectory);
    }
}

#endif
