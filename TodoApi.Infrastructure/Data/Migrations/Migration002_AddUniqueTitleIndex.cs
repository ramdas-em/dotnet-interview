namespace TodoApi.Infrastructure.Data.Migrations;

public class Migration002_AddUniqueTitleIndex : IMigration
{
    public int Version => 2;

    public string Description => "Add unique index on Todos Title column";

    public string UpSql => @"
        -- Remove duplicates, keeping the row with the lowest Id for each Title
        WITH Duplicates AS (
            SELECT Id,
                   ROW_NUMBER() OVER (PARTITION BY Title ORDER BY Id) AS rn
            FROM Todos
        )
        DELETE FROM Todos
        WHERE Id IN (
            SELECT Id FROM Duplicates WHERE rn > 1
        );

        -- Create the unique index if it does not exist
        IF NOT EXISTS (
            SELECT 1 FROM sys.indexes WHERE name = 'IX_Todos_Title' AND object_id = OBJECT_ID('Todos')
        )
            CREATE UNIQUE INDEX IX_Todos_Title ON Todos (Title);";

    public string DownSql => @"DROP INDEX IF EXISTS IX_Todos_Title ON Todos";
}
