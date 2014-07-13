using Windows.ApplicationModel;

namespace System.IO
{
    internal static class File
    {
        public static bool Exists(string path)
        {
            if (String.IsNullOrWhiteSpace(path))
                return false;

            var folder = Package.Current.InstalledLocation;

            if (!path.Contains(folder.Path))
                return false;

            var fileName = Path.GetFileName(path);
            var fileAsync = folder.GetFileAsync(fileName);

            try
            {
                fileAsync.AsTask().Wait();
                return fileAsync.GetResults() != null;
            }
            catch { }

            return false;
        }
    }
}
