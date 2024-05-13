using System.IO;
using System.Text;
using Bullseye.Internal;

namespace Xunit.BuildTools.Models;

class NullLogger : Logger
{
	public NullLogger()
		: base(new NullTextWriter(), default, default, default, default, new Palette(default, default, default, default), default)
	{ }

	class NullTextWriter : TextWriter
	{
		public override Encoding Encoding => Encoding.UTF8;
	}
}
