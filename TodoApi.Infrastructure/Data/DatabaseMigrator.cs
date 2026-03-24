using Microsoft.Data.SqlClient;

namespace TodoApi.Infrastructure.Data;

public class DatabaseMigrator
{
    private readonly string _connectionString;
    private readonly List<IMigration> _migrations;

    public DatabaseMigrator(string connectionString)
    {
        _connectionString = connectionString;
        _migrations = DiscoverMigrations();
    }

    public void Migrate()
    {
        EnsureDatabaseExists();
        EnsureMigrationHistoryTable();

        var appliedVersions = GetAppliedMigrationVersions();

        var pendingMigrations = _migrations
            .Where(m => !appliedVersions.Contains(m.Version))
            .OrderBy(m => m.Version)
            .ToList();

        if (pendingMigrations.Count == 0)
        {
            Console.WriteLine("Database is up to date. No pending migrations.");
            return;
        }

        foreach (var migration in pendingMigrations)
        {
            ApplyMigration(migration);
        }
    }

    private void EnsureDatabaseExists()
    {
        var builder = new SqlConnectionStringBuilder(_connectionString);
        var databaseName = builder.InitialCatalog;

        if (string.IsNullOrEmpty(databaseName))
        {
            return;
        }

        builder.InitialCatalog = "master";
        using var connection = new SqlConnection(builder.ConnectionString);
        connection.Open();

        using var checkCommand = connection.CreateCommand();
        checkCommand.CommandText = "SELECT DB_ID(@DatabaseName)";
        checkCommand.Parameters.AddWithValue("@DatabaseName", databaseName);

        var result = checkCommand.ExecuteScalar();
        if (result == DBNull.Value || result == null)
        {
            Console.WriteLine($"Creating database '{databaseName}'...");
            using var createCommand = connection.CreateCommand();
            createCommand.CommandText = $"CREATE DATABASE [{databaseName.Replace("]", "]]")}]";
            createCommand.ExecuteNonQuery();
            Console.WriteLine($"Database '{databaseName}' created successfully.");
        }
        else
        {
            Console.WriteLine($"Database '{databaseName}' already exists.");
        }
    }

    private void EnsureMigrationHistoryTable()
    {
        using var connection = new SqlConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='__MigrationHistory' AND xtype='U')
            CREATE TABLE __MigrationHistory (
                Version INT NOT NULL PRIMARY KEY,
                Description NVARCHAR(500) NOT NULL,
                AppliedOn DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
            )";
        command.ExecuteNonQuery();
    }

    private HashSet<int> GetAppliedMigrationVersions()
    {
        using var connection = new SqlConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT Version FROM __MigrationHistory";

        var versions = new HashSet<int>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            versions.Add(reader.GetInt32(0));
        }

        return versions;
    }

    private void ApplyMigration(IMigration migration)
    {
        Console.WriteLine($"Applying migration {migration.Version}: {migration.Description}...");

        using var connection = new SqlConnection(_connectionString);
        connection.Open();

        using var transaction = connection.BeginTransaction();
        try
        {
            using var migrationCommand = connection.CreateCommand();
            migrationCommand.Transaction = transaction;
            migrationCommand.CommandText = migration.UpSql;
            migrationCommand.ExecuteNonQuery();

            using var historyCommand = connection.CreateCommand();
            historyCommand.Transaction = transaction;
            historyCommand.CommandText = @"
                INSERT INTO __MigrationHistory (Version, Description, AppliedOn)
                VALUES (@Version, @Description, SYSUTCDATETIME())";
            historyCommand.Parameters.AddWithValue("@Version", migration.Version);
            historyCommand.Parameters.AddWithValue("@Description", migration.Description);
            historyCommand.ExecuteNonQuery();

            transaction.Commit();
            Console.WriteLine($"Migration {migration.Version} applied successfully.");
        }
        catch
        {
            transaction.Rollback();
            Console.WriteLine($"Migration {migration.Version} failed. Transaction rolled back.");
            throw;
        }
    }

    private static List<IMigration> DiscoverMigrations()
    {
        return typeof(DatabaseMigrator).Assembly
            .GetTypes()
            .Where(t => typeof(IMigration).IsAssignableFrom(t) && t is { IsAbstract: false, IsInterface: false })
            .Select(t => (IMigration)Activator.CreateInstance(t)!)
            .OrderBy(m => m.Version)
            .ToList();
    }
}
