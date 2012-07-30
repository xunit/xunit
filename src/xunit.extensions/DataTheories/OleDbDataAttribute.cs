using System;
using System.Data;
using System.Data.OleDb;
using System.Diagnostics.CodeAnalysis;

namespace Xunit.Extensions
{
    /// <summary>
    /// Provides a data source for a data theory, with the data coming from an OLEDB connection.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Db", Justification = "That is the correct casing.")]
    [SuppressMessage("Microsoft.Performance", "CA1813:AvoidUnsealedAttributes", Justification = "This attribute is designed as an extensibility point.")]
    public class OleDbDataAttribute : DataAdapterDataAttribute
    {
        readonly string connectionString;
        readonly string selectStatement;

        /// <summary>
        /// Creates a new instance of <see cref="OleDbDataAttribute"/>.
        /// </summary>
        /// <param name="connectionString">The OLEDB connection string to the data</param>
        /// <param name="selectStatement">The SELECT statement used to return the data for the theory</param>
        public OleDbDataAttribute(string connectionString, string selectStatement)
        {
            this.connectionString = connectionString;
            this.selectStatement = selectStatement;
        }

        /// <summary>
        /// Gets the connection string.
        /// </summary>
        public string ConnectionString
        {
            get { return connectionString; }
        }

        /// <summary>
        /// Gets the select statement.
        /// </summary>
        public string SelectStatement
        {
            get { return selectStatement; }
        }

        /// <inheritdoc/>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Consumers of this property result already dispose of the result when they're finished with it.")]
        [SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "This query comes from the developer, not the end user.")]
        protected override IDataAdapter DataAdapter
        {
            get { return new OleDbDataAdapter(selectStatement, connectionString); }
        }
    }
}