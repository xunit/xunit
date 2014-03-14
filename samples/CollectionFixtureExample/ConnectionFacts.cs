using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

[Collection("DatabaseCollection")]
public class ConnectionTests 
{
    DatabaseFixture database;

    public ConnectionTests(DatabaseFixture data)
    {
        database = data;
    }

    [Fact]
    public void ConnectionIsEstablished()
    {
        Assert.NotNull(database.Connection);
    }
}

