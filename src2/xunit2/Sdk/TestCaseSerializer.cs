using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// Serializes and de-serializes <see cref="ITestCase"/> instances using <see cref="BinaryFormatter"/>,
    /// <see cref="Convert.ToBase64String(byte[])"/>, and <see cref="Convert.FromBase64String"/>.
    /// </summary>
    public static class TestCaseSerializer
    {
        static BinaryFormatter binaryFormatter = new BinaryFormatter();

        /// <inheritdoc/>
        public static ITestCase Deserialize(string value)
        {
            using (var stream = new MemoryStream(Convert.FromBase64String(value)))
                return (ITestCase)binaryFormatter.Deserialize(stream);
        }

        /// <inheritdoc/>
        public static string Serialize(ITestCase testCase)
        {
            using (var stream = new MemoryStream())
            {
                binaryFormatter.Serialize(stream, testCase);
                return Convert.ToBase64String(stream.GetBuffer());
            }
        }
    }
}