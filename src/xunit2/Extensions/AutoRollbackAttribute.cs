using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Transactions;
using Xunit.Sdk;

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
        TransactionScope scope;

        /// <summary>
        /// Initializes a new instance of the <see cref="AutoRollbackAttribute"/> class.
        /// </summary>
        public AutoRollbackAttribute()
        {
            IsolationLevel = IsolationLevel.Unspecified;
            ScopeOption = TransactionScopeOption.Required;
        }

        /// <summary>
        /// Gets or sets the isolation level of the transaction.
        /// Default value is <see cref="IsolationLevel"/>.Unspecified.
        /// </summary>
        public IsolationLevel IsolationLevel { get; set; }

        /// <summary>
        /// Gets or sets the scope option for the transaction.
        /// Default value is <see cref="TransactionScopeOption"/>.Required.
        /// </summary>
        public TransactionScopeOption ScopeOption { get; set; }

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
            scope = new TransactionScope(ScopeOption, new TransactionOptions { IsolationLevel = IsolationLevel });
        }
    }
}