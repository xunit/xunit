using System.Text;
using Xunit.v3;

namespace Xunit.Runner.Common
{
	/// <summary>
	/// An implementation of <see cref="_IMessageSink" /> that supports <see cref="JsonReporter" />.
	/// </summary>
	public class JsonReporterMessageHandler : _IMessageSink
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
		public bool OnMessage(_MessageSinkMessage message)
		{
			if (message is _MessageSinkMessage v3Message)
				logger.LogImportantMessage(Encoding.UTF8.GetString(v3Message.ToJson()));

			return true;
		}
	}
}
