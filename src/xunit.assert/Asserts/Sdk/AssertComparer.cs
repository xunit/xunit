using System;
using System.Collections.Generic;
using System.Reflection;

namespace Xunit.Sdk
{
    /// <summary>
    /// Default implementation of <see cref="IComparer{T}"/> used by the xUnit.net range assertions.
    /// </summary>
    /// <typeparam name="T">The type that is being compared.</typeparam>
    class AssertComparer<T> : IComparer<T> where T : IComparable
    {
        static readonly TypeInfo NullableTypeInfo = typeof(Nullable<>).GetTypeInfo();

        /// <inheritdoc/>
        public int Compare(T x, T y)
        {
            var typeInfo = typeof(T).GetTypeInfo();

            // Null?
            if (!typeInfo.IsValueType || (typeInfo.IsGenericType && typeInfo.GetGenericTypeDefinition().GetTypeInfo().IsAssignableFrom(NullableTypeInfo)))
            {
                if (Equals(x, default(T)))
                {
                    if (Equals(y, default(T)))
                        return 0;
                    return -1;
                }

                if (Equals(y, default(T)))
                    return -1;
            }

            // Same type?
            if (x.GetType() != y.GetType())
                return -1;

            // Implements IComparable<T>?
            var comparable1 = x as IComparable<T>;
            if (comparable1 != null)
                return comparable1.CompareTo(y);

            // Implements IComparable
            return x.CompareTo(y);
        }
    }
}