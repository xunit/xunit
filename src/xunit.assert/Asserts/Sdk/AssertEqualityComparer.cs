using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

namespace Xunit.Sdk
{
    /// <summary>
    /// Default implementation of <see cref="IEqualityComparer{T}"/> used by the xUnit.net equality assertions.
    /// </summary>
    /// <typeparam name="T">The type that is being compared.</typeparam>
    class AssertEqualityComparer<T> : IEqualityComparer<T>
    {
        static readonly IEqualityComparer DefaultInnerComparer = new AssertEqualityComparerAdapter<object>(new AssertEqualityComparer<object>());
        static readonly TypeInfo NullableTypeInfo = typeof(Nullable<>).GetTypeInfo();

        readonly Func<IEqualityComparer> innerComparerFactory;
        readonly bool skipTypeCheck;

        /// <summary>
        /// Initializes a new instance of the <see cref="AssertEqualityComparer{T}" /> class.
        /// </summary>
        /// <param name="skipTypeCheck">Set to <c>true</c> to skip type equality checks.</param>
        /// <param name="innerComparer">The inner comparer to be used when the compared objects are enumerable.</param>
        public AssertEqualityComparer(bool skipTypeCheck = false, IEqualityComparer innerComparer = null)
        {
            this.skipTypeCheck = skipTypeCheck;

            // Use a thunk to delay evaluation of DefaultInnerComparer
            innerComparerFactory = () => innerComparer ?? DefaultInnerComparer;
        }

        /// <inheritdoc/>
        public bool Equals(T x, T y)
        {
            var typeInfo = typeof(T).GetTypeInfo();

            // Null?
            if (!typeInfo.IsValueType || (typeInfo.IsGenericType && typeInfo.GetGenericTypeDefinition().GetTypeInfo().IsAssignableFrom(NullableTypeInfo)))
            {
                if (object.Equals(x, default(T)))
                    return object.Equals(y, default(T));

                if (object.Equals(y, default(T)))
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

            // Dictionaries?
            var dictionariesEqual = CheckIfDictionariesAreEqual(x, y);
            if (dictionariesEqual.HasValue)
                return dictionariesEqual.GetValueOrDefault();

            // Sets?
            var setsEqual = CheckIfSetsAreEqual(x, y, typeInfo);
            if (setsEqual.HasValue)
                return setsEqual.GetValueOrDefault();

            // Enumerable?
            var enumerablesEqual = CheckIfEnumerablesAreEqual(x, y);
            if (enumerablesEqual.HasValue)
                return enumerablesEqual.GetValueOrDefault();

            // Last case, rely on object.Equals
            return object.Equals(x, y);
        }

        bool? CheckIfEnumerablesAreEqual(T x, T y)
        {
            var enumerableX = x as IEnumerable;
            var enumerableY = y as IEnumerable;

            if (enumerableX == null || enumerableY == null)
                return null;

            IEnumerator enumeratorX = null, enumeratorY = null;
            try
            {
                enumeratorX = enumerableX.GetEnumerator();
                enumeratorY = enumerableY.GetEnumerator();
                var equalityComparer = innerComparerFactory();

                while (true)
                {
                    var hasNextX = enumeratorX.MoveNext();
                    var hasNextY = enumeratorY.MoveNext();

                    if (!hasNextX || !hasNextY)
                        return hasNextX == hasNextY;

                    if (!equalityComparer.Equals(enumeratorX.Current, enumeratorY.Current))
                        return false;
                }
            }
            finally
            {
                var asDisposable = enumeratorX as IDisposable;
                if (asDisposable != null)
                    asDisposable.Dispose();
                asDisposable = enumeratorY as IDisposable;
                if (asDisposable != null)
                    asDisposable.Dispose();
            }
        }

        bool? CheckIfDictionariesAreEqual(T x, T y)
        {
            var dictionaryX = x as IDictionary;
            var dictionaryY = y as IDictionary;

            if (dictionaryX == null || dictionaryY == null)
                return null;

            if (dictionaryX.Count != dictionaryY.Count)
                return false;

            var equalityComparer = innerComparerFactory();
            var dictionaryYKeys = new HashSet<object>(dictionaryY.Keys.Cast<object>());

            foreach (var key in dictionaryX.Keys)
            {
                if (!dictionaryYKeys.Contains(key))
                    return false;

                var valueX = dictionaryX[key];
                var valueY = dictionaryY[key];

                if (!equalityComparer.Equals(valueX, valueY))
                    return false;

                dictionaryYKeys.Remove(key);
            }

            return dictionaryYKeys.Count == 0;
        }

        bool? CheckIfSetsAreEqual(T x, T y, TypeInfo typeInfo)
        {
            if (!IsSet(typeInfo))
                return null;

            var enumX = x as IEnumerable;
            var enumY = y as IEnumerable;
            if (enumX == null || enumY == null)
                return null;

            var elementType = typeof(T).GenericTypeArguments[0];
            MethodInfo method = GetType().GetTypeInfo().GetDeclaredMethod("CompareTypedSets");
            method = method.MakeGenericMethod(new Type[] { elementType });
            return (bool)method.Invoke(this, new object[] { enumX, enumY });
        }

        bool CompareTypedSets<R>(IEnumerable enumX, IEnumerable enumY)
        {
            var setX = new HashSet<R>(enumX.Cast<R>());
            var setY = new HashSet<R>(enumY.Cast<R>());
            return setX.SetEquals(setY);
        }

        bool IsSet(TypeInfo typeInfo)
        {
            return typeInfo.ImplementedInterfaces
                .Select(i => i.GetTypeInfo())
                .Where(ti => ti.IsGenericType)
                .Select(ti => ti.GetGenericTypeDefinition())
                .Contains(typeof(ISet<>).GetGenericTypeDefinition());
        }

        /// <inheritdoc/>
        [SuppressMessage("Code Notifications", "RECS0083:Shows NotImplementedException throws in the quick task bar", Justification = "This class is not intended to be used in a hased container")]
        public int GetHashCode(T obj)
        {
            throw new NotImplementedException();
        }
    }
}