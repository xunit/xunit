using Xunit.Sdk;

namespace Xunit.Internal;

/// <summary>
/// INTERNAL CLASS. DO NOT USE.
/// </summary>
public abstract class CollectionAttributeBase : Attribute
{
	internal CollectionAttributeBase(string name) =>
		Name = name;

	internal CollectionAttributeBase(Type type)
	{
		Name = GetCollectionNameForType(type);
		Type = type;
	}

	/// <summary>
	/// Gets the name of the collection. If <see cref="CollectionAttribute(string)"/> was called,
	/// will return the provided name; if <see cref="CollectionAttribute(Type)"/> was called,
	/// will return a synthetic name for the type.
	/// </summary>
	public string Name { get; }

	/// <summary>
	/// Gets the type of the collection. If <see cref="CollectionAttribute(string)"/> was called,
	/// will return <see langword="null"/>; if <see cref="CollectionAttribute(Type)"/> was called,
	/// will return the passed type.
	/// </summary>
	public Type? Type { get; }

	/// <summary/>
	protected static string GetCollectionNameForType(Type type) =>
		string.Format(CultureInfo.InvariantCulture, "Test collection for {0} (id: {1})", Guard.ArgumentNotNull(type).SafeName(), UniqueIDGenerator.ForType(type));
}
