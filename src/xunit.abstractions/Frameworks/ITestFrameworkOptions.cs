namespace Xunit.Abstractions
{
    /// <summary>
    /// Represents options given to an implementation of <see cref="ITestFrameworkDiscoverer"/>.Find
    /// or <see cref="ITestFrameworkExecutor"/>.Run.
    /// </summary>
    public interface ITestFrameworkOptions
    {
        /// <summary>
        /// Gets an option value.
        /// </summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="name">The name of the value.</param>
        /// <param name="defaultValue">The default value when none is present.</param>
        /// <returns>The value.</returns>
        TValue GetValue<TValue>(string name, TValue defaultValue);

        /// <summary>
        /// Sets an option value.
        /// </summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="name">The name of the value.</param>
        /// <param name="value">The value to be set.</param>
        void SetValue<TValue>(string name, TValue value);
    }
}
