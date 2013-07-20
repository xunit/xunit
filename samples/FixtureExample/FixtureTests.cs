using System;
using System.Configuration;
using System.Data.SqlClient;
using Xunit;

namespace FixtureExample
{
    public class DatabaseFixture : IDisposable
    {
        SqlConnection connection;
        int fooUserID;

        public DatabaseFixture()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["DatabaseFixture"].ConnectionString;
            connection = new SqlConnection(connectionString);
            connection.Open();

            string sql = @"INSERT INTO Users VALUES ('foo', 'bar'); SELECT SCOPE_IDENTITY();";

            using (SqlCommand cmd = new SqlCommand(sql, connection))
                fooUserID = Convert.ToInt32(cmd.ExecuteScalar());
        }

        public SqlConnection Connection
        {
            get { return connection; }
        }

        public int FooUserID
        {
            get { return fooUserID; }
        }

        public void Dispose()
        {
            string sql = @"DELETE FROM Users WHERE ID = @id;";

            using (SqlCommand cmd = new SqlCommand(sql, connection))
            {
                cmd.Parameters.AddWithValue("@id", fooUserID);
                cmd.ExecuteNonQuery();
            }

            connection.Close();
        }
    }

    public class FixtureTests : IClassFixture<DatabaseFixture>
    {
        DatabaseFixture database;

        public FixtureTests(DatabaseFixture data)
        {
            database = data;
        }

        [Fact]
        public void ConnectionIsEstablished()
        {
            Assert.NotNull(database.Connection);
        }

        [Fact]
        public void FooUserWasInserted()
        {
            string sql = "SELECT COUNT(*) FROM Users WHERE ID = @id;";

            using (SqlCommand cmd = new SqlCommand(sql, database.Connection))
            {
                cmd.Parameters.AddWithValue("@id", database.FooUserID);

                int rowCount = Convert.ToInt32(cmd.ExecuteScalar());

                Assert.Equal(1, rowCount);
            }
        }
    }
}
