using Windows.ApplicationModel;
using Windows.Storage;

namespace System.IO
{
    internal static class File
    {
        public static bool Exists(string path)
        {
            return GetStorageFile(path) != null;
        }

        public static Stream OpenRead(string path)
        {
            var storageFile = GetStorageFile(path);
            if (storageFile == null)
                throw new FileNotFoundException("Could not open file for read", path);

            return storageFile.OpenStreamForReadAsync().GetAwaiter().GetResult();
        }

        // Helpers

        static StorageFile GetStorageFile(string path)
        {
            if (String.IsNullOrWhiteSpace(path))
                return null;

            var folder = Package.Current.InstalledLocation;

            if (!path.Contains(folder.Path))
                return null;

            var fileName = Path.GetFileName(path);
            var fileAsync = folder.GetFileAsync(fileName);

            try
            {
                fileAsync.AsTask().Wait();
                return fileAsync.GetResults();
            }
            catch
            {
                return null;
            }
        }
    }
}
