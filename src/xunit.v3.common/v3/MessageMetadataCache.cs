using System.Collections.Generic;
using Xunit.Internal;

namespace Xunit.v3
{
	/// <summary/>
	public class MessageMetadataCache
	{
		readonly Dictionary<string, object> cache = new Dictionary<string, object>();

		/// <summary/>
		public void Set(_TestAssemblyStarting message) =>
			InternalSet(message.AssemblyUniqueID, message);

		/// <summary/>
		public void Set(_TestCollectionStarting message) =>
			InternalSet(message.TestCollectionUniqueID, message);

		/// <summary/>
		public _IAssemblyMetadata? TryGet(_TestAssemblyMessage message) =>
			InternalGetAndRemove<_IAssemblyMetadata>(message.AssemblyUniqueID, false);

		/// <summary/>
		public _ITestCollectionMetadata? TryGet(_TestCollectionMessage message) =>
			InternalGetAndRemove<_ITestCollectionMetadata>(message.TestCollectionUniqueID, false);

		/// <summary/>
		public _IAssemblyMetadata? TryRemove(_TestAssemblyMessage message) =>
			InternalGetAndRemove<_IAssemblyMetadata>(message.AssemblyUniqueID, true);

		/// <summary/>
		public _ITestCollectionMetadata? TryRemove(_TestCollectionMessage message) =>
			InternalGetAndRemove<_ITestCollectionMetadata>(message.TestCollectionUniqueID, true);

		// Helpers

		TMetadata? InternalGetAndRemove<TMetadata>(
			string uniqueID,
			bool remove)
				where TMetadata : class
		{
			lock (cache)
			{
				if (!cache.TryGetValue(uniqueID, out var metadata))
					return null;

				if (remove)
					cache.Remove(uniqueID);

				return (TMetadata)metadata;
			}
		}

		void InternalSet<TMetadata>(
			string uniqueID,
			TMetadata message)
		{
			Guard.ArgumentNotNull(nameof(uniqueID), uniqueID);
			Guard.ArgumentNotNull(nameof(message), message);

			lock (cache)
				cache.Add(uniqueID, message);
		}
	}
}
