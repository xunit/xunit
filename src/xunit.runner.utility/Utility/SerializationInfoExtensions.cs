using System.Runtime.Serialization;

internal static class SerializationInfoExtensions
{
    public static T GetValue<T>(this SerializationInfo info, string name)
    {
        return (T)info.GetValue(name, typeof(T));
    }
}