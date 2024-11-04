using System;
using System.Globalization;

namespace Xunit.Sdk;

internal static class ArraySerializer
{
	public static T[] Deserialize<T>(
		SerializationHelper serializationHelper,
		string serializedValue) =>
			(T[])Deserialize(serializationHelper, serializedValue);

	public static object Deserialize(
		SerializationHelper serializationHelper,
		string serializedValue)
	{
		var info = new XunitSerializationInfo(serializationHelper, serializedValue);
		var elementType = info.GetValue<Type>("t") ?? throw new ArgumentException("Missing array element type in serialization:" + Environment.NewLine + serializedValue, nameof(serializedValue));
		var rank = info.GetValue<int>("r");
		var totalLength = info.GetValue<int>("tl");

		var lengths = new int[rank];
		var lowerBounds = new int[rank];
		for (var i = 0; i < lengths.Length; i++)
		{
			lengths[i] = info.GetValue<int>(string.Format(CultureInfo.InvariantCulture, "l{0}", i));
			lowerBounds[i] = info.GetValue<int>(string.Format(CultureInfo.InvariantCulture, "lb{0}", i));
		}

		var array = Array.CreateInstance(elementType, lengths, lowerBounds);

		var indices = new int[rank];
		for (var i = 0; i < indices.Length; i++)
			indices[i] = lowerBounds[i];

		for (var i = 0; i < totalLength; i++)
		{
			var complete = false;

			for (var dim = rank - 1; dim >= 0; dim--)
			{
				if (indices[dim] >= lowerBounds[dim] + lengths[dim])
				{
					if (dim == 0)
					{
						complete = true;
						break;
					}
					for (var j = dim; j < rank; j++)
						indices[j] = lowerBounds[dim];
					indices[dim - 1]++;
				}
			}

			if (complete)
				break;

			var item = info.GetValue(string.Format(CultureInfo.InvariantCulture, "i{0}", i));
			array.SetValue(item, indices);
			indices[rank - 1]++;
		}

		return array;
	}

	public static string Serialize<T>(
		SerializationHelper serializationHelper,
		T[] array) =>
			Serialize(serializationHelper, typeof(T), array);

	public static string Serialize(
		SerializationHelper serializationHelper,
		Type elementType,
		object data)
	{
		if (data is not Array array)
			throw new ArgumentException("Data must be an array", nameof(data));

		var info = new XunitSerializationInfo(serializationHelper);
		info.AddValue("t", elementType);
		info.AddValue("r", array.Rank);
		info.AddValue("tl", array.Length);

		for (var dimension = 0; dimension < array.Rank; dimension++)
			info.AddValue(string.Format(CultureInfo.InvariantCulture, "l{0}", dimension), array.GetLength(dimension));
		for (var dimension = 0; dimension < array.Rank; dimension++)
			info.AddValue(string.Format(CultureInfo.InvariantCulture, "lb{0}", dimension), array.GetLowerBound(dimension));

		var i = 0;
		foreach (var obj in array)
			info.AddValue(string.Format(CultureInfo.InvariantCulture, "i{0}", i++), obj, obj?.GetType() ?? elementType);

		return info.ToSerializedString();
	}
}
