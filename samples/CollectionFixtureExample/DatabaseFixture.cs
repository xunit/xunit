using System;
using System.Configuration;
using System.Data.SqlClient;

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
