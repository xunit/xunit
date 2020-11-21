using System;

namespace Xunit.v3
{
	/// <summary>
	/// Interface implement by objects that want to support serialization in xUnit.net.
	/// </summary>
	[Obsolete("I don't think this should be necessary, but it's here for now...")]
	public interface _IXunitSerializable
	{
		/// <summary>
		/// Called when the object should populate itself with data from the serialization info.
		/// </summary>
		/// <param name="info">The info to get the data from</param>
		void Deserialize(_IXunitSerializationInfo info);

		/// <summary>
		/// Called when the object should store its data into the serialization info.
		/// </summary>
		/// <param name="info">The info to store the data in</param>
		void Serialize(_IXunitSerializationInfo info);
	}
}
