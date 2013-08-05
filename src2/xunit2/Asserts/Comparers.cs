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
            static readonly IEqualityComparer DefaultInnerComparer = new AssertEqualityComparerAdapter<object>(new AssertEqualityComparer<object>());

            readonly Func<IEqualityComparer> innerComparerFactory;
            readonly bool skipTypeCheck;

            public AssertEqualityComparer(bool skipTypeCheck = false, IEqualityComparer innerComparer = null)
            {
                this.skipTypeCheck = skipTypeCheck;

                // Use a thunk to delay evaluation of DefaultInnerComparer
                this.innerComparerFactory = () => innerComparer ?? DefaultInnerComparer;
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
                var equatable = x as IEquatable<T>;
                if (equatable != null)
                    return equatable.Equals(y);

                // Implements IComparable<T>?
                var comparableGeneric = x as IComparable<T>;
                if (comparableGeneric != null)
                    return comparableGeneric.CompareTo(y) == 0;

                // Implements IComparable?
                var comparable = x as IComparable;
                if (comparable != null)
                    return comparable.CompareTo(y) == 0;

                // Enumerable?
                var enumerableX = x as IEnumerable;
                var enumerableY = y as IEnumerable;

                if (enumerableX != null && enumerableY != null)
                {
                    var enumeratorX = enumerableX.GetEnumerator();
                    var enumeratorY = enumerableY.GetEnumerator();
                    var equalityComparer = innerComparerFactory();

                    while (true)
                    {
                        bool hasNextX = enumeratorX.MoveNext();
                        bool hasNextY = enumeratorY.MoveNext();

                        if (!hasNextX || !hasNextY)
                            return (hasNextX == hasNextY);

                        if (!equalityComparer.Equals(enumeratorX.Current, enumeratorY.Current))
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