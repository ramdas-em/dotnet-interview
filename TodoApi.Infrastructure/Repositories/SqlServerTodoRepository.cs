using Microsoft.Data.SqlClient;
using TodoApi.Domain.Entities;
using TodoApi.Domain.Repositories;

namespace TodoApi.Infrastructure.Repositories;

public class SqlServerTodoRepository : ITodoRepository
{
    private readonly string _connectionString;

    public SqlServerTodoRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public bool ExistsByTitle(string title)
    {
        using var connection = new SqlConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(1) FROM Todos WHERE Title = @Title";
        command.Parameters.AddWithValue("@Title", title);

        return Convert.ToInt32(command.ExecuteScalar()) > 0;
    }

    public Todo Create(Todo todo)
    {
        using var connection = new SqlConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO Todos (Title, Description, IsCompleted, CreatedAt)
            OUTPUT INSERTED.Id
            VALUES (@Title, @Description, @IsCompleted, @CreatedAt);
        ";

        var now = DateTime.UtcNow;
        command.Parameters.AddWithValue("@Title", todo.Title);
        command.Parameters.AddWithValue("@Description", (object?)todo.Description ?? DBNull.Value);
        command.Parameters.AddWithValue("@IsCompleted", todo.IsCompleted);
        command.Parameters.AddWithValue("@CreatedAt", now);

        var id = Convert.ToInt32(command.ExecuteScalar());
        todo.Id = id;
        todo.CreatedAt = now;
        return todo;
    }

    public List<Todo> GetAll()
    {
        var todos = new List<Todo>();
        using var connection = new SqlConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
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
        using var connection = new SqlConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
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
        using var connection = new SqlConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            UPDATE Todos
            SET Title = @Title, Description = @Description, IsCompleted = @IsCompleted
            WHERE Id = @Id
        ";
        command.Parameters.AddWithValue("@Title", todo.Title);
        command.Parameters.AddWithValue("@Description", (object?)todo.Description ?? DBNull.Value);
        command.Parameters.AddWithValue("@IsCompleted", todo.IsCompleted);
        command.Parameters.AddWithValue("@Id", id);

        var rowsAffected = command.ExecuteNonQuery();
        return rowsAffected > 0;
    }

    public bool Delete(int id)
    {
        using var connection = new SqlConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM Todos WHERE Id = @Id";
        command.Parameters.AddWithValue("@Id", id);

        var rowsAffected = command.ExecuteNonQuery();
        return rowsAffected > 0;
    }

    private static Todo MapTodo(SqlDataReader reader)
    {
        return new Todo
        {
            Id = reader.GetInt32(0),
            Title = reader.GetString(1),
            Description = reader.IsDBNull(2) ? null : reader.GetString(2),
            IsCompleted = reader.GetBoolean(3),
            CreatedAt = reader.GetDateTime(4)
        };
    }
}
