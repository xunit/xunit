using System;
using System.IO;

public class TempFile : IDisposable
{
    public TempFile()
        : this(Path.GetTempFileName()) { }

    public TempFile(string fileName)
    {
        FileName = Path.GetFullPath(Path.Combine(Path.GetTempPath(), fileName));
        File.WriteAllText(FileName, "");
    }

    public string FileName { get; private set; }

    public void Dispose()
    {
        File.Delete(FileName);
    }
}
