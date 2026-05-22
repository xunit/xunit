using Xunit.Sdk;

namespace Xunit;

/// <summary>
/// Used to declare a test collection container class. The container class gives
/// developers a place to attach interfaces like <see cref="IClassFixture{T}"/> and
/// <see cref="ICollectionFixture{T}"/> that will be applied to all tests classes
/// that are members of the test collection.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class CollectionDefinitionAttribute : Attribute
{
	/// <summary>
	/// Initializes a new instance of the <see cref="CollectionDefinitionAttribute" /> class.
	/// Use this constructor when collection references by test classes use the generic
	/// <see cref="CollectionAttribute{TCollectionDefinition}"/> attribute or refer to the
	/// fixture class using <see cref="CollectionAttribute(Type)"/>.
	/// </summary>
	public CollectionDefinitionAttribute()
	{ }

	/// <summary>
	/// Initializes a new instance of the <see cref="CollectionDefinitionAttribute" /> class.
	/// Use this constructor when collection references by test classes use
	/// <see cref="CollectionAttribute(string)"/>.
	/// </summary>
	/// <param name="name">The test collection name.</param>
	public CollectionDefinitionAttribute(string name) =>
		Name = Guard.ArgumentNotNull(name);

	/// <summary>
	/// Gets or sets a value indicating whether this collection should not run in parallel with other collections in the assembly.
	/// </summary>
	public bool DisableParallelization
	{
		get => OptionalParallelismOptions == ParallelismOptions.None;
		set
		{
			if (value)
			{
				OptionalParallelismOptions = ParallelismOptions.None;
			}
		}
	}

	/// <summary>
	/// Gets or sets the parallelism options to use for this test collection.
	/// </summary>
	/// <remarks>
	/// Defaults to <see cref="ParallelismOptionsAliases.Default"/> when unspecified.
	/// </remarks>
	public ParallelismOptions ParallelismOptions
	{
		get => OptionalParallelismOptions ?? ParallelismOptionsAliases.Default;
		set => OptionalParallelismOptions = value;
	}

	/// <summary>
	/// Gets the collection definition name, if one was provided.
	/// </summary>
	public string? Name { get; }

	/// <summary>
	/// Gets the parallelism options to use for this test collection, or null if none have been specified.
	/// </summary>
	/// <remarks>
	/// This property is required in addition to <see cref="ParallelismOptions"/> because the assembly default value
	/// (defined by <see cref="CollectionBehaviorAttribute"/>) should be used if unspecified, and because you cannot
	/// initialize nullable properties on attribute usages since arguments must be constant expressions.
	/// </remarks>
	public ParallelismOptions? OptionalParallelismOptions { get; private set; }
}
