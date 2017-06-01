
namespace System {
#if !NET35 && !NET452
    /// <summary>
    /// Attribute to mark a class as serializable
    /// </summary> 
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Delegate | AttributeTargets.Interface, AllowMultiple = true)]
    public sealed class SerializableAttribute : Attribute
    {
        /// <summary>
        /// Default constructor that creates a new instance of the <see cref="SerializableAttribute"/> type.
        /// </summary> 
        public SerializableAttribute() {
        }
    }
#endif
}