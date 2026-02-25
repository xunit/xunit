using Xunit.Sdk;

namespace Xunit.Internal;

/// <summary>
/// INTERNAL CLASS. DO NOT USE.
/// </summary>
public static partial class ObjectExtensions
{
	/// <summary/>
	public static T ValidateNullablePropertyValue<T>(
		this object @object,
		T? value,
		string propertyName)
			where T : class
	{
		Guard.ArgumentNotNull(@object);

		return
			value is not null
				? value
				: throw new UnsetPropertyException(propertyName, @object.GetType());
	}

	/// <summary/>
	public static T ValidateNullablePropertyValue<T>(
		this object @object,
		T? value,
		string propertyName)
			where T : struct
	{
		Guard.ArgumentNotNull(@object);

		return
			value is not null
				? value.Value
				: throw new UnsetPropertyException(propertyName, @object.GetType());
	}
}
