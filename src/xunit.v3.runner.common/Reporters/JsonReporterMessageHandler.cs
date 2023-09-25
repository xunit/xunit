using System;
using System.Text;
using System.Threading.Tasks;
using Xunit.v3;

namespace Xunit.Runner.Common;

/// <summary>
/// An implementation of <see cref="IRunnerReporterMessageHandler" /> that supports <see cref="JsonReporter" />.
/// </summary>
public class JsonReporterMessageHandler : IRunnerReporterMessageHandler
{
	readonly IRunnerLogger logger;

	/// <summary>
	/// Initializes a new instance of the <see cref="JsonReporterMessageHandler" /> class.
	/// </summary>
	/// <param name="logger">The logger used to report messages</param>
	public JsonReporterMessageHandler(IRunnerLogger logger)
	{
		this.logger = logger;
	}

	/// <inheritdoc/>
	public ValueTask DisposeAsync()
	{
		GC.SuppressFinalize(this);

		return default;
	}

	/// <inheritdoc/>
	public bool OnMessage(_MessageSinkMessage message)
	{
		if (message is _MessageSinkMessage v3Message)
			logger.LogImportantMessage(Encoding.UTF8.GetString(v3Message.ToJson()));

		return true;
	}
}
