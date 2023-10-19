namespace Spawn;

// ReSharper disable once ClassNeverInstantiated.Global
public class DatabaseFixture : IAsyncLifetime
{
    private const string DatabaseName = "test";
    
    private readonly Dictionary<string, string> connectionStrings = new()
    {
        { DatabaseName, $"Data Source=localhost,11433;Database={DatabaseName};User Id=sa;Password=Pass123!;Encrypt=FALSE;" },
        { "master", "Data Source=localhost,11433;Database=master;User Id=sa;Password=Pass123!;Encrypt=FALSE;" }
    };

    public async Task<SqlConnection> GetOpenConnectionAsync(string databaseName = DatabaseName)
    {
        var sqlConnection = new SqlConnection(connectionStrings[databaseName]);
        await sqlConnection.OpenAsync();
        return sqlConnection;
    }

    private Respawner respawn = null!;

    public async Task InitializeAsync()
    {
        await MigrateAsync();

        respawn = await Respawner.CreateAsync(connectionStrings[DatabaseName],
            new RespawnerOptions
            {
                DbAdapter = DbAdapter.SqlServer
            });
    }

    private async Task MigrateAsync()
    {
        // only doing this for the sample,
        // you'll likely already have the database created somewhere
        {
            try
            {
                await using var connection = await GetOpenConnectionAsync("master");
                await connection.ExecuteAsync(
                    // lang=SQL
                    $"""
                     IF NOT EXISTS (SELECT [name] FROM sys.databases WHERE [name] = N'{DatabaseName}')
                        CREATE DATABASE {DatabaseName};
                     """
                );
            }
            catch (SqlException e)
            {
                throw new Exception("Create and run the container found in the docker-compose.yml", e);
            }
        }

        {
            await using var connection = await GetOpenConnectionAsync();
            await connection.ExecuteAsync(
                // lang=SQL
                """
                IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[People]') AND type in (N'U'))
                  BEGIN
                      CREATE TABLE People
                      (
                          ID INT PRIMARY KEY IDENTITY,
                          FirstName NVARCHAR(50),
                          LastName NVARCHAR(50),
                          Age INT,
                          Email NVARCHAR(255)
                      );
                  END;
                """);
        }
            
    }

    public Task ResetAsync()
        => respawn.ResetAsync(connectionStrings[DatabaseName]);

    public async Task DisposeAsync()
    {
        await using var connection = await GetOpenConnectionAsync("master");
        await connection.ExecuteAsync(
            // lang=SQL
            $"""
            IF EXISTS (SELECT [name] FROM sys.databases WHERE [name] = N'{DatabaseName}')
            BEGIN
                ALTER DATABASE {DatabaseName}
                SET SINGLE_USER -- Disallow multi-user access.
                WITH ROLLBACK IMMEDIATE -- Rollback any transaction in progress.
                DROP DATABASE {DatabaseName};
            END;
            """
        );
    }
}