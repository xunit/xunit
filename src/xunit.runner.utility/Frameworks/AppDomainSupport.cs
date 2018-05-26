namespace Xunit
{
    /// <summary>
    /// Indicates the level of app domain support that the runner is requesting.
    /// </summary>
    public enum AppDomainSupport
    {
        /// <summary>
        /// Requests that app domains be used, if available; if app domains cannot be used, then
        /// the tests will be discovered and run in the runner's app domain.
        /// </summary>
        IfAvailable = 1,

#if NETFRAMEWORK
        /// <summary>
        /// Requires that app domain support be used. Can only be requested by runners which link
        /// against xunit.runner.utility.desktop, and can only run test assemblies which link
        /// against xunit.execution.desktop.
        /// </summary>
        Required = 2,
#endif

        /// <summary>
        /// Requires that tests be run in the runner's app domain. This is supported by all runners
        /// and all execution libraries.
        /// </summary>
        Denied = 3
    }
}
