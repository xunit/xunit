namespace System.Runtime.Serialization
{
	/// <summary>
	/// Extensions for <see cref="SerializationInfo"/>.
	/// </summary>
	public static class XunitSerializationInfoExtensions
	{
		/// <summary>
		/// Retrieves a type-casted value from the <see cref="SerializationInfo" /> store.
		/// </summary>
		/// <param name="info"/>
		/// <param name="name">The name associated with the value to retrieve.</param>
		/// <returns>The object of the specified type <typeparamref name="T"/> associated with <paramref name="name" />.</returns>
		public static T? GetValue<T>(this SerializationInfo info, string name) =>
			(T?)info.GetValue(name, typeof(T));
	}
}
