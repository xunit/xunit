﻿using System;
using System.Collections;
using System.Collections.Generic;

namespace Xunit.Sdk
{
    /// <summary>
    /// A class that wraps <see cref="IEqualityComparer{T}"/> to create <see cref="IEqualityComparer"/>.
    /// </summary>
    /// <typeparam name="T">The type that is being compared.</typeparam>
    internal class AssertEqualityComparerAdapter<T> : IEqualityComparer
    {
        readonly IEqualityComparer<T> innerComparer;

        /// <summary>
        /// Initializes a new instance of the <see cref="AssertEqualityComparerAdapter{T}"/> class.
        /// </summary>
        /// <param name="innerComparer">The comparer that is being adapted.</param>
        public AssertEqualityComparerAdapter(IEqualityComparer<T> innerComparer)
        {
            this.innerComparer = innerComparer;
        }

        /// <inheritdoc/>
        public new bool Equals(object x, object y)
        {
            return innerComparer.Equals((T)x, (T)y);
        }

        /// <inheritdoc/>
        public int GetHashCode(object obj)
        {
            throw new NotImplementedException();
        }
    }
}