namespace Xunit
{
    /// <summary>
    /// Represents discovery options for xUnit.net v2 tests.
    /// </summary>
    public class XunitDiscoveryOptions : TestFrameworkOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="XunitDiscoveryOptions"/> class.
        /// </summary>
        /// <param name="configuration">The optional configuration to copy values from.</param>
        public XunitDiscoveryOptions(TestAssemblyConfiguration configuration = null)
        {
            if (configuration != null)
            {
                DiagnosticMessages = configuration.DiagnosticMessages;
                MethodDisplay = configuration.MethodDisplay;
            }
        }

        /// <summary>
        /// Gets or sets a flag that determines whether diagnostic messages will be emitted.
        /// </summary>
        public bool DiagnosticMessages
        {
            get { return GetValue<bool>(TestOptionsNames.Discovery.DiagnosticMessages, false); }
            set { SetValue(TestOptionsNames.Discovery.DiagnosticMessages, value); }
        }

        /// <summary>
        /// Gets or sets the default display name for test methods. Defaults
        /// to <see cref="TestMethodDisplay.NamespaceAndClassAndMethod"/>.
        /// </summary>
        public TestMethodDisplay MethodDisplay
        {
            get { return GetValue<TestMethodDisplay>(TestOptionsNames.Discovery.MethodDisplay, TestMethodDisplay.NamespaceAndClassAndMethod); }
            set { SetValue(TestOptionsNames.Discovery.MethodDisplay, value); }
        }
    }
}
