
namespace System {
#if !NET35 && !NET452
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Delegate | AttributeTargets.Interface, AllowMultiple = true)]
    public sealed class SerializableAttribute : Attribute
    {
        public SerializableAttribute() {
        }
    }
#endif
}