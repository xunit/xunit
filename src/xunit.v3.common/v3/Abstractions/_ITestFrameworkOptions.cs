namespace Xunit.v3
{
	/// <summary>
	/// This interface should not be consumed directly; instead, you should
	/// consume <see cref="_ITestFrameworkDiscoveryOptions"/>
	/// or <see cref="_ITestFrameworkExecutionOptions"/>.
	/// </summary>
	public interface _ITestFrameworkOptions
	{
		/// <summary>
		/// Gets an option value.
		/// </summary>
		/// <typeparam name="TValue">The type of the value.</typeparam>
		/// <param name="name">The name of the value.</param>
		/// <returns>The value.</returns>
		TValue? GetValue<TValue>(string name);

		/// <summary>
		/// Sets an option value.
		/// </summary>
		/// <typeparam name="TValue">The type of the value.</typeparam>
		/// <param name="name">The name of the value.</param>
		/// <param name="value">The value to be set.</param>
		void SetValue<TValue>(string name, TValue value);

		/// <summary>
		/// Converts the test framework options into JSON. Used for serializing the options
		/// for sending across a process boundary.
		/// </summary>
		/// <returns>The serialized framework options.</returns>
		string ToJson();
	}
}
