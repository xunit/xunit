namespace Xunit.v3
{
	/// <summary>
	/// The implementation of <see cref="_ITestFramework"/> that supports discovery and
	/// execution of unit tests linked against xunit.v3.core.dll.
	/// </summary>
	public class XunitTestFramework : TestFramework
	{
		readonly string? configFileName;

		/// <summary>
		/// Initializes a new instance of the <see cref="XunitTestFramework"/> class.
		/// </summary>
		/// <param name="diagnosticMessageSink">The message sink which receives <see cref="_DiagnosticMessage"/> messages.</param>
		public XunitTestFramework(_IMessageSink diagnosticMessageSink)
			: this(diagnosticMessageSink, null)
		{ }

		/// <summary>
		/// Initializes a new instance of the <see cref="XunitTestFramework"/> class.
		/// </summary>
		/// <param name="diagnosticMessageSink">The message sink which receives <see cref="_DiagnosticMessage"/> messages.</param>
		/// <param name="configFileName">The test configuration file.</param>
		public XunitTestFramework(
			_IMessageSink diagnosticMessageSink,
			string? configFileName)
				: base(diagnosticMessageSink)
		{
			this.configFileName = configFileName;
		}

		/// <inheritdoc/>
		protected override _ITestFrameworkDiscoverer CreateDiscoverer(_IAssemblyInfo assembly) =>
			new XunitTestFrameworkDiscoverer(assembly, configFileName, SourceInformationProvider, DiagnosticMessageSink);

		/// <inheritdoc/>
		protected override _ITestFrameworkExecutor CreateExecutor(_IReflectionAssemblyInfo assembly) =>
			new XunitTestFrameworkExecutor(assembly, configFileName, SourceInformationProvider, DiagnosticMessageSink);
	}
}
