using System.Collections.Generic;
using System.IO;
using System.Text;
using Xunit;

public class SpyRunnerLogger : IRunnerLogger
{
    static readonly string currentDirectory = Directory.GetCurrentDirectory();

    public List<string> Messages = new List<string>();

    public SpyRunnerLogger()
    {
        LockObject = new object();
    }

    public object LockObject { get; private set; }

    public void LogError(StackFrameInfo stackFrame, string message)
    {
        AddMessage("Err", stackFrame, message);
    }

    public void LogImportantMessage(StackFrameInfo stackFrame, string message)
    {
        AddMessage("Imp", stackFrame, message);
    }

    public void LogMessage(StackFrameInfo stackFrame, string message)
    {
        AddMessage("---", stackFrame, message);
    }

    public void LogWarning(StackFrameInfo stackFrame, string message)
    {
        AddMessage("Wrn", stackFrame, message);
    }

    void AddMessage(string category, StackFrameInfo stackFrame, string message)
    {
        var result = new StringBuilder();
        result.AppendFormat("[{0}", category);

        if (!stackFrame.IsEmpty)
        {
            var fileName = stackFrame.FileName;
            if (fileName.StartsWith(currentDirectory))
                fileName = fileName.Substring(currentDirectory.Length + 1);

            result.AppendFormat(" @ {0}:{1}", fileName, stackFrame.LineNumber);
        }

        result.AppendFormat("] => {0}", message);

        Messages.Add(result.ToString());
    }
}
