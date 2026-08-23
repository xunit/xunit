#pragma warning disable CA1305 // Specify IFormatProvider

using System.Text;

namespace Xunit.Runner.Common;

/// <summary>
/// The message handler for <see cref="MarkdownResultWriter"/>.
/// </summary>
public class MarkdownResultWriterMessageHandler : MarkupResultWriterMessageHandlerBase
{
	bool disposed;
	readonly Lazy<Stream> stream;

	/// <summary>
	/// Initializes a new instance of the <see cref="MarkdownResultWriterMessageHandler"/> class.
	/// </summary>
	/// <param name="fileName">The output file name</param>
	public MarkdownResultWriterMessageHandler(string fileName) :
		this(new Lazy<Stream>(() => File.Create(fileName), isThreadSafe: false))
	{ }

	/// <summary>
	/// This constructor is for testing purposes only. Please call the public constructor.
	/// </summary>
	protected MarkdownResultWriterMessageHandler(Stream stream) :
		this(new Lazy<Stream>(() => stream))
	{ }

	MarkdownResultWriterMessageHandler(Lazy<Stream> stream) =>
		this.stream = stream;

	/// <summary>
	/// This is for testing purposes only. Do not use.
	/// </summary>
	protected Action<string>? OnDisposed { get; set; }

	internal override ResultMetadataBase CreateMetadata() =>
		new();

	/// <inheritdoc/>
	public override async ValueTask DisposeAsync()
	{
		GC.SuppressFinalize(this);

		if (!disposed)
			try
			{
				var buffer = new StringBuilder();
				var multiAssembly = Assemblies.Count > 1;
				var shortNames = Assemblies.Count == Assemblies.Select(static a => Path.GetFileName(a)).Distinct().Count();

				buffer.AppendLine("### Test Results");
				buffer.AppendLine();

				if (Totals.Total == 0)
					buffer.AppendLine("No tests were run.");
				else
				{
					string timeText;
					var time = TimeSpan.FromSeconds((double)Totals.Time);
					var timeFormat = $"mm\\:ss\\{CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator}fff";
					if (time.TotalHours >= 1)
						timeText = $"{(int)time.TotalHours}:{time.ToString(timeFormat)}";
					else if (time.TotalMilliseconds < 1000)
						timeText = $"{time.Milliseconds}ms";
					else
						timeText = time.ToString(timeFormat);

					buffer.AppendLine($"⌚ {timeText} total run time  ");
					buffer.AppendLine($"🧪 {Totals.Total} test{(Totals.Total != 1 ? "s" : "")}{(multiAssembly ? $" in {Assemblies.Count} assemblies" : "")}  ");

					if (Totals.Passed != 0)
						buffer.AppendLine($"✅ {Totals.Passed} passed  ");
					if (Totals.Failed != 0)
						buffer.AppendLine($"❌ {Totals.Failed} failed  ");
					if (Totals.Skipped != 0)
						buffer.AppendLine($"❔ {Totals.Skipped} skipped  ");
					if (Totals.NotRun != 0)
						buffer.AppendLine($"🚫 {Totals.NotRun} not run  ");
					if (Totals.Errors != 0)
						buffer.AppendLine($"💣 {Totals.Errors} error{(Totals.Errors > 1 ? "s" : "")}  ");
				}

				if (Totals.Failed != 0)
				{
					buffer.AppendLine();
					buffer.AppendLine("#### Failed ❌");
					buffer.AppendLine();

					foreach (var failed in Tests.Where(t => t.Status == TestResultStatus.Failed).OrderBy(t => t.DisplayName).ThenBy(t => t.Assembly))
					{
						buffer.AppendLine($"* {failed.DisplayName} {timing(failed)}{assemblyDisplay(failed)}  ");

						if (failed.Message is not null || failed.StackTrace is not null)
						{
							buffer.AppendLine("  _Exception:_");
							buffer.AppendLine("  ```");
							if (failed.Message is not null)
								buffer.AppendLine($"  {failed.Message.Indent("  ")}");
							if (failed.StackTrace is not null)
								buffer.AppendLine($"  {failed.StackTrace.Indent("  ")}");
							buffer.AppendLine("  ```");
						}
					}
				}

				if (Totals.Skipped != 0)
				{
					buffer.AppendLine();
					buffer.AppendLine("#### Skipped ❔");
					buffer.AppendLine();

					foreach (var skipped in Tests.Where(t => t.Status == TestResultStatus.Skipped).OrderBy(t => t.DisplayName).ThenBy(t => t.Assembly))
						buffer.AppendLine($"* {skipped.DisplayName}: \"{skipped.Message}\" {timing(skipped)}{assemblyDisplay(skipped)}");
				}

				if (Totals.NotRun != 0)
				{
					buffer.AppendLine();
					buffer.AppendLine("#### Not Run 🚫");
					buffer.AppendLine();

					foreach (var notRun in Tests.Where(t => t.Status == TestResultStatus.NotRun).OrderBy(t => t.DisplayName).ThenBy(t => t.Assembly))
						buffer.AppendLine($"* {notRun.DisplayName} {timing(notRun)}{assemblyDisplay(notRun)}");
				}

				if (Totals.Errors != 0)
				{
					buffer.AppendLine();
					buffer.AppendLine("#### Errors 💣");
					buffer.AppendLine();

					foreach (var error in Tests.Where(t => t.Status == TestResultStatus.Error).OrderBy(t => t.DisplayName).ThenBy(t => t.Assembly))
					{
						buffer.AppendLine($"* {error.DisplayName} {timing(error)}{assemblyDisplay(error)}  ");

						if (error.Message is not null || error.StackTrace is not null)
						{
							buffer.AppendLine("  _Exception:_");
							buffer.AppendLine("  ```");
							if (error.Message is not null)
								buffer.AppendLine($"  {error.Message.Indent("  ")}");
							if (error.StackTrace is not null)
								buffer.AppendLine($"  {error.StackTrace.Indent("  ")}");
							buffer.AppendLine("  ```");
						}
					}
				}

				var markdown = buffer.ToString();

				using (var streamWriter = new StreamWriter(stream.Value, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false)))
					await streamWriter.WriteAsync(markdown);

				stream.Value.SafeDispose();

				OnDisposed?.Invoke(markdown);

				string assemblyDisplay(TestResult testResult) =>
					multiAssembly && testResult.Assembly is not null
						? $" (`{(shortNames ? Path.GetFileName(testResult.Assembly) : testResult.Assembly)}`)"
						: string.Empty;

				string timing(TestResult testResult) =>
					$"⌚ {(testResult.Time == 0 ? "0" : testResult.Time)}s";
			}
			finally
			{
				disposed = true;
			}
	}
}
