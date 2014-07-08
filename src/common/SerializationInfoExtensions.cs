using System.Runtime.Serialization;
using Xunit;

#if WINDOWS_PHONE_APP
using Xunit.Serialization;
#endif

internal static class SerializationInfoExtensions
{
    public static T GetValue<T>(this SerializationInfo info, string name)
    {
        return (T)info.GetValue(name, typeof(T));
    }
}