using System.Reflection;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
	/// <summary>
	/// The implementation of <see cref="ITestFramework"/> that supports discovery and
	/// execution of unit tests linked against xunit.v3.core.dll.
	/// </summary>
	public class XunitTestFramework : TestFramework
	{
		readonly string? configFileName;

		/// <summary>
		/// Initializes a new instance of the <see cref="XunitTestFramework"/> class.
		/// </summary>
		/// <param name="diagnosticMessageSink">The message sink used to send diagnostic messages</param>
		public XunitTestFramework(IMessageSink diagnosticMessageSink)
			: this(diagnosticMessageSink, null)
		{ }

		/// <summary>
		/// Initializes a new instance of the <see cref="XunitTestFramework"/> class.
		/// </summary>
		/// <param name="diagnosticMessageSink">The message sink used to send diagnostic messages</param>
		/// <param name="configFileName">The test configuration file.</param>
		public XunitTestFramework(
			IMessageSink diagnosticMessageSink,
			string? configFileName)
				: base(diagnosticMessageSink)
		{
			this.configFileName = configFileName;
		}

		/// <inheritdoc/>
		protected override ITestFrameworkDiscoverer CreateDiscoverer(IAssemblyInfo assemblyInfo) =>
			new XunitTestFrameworkDiscoverer(assemblyInfo, configFileName, SourceInformationProvider, DiagnosticMessageSink);

		/// <inheritdoc/>
		protected override ITestFrameworkExecutor CreateExecutor(AssemblyName assemblyName) =>
			new XunitTestFrameworkExecutor(assemblyName, configFileName, SourceInformationProvider, DiagnosticMessageSink);
	}
}
