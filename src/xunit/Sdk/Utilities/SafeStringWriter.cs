using System.IO;

namespace Xunit.Sdk.Utilities
{
    internal class SafeStringWriter : StringWriter
    {
        private readonly object _lock = new object();
        public override void Write(char value)
        {
            lock(_lock)
            {
                base.Write(value);
            }
        }

        public override void Write(char[] buffer, int index, int count)
        {
            lock(_lock)
            {
                base.Write(buffer, index, count);
            }
        }

        public override void Write(string value)
        {
            lock(_lock)
            {
                base.Write(value);
            }
        }

        public override string ToString()
        {
            lock(_lock)
            {
                return base.ToString();
            }
        }
    }
}