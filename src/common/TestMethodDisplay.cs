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
        NamespaceAndClassAndMethod = 1,

        /// <summary>
        /// Use a class qualified name (class + method, without namespace)
        /// </summary>
        ClassAndMethod = 2,

        /// <summary>
        /// Use just the method name (without namespace or class)
        /// </summary>
        Method = 3
    }
}