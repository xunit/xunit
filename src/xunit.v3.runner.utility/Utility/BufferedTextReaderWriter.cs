using System.Collections.Concurrent;
using System.IO;
using System.Text;
using System.Threading;

namespace Xunit.v3;

/// <summary>
/// Represents a pair of <see cref="TextReader"/> and <see cref="TextWriter"/> that share a buffer,
/// such that data written to the writer can be read by the reader.
/// </summary>
/// <remarks>
/// This is used by <see cref="InProcessTestProcessLauncher"/> to simulate the StdIn and StdOut we'd
/// normally have from launching the process externally, and passed to the <c>EntryPoint</c> method
/// of <c>Xunit.Runner.InProc.SystemConsole.ConsoleRunner</c> via reflection (see
/// <see cref="InProcessTestProcess.Create"/>).
/// </remarks>
internal sealed class BufferedTextReaderWriter
{
	readonly ConcurrentQueue<char> buffer = new();
	volatile bool closed;

	public BufferedTextReaderWriter()
	{
		Reader = new BufferedReader(this);
		Writer = new BufferedWriter(this);
	}

	public TextReader Reader { get; }

	public TextWriter Writer { get; }

	sealed class BufferedReader(BufferedTextReaderWriter parent) : TextReader
	{
		public override int Peek()
		{
			while (true)
			{
				if (parent.closed && parent.buffer.IsEmpty)
					return -1;

				if (parent.buffer.TryPeek(out var result))
					return result;

				Thread.Sleep(10);
			}
		}

		public override int Read()
		{
			while (true)
			{
				if (parent.closed && parent.buffer.IsEmpty)
					return -1;

				if (parent.buffer.TryDequeue(out var result))
					return result;

				Thread.Sleep(10);
			}
		}
	}

	sealed class BufferedWriter(BufferedTextReaderWriter parent) : TextWriter
	{
		public override Encoding Encoding =>
			Encoding.UTF8;

		protected override void Dispose(bool disposing)
		{
			parent.closed = true;
			base.Dispose(disposing);
		}

		public override void Write(char value) =>
			parent.buffer.Enqueue(value);
	}
}
