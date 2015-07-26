using System;
using System.Reflection;
using Xunit.Sdk;

namespace Xunit
{
    public partial class Assert
    {
        /// <summary>
        /// Verifies that an object is of the given type or a derived type.
        /// </summary>
        /// <typeparam name="T">The type the object should be</typeparam>
        /// <param name="object">The object to be evaluated</param>
        /// <returns>The object, casted to type T when successful</returns>
        /// <exception cref="IsAssignableFromException">Thrown when the object is not the given type</exception>
        public static T IsAssignableFrom<T>(object @object)
        {
            IsAssignableFrom(typeof(T), @object);
            return (T)@object;
        }

        /// <summary>
        /// Verifies that an object is of the given type or a derived type.
        /// </summary>
        /// <param name="expectedType">The type the object should be</param>
        /// <param name="object">The object to be evaluated</param>
        /// <exception cref="IsAssignableFromException">Thrown when the object is not the given type</exception>
        public static void IsAssignableFrom(Type expectedType, object @object)
        {
            Assert.GuardArgumentNotNull("expectedType", expectedType);

            if (@object == null || !expectedType.GetTypeInfo().IsAssignableFrom(@object.GetType().GetTypeInfo()))
                throw new IsAssignableFromException(expectedType, @object);
        }

        /// <summary>
        /// Verifies that an object is not exactly the given type.
        /// </summary>
        /// <typeparam name="T">The type the object should not be</typeparam>
        /// <param name="object">The object to be evaluated</param>
        /// <exception cref="IsNotTypeException">Thrown when the object is the given type</exception>
        public static void IsNotType<T>(object @object)
        {
            IsNotType(typeof(T), @object);
        }

        /// <summary>
        /// Verifies that an object is not exactly the given type.
        /// </summary>
        /// <param name="expectedType">The type the object should not be</param>
        /// <param name="object">The object to be evaluated</param>
        /// <exception cref="IsNotTypeException">Thrown when the object is the given type</exception>
        public static void IsNotType(Type expectedType, object @object)
        {
            Assert.GuardArgumentNotNull("expectedType", expectedType);

            if (@object != null && expectedType.Equals(@object.GetType()))
                throw new IsNotTypeException(expectedType, @object);
        }

        /// <summary>
        /// Verifies that an object is exactly the given type (and not a derived type).
        /// </summary>
        /// <typeparam name="T">The type the object should be</typeparam>
        /// <param name="object">The object to be evaluated</param>
        /// <returns>The object, casted to type T when successful</returns>
        /// <exception cref="IsTypeException">Thrown when the object is not the given type</exception>
        public static T IsType<T>(object @object)
        {
            IsType(typeof(T), @object);
            return (T)@object;
        }

        /// <summary>
        /// Verifies that an object is exactly the given type (and not a derived type).
        /// </summary>
        /// <param name="expectedType">The type the object should be</param>
        /// <param name="object">The object to be evaluated</param>
        /// <exception cref="IsTypeException">Thrown when the object is not the given type</exception>
        public static void IsType(Type expectedType, object @object)
        {
            Assert.GuardArgumentNotNull("expectedType", expectedType);

            if (@object == null)
                throw new IsTypeException(expectedType.FullName, null);

            Type actualType = @object.GetType();
            if (expectedType != actualType)
            {
                string expectedTypeName = expectedType.FullName;
                string actualTypeName = actualType.FullName;

                if (expectedTypeName == actualTypeName)
                {
                    expectedTypeName += string.Format(" ({0})", expectedType.GetTypeInfo().Assembly.GetName().FullName);
                    actualTypeName += string.Format(" ({0})", actualType.GetTypeInfo().Assembly.GetName().FullName);
                }

                throw new IsTypeException(expectedTypeName, actualTypeName);
            }
        }
    }
}