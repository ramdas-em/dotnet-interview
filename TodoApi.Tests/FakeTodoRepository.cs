using Microsoft.Data.Sqlite;
using TodoApi.Models;
using TodoApi.Repositories;

namespace TodoApi.Tests;

public class FakeTodoRepository : ITodoRepository, IDisposable
{
    private readonly SqliteConnection _connection;

    public FakeTodoRepository()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();
        InitializeDatabase();
    }

    private void InitializeDatabase()
    {
        using var command = _connection.CreateCommand();
        command.CommandText = @"
            CREATE TABLE IF NOT EXISTS Todos (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Title TEXT NOT NULL,
                Description TEXT,
                IsCompleted INTEGER NOT NULL DEFAULT 0,
                CreatedAt TEXT NOT NULL
            )
        ";
        command.ExecuteNonQuery();
    }

    public bool ExistsByTitle(string title)
    {
        using var command = _connection.CreateCommand();
        command.CommandText = "SELECT COUNT(1) FROM Todos WHERE Title = @Title";
        command.Parameters.AddWithValue("@Title", title);

        return Convert.ToInt32(command.ExecuteScalar()) > 0;
    }

    public Todo Create(Todo todo)
    {
        using var command = _connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO Todos (Title, Description, IsCompleted, CreatedAt)
            VALUES (@Title, @Description, @IsCompleted, @CreatedAt);
            SELECT last_insert_rowid();
        ";

        var now = DateTime.UtcNow;
        command.Parameters.AddWithValue("@Title", todo.Title);
        command.Parameters.AddWithValue("@Description", (object?)todo.Description ?? DBNull.Value);
        command.Parameters.AddWithValue("@IsCompleted", todo.IsCompleted ? 1 : 0);
        command.Parameters.AddWithValue("@CreatedAt", now.ToString("o"));

        var id = Convert.ToInt32(command.ExecuteScalar());
        todo.Id = id;
        todo.CreatedAt = now;
        return todo;
    }

    public List<Todo> GetAll()
    {
        var todos = new List<Todo>();
        using var command = _connection.CreateCommand();
        command.CommandText = "SELECT Id, Title, Description, IsCompleted, CreatedAt FROM Todos";

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            todos.Add(MapTodo(reader));
        }

        return todos;
    }

    public Todo? GetById(int id)
    {
        using var command = _connection.CreateCommand();
        command.CommandText = "SELECT Id, Title, Description, IsCompleted, CreatedAt FROM Todos WHERE Id = @Id";
        command.Parameters.AddWithValue("@Id", id);

        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            return MapTodo(reader);
        }

        return null;
    }

    public bool Update(int id, Todo todo)
    {
        using var command = _connection.CreateCommand();
        command.CommandText = @"
            UPDATE Todos
            SET Title = @Title, Description = @Description, IsCompleted = @IsCompleted
            WHERE Id = @Id
        ";
        command.Parameters.AddWithValue("@Title", todo.Title);
        command.Parameters.AddWithValue("@Description", (object?)todo.Description ?? DBNull.Value);
        command.Parameters.AddWithValue("@IsCompleted", todo.IsCompleted ? 1 : 0);
        command.Parameters.AddWithValue("@Id", id);

        var rowsAffected = command.ExecuteNonQuery();
        return rowsAffected > 0;
    }

    public bool Delete(int id)
    {
        using var command = _connection.CreateCommand();
        command.CommandText = "DELETE FROM Todos WHERE Id = @Id";
        command.Parameters.AddWithValue("@Id", id);

        var rowsAffected = command.ExecuteNonQuery();
        return rowsAffected > 0;
    }

    private static Todo MapTodo(SqliteDataReader reader)
    {
        return new Todo
        {
            Id = reader.GetInt32(0),
            Title = reader.GetString(1),
            Description = reader.IsDBNull(2) ? null : reader.GetString(2),
            IsCompleted = reader.GetInt32(3) == 1,
            CreatedAt = DateTime.Parse(reader.GetString(4))
        };
    }

    public void Dispose()
    {
        _connection?.Dispose();
    }
}
