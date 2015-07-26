using System;
using System.ComponentModel;

namespace Xunit
{
    /// <summary>
    /// Contains various static methods that are used to verify that conditions are met during the
    /// process of running tests.
    /// </summary>
    public partial class Assert
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Assert"/> class.
        /// </summary>
        protected Assert() { }

        /// <summary>Do not call this method.</summary>
        [Obsolete("This is an override of Object.Equals(). Call Assert.Equal() instead.", true)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new static bool Equals(object a, object b)
        {
            throw new InvalidOperationException("Assert.Equals should not be used");
        }

        /// <summary>Do not call this method.</summary>
        [Obsolete("This is an override of Object.ReferenceEquals(). Call Assert.Same() instead.", true)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new static bool ReferenceEquals(object a, object b)
        {
            throw new InvalidOperationException("Assert.ReferenceEquals should not be used");
        }
    }
}
