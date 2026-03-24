namespace TodoApi.Infrastructure.Data;

public interface IMigration
{
    int Version { get; }
    string Description { get; }
    string UpSql { get; }
    string DownSql { get; }
}
