using System;

#if XUNIT_FRAMEWORK
namespace Xunit.v3
#else
namespace Xunit.Runner.v3
#endif
{
	/// <summary />
	public class _TestCollectionMessage : _TestAssemblyMessage
	{
		string? testCollectionDisplayName;
		string? testCollectionId;

		/// <summary />
		public string? TestCollectionClass { get; set; }

		/// <summary />
		public string TestCollectionDisplayName
		{
			get => testCollectionDisplayName ?? throw new InvalidOperationException($"Attempted to get {nameof(TestCollectionDisplayName)} on an uninitialized '{GetType().FullName}' object");
			set => testCollectionDisplayName = Guard.ArgumentNotNullOrEmpty(nameof(TestCollectionDisplayName), value);
		}

		/// <summary />
		public string TestCollectionId
		{
			get => testCollectionId ?? throw new InvalidOperationException($"Attempted to get {nameof(TestCollectionId)} on an uninitialized '{GetType().FullName}' object");
			set => testCollectionId = Guard.ArgumentNotNullOrEmpty(nameof(TestCollectionId), value);
		}
	}
}
