using Xunit.Abstractions;

#if XUNIT_CORE_DLL
namespace Xunit.Sdk
#else
namespace Xunit
#endif
{
    /// <summary>
    /// Default implementation of <see cref="IDiagnosticMessage"/>.
    /// </summary>
    public class DiagnosticMessage : LongLivedMarshalByRefObject, IDiagnosticMessage
    {
        /// <inheritdoc/>
        public string Message { get; set; }
    }
}