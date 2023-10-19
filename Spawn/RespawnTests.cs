namespace Spawn;

public class RespawnTests(DatabaseFixture database, ITestOutputHelper output)
    : IClassFixture<DatabaseFixture>, IAsyncLifetime
{
    [Fact]
    public async Task Can_Insert_Person_Into_People()
    {
        await using var connection = await database.GetOpenConnectionAsync();
        await connection.InsertAsync<Person>(new()
        {
            FirstName = "Khalid",
            LastName = "Abuhakmeh",
            Age = 40,
            Email = "khalid@example.com"
        });

        var person = await connection.QueryFirstAsync<Person>("select top 1 * from people");
        var total = await connection.ExecuteScalarAsync("select count(ID) from People");

        output.WriteLine($"{person.FirstName} says hi!");

        Assert.NotNull(person);
        Assert.Equal(expected: 1, actual: total);
    }

    [Fact]
    public async Task People_Table_Is_Always_Empty()
    {
        await using var connection = await database.GetOpenConnectionAsync();
        var person = await connection.QueryFirstOrDefaultAsync<Person>("select top 1 * from people");
        Assert.Null(person);
    }

    public Task InitializeAsync()
        => Task.CompletedTask;

    public Task DisposeAsync()
        => database.ResetAsync();
}