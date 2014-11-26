#if XUNIT_CORE_DLL
namespace Xunit.Sdk
#else
namespace Xunit
#endif
{
    /// <summary>
    /// Indicates the default display name format for test methods.
    /// </summary>
    public enum TestMethodDisplay
    {
        /// <summary>
        /// Use a fully qualified name (namespace + class + method)
        /// </summary>
        ClassAndMethod = 1,

        /// <summary>
        /// Use just the method name (without class)
        /// </summary>
        Method = 2
    }
}
