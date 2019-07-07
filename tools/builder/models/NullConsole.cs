using System.IO;
using System.Text;
using Bullseye.Internal;

class NullConsole : IConsole
{
    public TextWriter Out { get; } = new NullTextWriter();

    public void Clear() {}

    class NullTextWriter : TextWriter
    {
        public override Encoding Encoding => Encoding.UTF8;
    }
}