#if XUNIT_CORE_DLL || WINDOWS_PHONE_APP
using Xunit.Serialization;
#endif

#if WINDOWS_PHONE_APP
using Xunit;
#else
using System.Runtime.Serialization;
#endif

internal static class SerializationInfoExtensions
{
    public static T GetValue<T>(this SerializationInfo info, string name)
    {
        return (T)info.GetValue(name, typeof(T));
    }

#if XUNIT_CORE_DLL || WINDOWS_PHONE_APP
    public static T GetValue<T>(this XunitSerializationInfo info, string name)
    {
        return (T)info.GetValue(name, typeof(T));
    }
#endif
}