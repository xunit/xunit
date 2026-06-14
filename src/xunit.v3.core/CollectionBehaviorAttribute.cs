using System.ComponentModel;
using Xunit.Sdk;
using Xunit.v3;

namespace Xunit;

/// <summary>
/// Used to declare the default test collection behavior for the assembly.
/// </summary>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
public sealed partial class CollectionBehaviorAttribute : Attribute
{
	/// <summary>
	/// Initializes a new instance of the <see cref="CollectionBehaviorAttribute" /> class.
	/// Uses the default collection behavior (<see cref="CollectionBehavior.CollectionPerClass"/>).
	/// </summary>
	public CollectionBehaviorAttribute()
	{ }

#pragma warning disable CA1019  // We don't want a property accessor for CollectionBehavior because it's just a type selector

	/// <summary>
	/// Initializes a new instance of the <see cref="CollectionBehaviorAttribute" /> class
	/// with the given built-in collection behavior.
	/// </summary>
	/// <param name="collectionBehavior">The collection behavior for the assembly.</param>
	public CollectionBehaviorAttribute(CollectionBehavior collectionBehavior) =>
		// This is an attribute constructor; throwing here would be wrong, so we just always fall back to the default
		CollectionFactoryType = collectionBehavior switch
		{
			CollectionBehavior.CollectionPerClass => typeof(CollectionPerClassTestCollectionFactory),
			CollectionBehavior.CollectionPerAssembly => typeof(CollectionPerAssemblyTestCollectionFactory),
			_ => null,
		};

#pragma warning restore CA1019

	public partial CollectionBehaviorAttribute(Type collectionFactoryType) =>
		CollectionFactoryType = collectionFactoryType;

	/// <summary>
	/// Please set <see cref="ParallelizationAttribute.Mode"/> instead.
	/// This property will be removed in the next major version.
	/// </summary>
	[Obsolete("Please set ParallelizationAttribute.Mode instead. This property will be removed in the next major version.", error: true)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public bool DisableTestParallelization { get; set; }

	/// <summary>
	/// Please set <see cref="ParallelizationAttribute.MaxThreads"/> instead.
	/// This property will be removed in the next major version.
	/// </summary>
	[Obsolete("Please set ParallelizationAttribute.MaxThreads instead. This property will be removed in the next major version.", error: true)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public int MaxParallelThreads { get; set; }

	/// <summary>
	/// Please set <see cref="ParallelizationAttribute.Algorithm"/> instead.
	/// This property will be removed in the next major version.
	/// </summary>
	[Obsolete("Please set ParallelizationAttribute.Algorithm instead. This property will be removed in the next major version.", error: true)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public ParallelAlgorithm ParallelAlgorithm { get; set; }
}

/// <summary>
/// Used to declare the default test collection behavior for the assembly.
/// </summary>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
public sealed partial class CollectionBehaviorAttribute<TCollectionFactory> : Attribute
{
	/// <summary>
	/// Please set <see cref="ParallelizationAttribute.Mode"/> instead.
	/// This property will be removed in the next major version.
	/// </summary>
	[Obsolete("Please set ParallelizationAttribute.Mode instead. This property will be removed in the next major version.", error: true)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public bool DisableTestParallelization { get; set; }

	/// <summary>
	/// Please set <see cref="ParallelizationAttribute.MaxThreads"/> instead.
	/// This property will be removed in the next major version.
	/// </summary>
	[Obsolete("Please set ParallelizationAttribute.MaxThreads instead. This property will be removed in the next major version.", error: true)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public int MaxParallelThreads { get; set; }

	/// <summary>
	/// Please set <see cref="ParallelizationAttribute.Algorithm"/> instead.
	/// This property will be removed in the next major version.
	/// </summary>
	[Obsolete("Please set ParallelizationAttribute.Algorithm instead. This property will be removed in the next major version.", error: true)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public ParallelAlgorithm ParallelAlgorithm { get; set; }
}
