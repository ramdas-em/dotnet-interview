namespace TodoApi.Data.Migrations;

public class Migration002_AddUniqueTitleIndex : IMigration
{
    public int Version => 2;

    public string Description => "Add unique index on Todos Title column";

    public string UpSql => @"
        CREATE UNIQUE INDEX IX_Todos_Title ON Todos (Title)";

    public string DownSql => @"DROP INDEX IF EXISTS IX_Todos_Title ON Todos";
}
