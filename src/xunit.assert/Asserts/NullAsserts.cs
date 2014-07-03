using Xunit.Sdk;

namespace Xunit
{
    public partial class Assert
    {
        /// <summary>
        /// Verifies that an object reference is not null.
        /// </summary>
        /// <param name="object">The object to be validated</param>
        /// <exception cref="NotNullException">Thrown when the object is not null</exception>
        public static void NotNull(object @object)
        {
            if (@object == null)
                throw new NotNullException();
        }

        /// <summary>
        /// Verifies that an object reference is null.
        /// </summary>
        /// <param name="object">The object to be inspected</param>
        /// <exception cref="NullException">Thrown when the object reference is not null</exception>
        public static void Null(object @object)
        {
            if (@object != null)
                throw new NullException(@object);
        }
    }
}
