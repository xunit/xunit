using System.IO;
using System.Reflection;

namespace Xunit.Installer
{
    public static class ResourceHelper
    {
        public static void WriteResourceToFile(string resourceName, string outputFilename)
        {
            byte[] buffer = new byte[65536];

            using (Stream inStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
            using (Stream outStream = File.Open(outputFilename, FileMode.Create))
            {
                while (true)
                {
                    int read = inStream.Read(buffer, 0, buffer.Length);

                    if (read <= 0)
                        break;

                    outStream.Write(buffer, 0, read);
                }
            }
        }
    }
}
