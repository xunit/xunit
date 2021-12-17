namespace Xunit.v3
{
	/// <summary>
	/// The implementation of <see cref="_ITestFramework"/> that supports discovery and
	/// execution of unit tests linked against xunit.v3.core.dll.
	/// </summary>
	public class XunitTestFramework : TestFramework
	{
		readonly string? configFileName;
		readonly _IMessageSink? diagnosticMessageSink;
		readonly _IMessageSink? internalDiagnosticMessageSink;

		// Note: These two constructors cannot be collapsed, because ExtensibilityPointFactory looks for test frameworks with very
		// specific signatures (either empty, or one `_IMessageSink` for diagnostics, or two `_IMessageSink`s both)

		/// <summary>
		/// Initializes a new instance of the <see cref="XunitTestFramework"/> class.
		/// </summary>
		/// <param name="diagnosticMessageSink">The message sink which receives <see cref="_DiagnosticMessage"/> messages.</param>
		/// <param name="internalDiagnosticMessageSink">The optional message sink which receives internal <see cref="_DiagnosticMessage"/> messages.</param>
		public XunitTestFramework(
			_IMessageSink? diagnosticMessageSink,
			_IMessageSink? internalDiagnosticMessageSink)
				: this(diagnosticMessageSink, internalDiagnosticMessageSink, null)
		{ }

		/// <summary>
		/// Initializes a new instance of the <see cref="XunitTestFramework"/> class.
		/// </summary>
		/// <param name="diagnosticMessageSink">The message sink which receives <see cref="_DiagnosticMessage"/> messages.</param>
		/// <param name="internalDiagnosticMessageSink">The optional message sink which receives internal <see cref="_DiagnosticMessage"/> messages.</param>
		/// <param name="configFileName">The test configuration file.</param>
		public XunitTestFramework(
			_IMessageSink? diagnosticMessageSink,
			_IMessageSink? internalDiagnosticMessageSink,
			string? configFileName)
		{
			this.diagnosticMessageSink = diagnosticMessageSink;
			this.internalDiagnosticMessageSink = internalDiagnosticMessageSink;
			this.configFileName = configFileName;
		}

		/// <inheritdoc/>
		protected override _ITestFrameworkDiscoverer CreateDiscoverer(_IAssemblyInfo assembly) =>
			new XunitTestFrameworkDiscoverer(assembly, configFileName, diagnosticMessageSink, internalDiagnosticMessageSink);

		/// <inheritdoc/>
		protected override _ITestFrameworkExecutor CreateExecutor(_IReflectionAssemblyInfo assembly) =>
			new XunitTestFrameworkExecutor(assembly, configFileName, diagnosticMessageSink, internalDiagnosticMessageSink);
	}
}
