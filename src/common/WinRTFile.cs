


using Windows.ApplicationModel;

namespace System.IO
{
    internal static class File
    {
        public static bool Exists(string path)
        {
            if (!string.IsNullOrWhiteSpace(path))
            {

                var folder = Package.Current.InstalledLocation;

                if (path.Contains(folder.Path))
                {
                    var fileName = Path.GetFileName(path);

                    var fileAsync = folder.GetFileAsync(fileName);
                    try
                    {
                        fileAsync.AsTask()
                                 .Wait();
                    }
                    catch (AggregateException)
                    {
                        return false;
                    }
                    if (fileAsync.GetResults() != null)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }    
}
