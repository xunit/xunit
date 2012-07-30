using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Transactions;

namespace Xunit.Extensions
{
    /// <summary>
    /// Apply this attribute to your test method to automatically create a
    /// <see cref="TransactionScope"/> that is rolled back when the test is
    /// finished.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "This class has its own disposability pattern.")]
    [SuppressMessage("Microsoft.Performance", "CA1813:AvoidUnsealedAttributes", Justification = "This attribute is designed as an extensibility point.")]
    public class AutoRollbackAttribute : BeforeAfterTestAttribute
    {
        IsolationLevel isolationLevel = IsolationLevel.Unspecified;
        TransactionScope scope;
        TransactionScopeOption scopeOption = TransactionScopeOption.Required;
        long timeoutInMS = -1;

        /// <summary>
        /// Gets or sets the isolation level of the transaction.
        /// Default value is <see cref="IsolationLevel"/>.Unspecified.
        /// </summary>
        public IsolationLevel IsolationLevel
        {
            get { return isolationLevel; }
            set { isolationLevel = value; }
        }

        /// <summary>
        /// Gets or sets the scope option for the transaction.
        /// Default value is <see cref="TransactionScopeOption"/>.Required.
        /// </summary>
        public TransactionScopeOption ScopeOption
        {
            get { return scopeOption; }
            set { scopeOption = value; }
        }

        /// <summary>
        /// Gets or sets the timeout of the transaction, in milliseconds.
        /// By default, the transaction will not timeout.
        /// </summary>
        public long TimeoutInMS
        {
            get { return timeoutInMS; }
            set { timeoutInMS = value; }
        }

        /// <summary>
        /// Rolls back the transaction.
        /// </summary>
        public override void After(MethodInfo methodUnderTest)
        {
            scope.Dispose();
        }

        /// <summary>
        /// Creates the transaction.
        /// </summary>
        public override void Before(MethodInfo methodUnderTest)
        {
            TransactionOptions options = new TransactionOptions();
            options.IsolationLevel = isolationLevel;
            if (timeoutInMS > 0)
                options.Timeout = new TimeSpan(timeoutInMS * 10);
            scope = new TransactionScope(scopeOption, options);
        }
    }
}