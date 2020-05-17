namespace Xunit.Abstractions
{
    /// <summary>
    /// This interface should not be consumed directly; instead, you should
    /// consume <see cref="ITestFrameworkDiscoveryOptions"/>
    /// or <see cref="ITestFrameworkExecutionOptions"/>.
    /// </summary>
    public interface ITestFrameworkOptions
    {
        /// <summary>
        /// Gets an option value.
        /// </summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="name">The name of the value.</param>
        /// <returns>The value.</returns>
        TValue GetValue<TValue>(string name);

        /// <summary>
        /// Sets an option value.
        /// </summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="name">The name of the value.</param>
        /// <param name="value">The value to be set.</param>
        void SetValue<TValue>(string name, TValue value);
    }
}
