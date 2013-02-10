using System;

namespace Xunit.Abstractions
{
    public interface ITestFrameworkDiscoverer : IDisposable
    {
        /// <summary>
        /// Starts the process of finding all tests in an assembly.
        /// </summary>
        /// <param name="includeSourceInformation">Whether to include source file information, if possible.</param>
        /// <param name="messageSink">The message sink to report results back to.</param>
        void Find(bool includeSourceInformation, IMessageSink messageSink);

        /// <summary>
        /// Starts the process of finding all tests in a class.
        /// </summary>
        /// <param name="type">The class to find tests in.</param>
        /// <param name="includeSourceInformation">Whether to include source file information, if possible.</param>
        /// <param name="messageSink">The message sink to report results back to.</param>
        void Find(ITypeInfo type, bool includeSourceInformation, IMessageSink messageSink);
    }
}