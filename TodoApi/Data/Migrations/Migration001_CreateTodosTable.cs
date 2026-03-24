namespace TodoApi.Data.Migrations;

public class Migration001_CreateTodosTable : IMigration
{
    public int Version => 1;

    public string Description => "Create Todos table";

    public string UpSql => @"
        CREATE TABLE Todos (
            Id INT IDENTITY(1,1) PRIMARY KEY,
            Title NVARCHAR(200) NOT NULL,
            Description NVARCHAR(MAX),
            IsCompleted BIT NOT NULL DEFAULT 0,
            CreatedAt DATETIME2 NOT NULL
        )";

    public string DownSql => @"DROP TABLE IF EXISTS Todos";
}
