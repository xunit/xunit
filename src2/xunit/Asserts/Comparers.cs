using System;
using System.Collections;
using System.Collections.Generic;

namespace Xunit
{
    public partial class Assert
    {
        static IComparer<T> GetComparer<T>() where T : IComparable
        {
            return new AssertComparer<T>();
        }

        static IEqualityComparer<T> GetEqualityComparer<T>(bool skipTypeCheck = false, IEqualityComparer innerComparer = null)
        {
            return new AssertEqualityComparer<T>(skipTypeCheck, innerComparer);
        }

        class AssertComparer<T> : IComparer<T> where T : IComparable
        {
            public int Compare(T x, T y)
            {
                Type type = typeof(T);

                // Null?
                if (!type.IsValueType || (type.IsGenericType && type.GetGenericTypeDefinition().IsAssignableFrom(typeof(Nullable<>))))
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
                IComparable<T> comparable1 = x as IComparable<T>;
                if (comparable1 != null)
                    return comparable1.CompareTo(y);

                // Implements IComparable
                return x.CompareTo(y);
            }
        }

        class AssertEqualityComparer<T> : IEqualityComparer<T>
        {
            static IEqualityComparer defaultInnerComparer = new AssertEqualityComparerAdapter<object>(new AssertEqualityComparer<object>());
            IEqualityComparer innerComparer;
            bool skipTypeCheck;

            public AssertEqualityComparer(bool skipTypeCheck = false, IEqualityComparer innerComparer = null)
            {
                this.skipTypeCheck = skipTypeCheck;
                this.innerComparer = innerComparer ?? defaultInnerComparer;
            }

            public bool Equals(T x, T y)
            {
                Type type = typeof(T);

                // Null?
                if (!type.IsValueType || (type.IsGenericType && type.GetGenericTypeDefinition().IsAssignableFrom(typeof(Nullable<>))))
                {
                    if (Object.Equals(x, default(T)))
                        return Object.Equals(y, default(T));

                    if (Object.Equals(y, default(T)))
                        return false;
                }

                // Same type?
                if (!skipTypeCheck && x.GetType() != y.GetType())
                    return false;

                // Implements IEquatable<T>?
                IEquatable<T> equatable = x as IEquatable<T>;
                if (equatable != null)
                    return equatable.Equals(y);

                // Implements IComparable<T>?
                IComparable<T> comparable1 = x as IComparable<T>;
                if (comparable1 != null)
                    return comparable1.CompareTo(y) == 0;

                // Implements IComparable?
                IComparable comparable2 = x as IComparable;
                if (comparable2 != null)
                    return comparable2.CompareTo(y) == 0;

                // Enumerable?
                IEnumerable enumerableX = x as IEnumerable;
                IEnumerable enumerableY = y as IEnumerable;

                if (enumerableX != null && enumerableY != null)
                {
                    IEnumerator enumeratorX = enumerableX.GetEnumerator();
                    IEnumerator enumeratorY = enumerableY.GetEnumerator();

                    while (true)
                    {
                        bool hasNextX = enumeratorX.MoveNext();
                        bool hasNextY = enumeratorY.MoveNext();

                        if (!hasNextX || !hasNextY)
                            return (hasNextX == hasNextY);

                        if (!innerComparer.Equals(enumeratorX.Current, enumeratorY.Current))
                            return false;
                    }
                }

                // Last case, rely on Object.Equals
                return Object.Equals(x, y);
            }

            public int GetHashCode(T obj)
            {
                throw new NotImplementedException();
            }
        }

        class AssertEqualityComparerAdapter<T> : IEqualityComparer
        {
            readonly IEqualityComparer<T> innerComparer;

            public AssertEqualityComparerAdapter(IEqualityComparer<T> innerComparer)
            {
                this.innerComparer = innerComparer;
            }

            public new bool Equals(object x, object y)
            {
                return innerComparer.Equals((T)x, (T)y);
            }

            public int GetHashCode(object obj)
            {
                throw new NotImplementedException();
            }
        }
    }
}
