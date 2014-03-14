using System;
using System.Data.SqlClient;
using Xunit;

[Collection("DatabaseCollection")]
public class InsertTests
{
    DatabaseFixture database;

    public InsertTests(DatabaseFixture fixture)
    {
        database = fixture;
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
