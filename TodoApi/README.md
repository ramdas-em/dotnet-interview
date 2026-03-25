# TODO API

A simple TODO API built with ASP.NET Core 8.0.

## Getting Started

### Prerequisites
- .NET 8.0 SDK

### How to Run the Application (Step by Step)
1. **Clone the repository (if not already done):**
   ```sh
   git clone https://github.com/ramdas-em/dotnet-interview.git
   cd dotnet-interview
   ```
2. **Restore dependencies:**
   ```sh
   dotnet restore
   ```
3. **Build the solution:**
   ```sh
   dotnet build
   ```
4. **Run database migrations (if required):**
   - The app will auto-create the database on first run. For manual migration, see project documentation.
5. **Run the application:**
   ```sh
   dotnet run --project TodoApi
   ```
6. **Access the API:**
   - The API will be available at the URL shown in the console output (e.g., `http://localhost:5164`).
7. **Access Swagger UI:**
   - Open `http://localhost:5164/swagger` in your browser to explore and test the API endpoints interactively.
8. **Run tests:**
   ```sh
   dotnet test TodoApi.Tests/TodoApi.Tests.csproj
   ```


## API Endpoints

### Authentication
- **POST /api/auth/login**
  - Request body:
    ```json
    {
      "username": "admin",
      "password": "password"
    }
    ```
  - Response:
    ```json
    {
      "token": "<jwt-token>"
    }
    ```

### Using the JWT Token
- For all protected endpoints (all except `/api/auth/login`), include the token in the `Authorization` header:
  ```http
  Authorization: Bearer <jwt-token>
  ```

All endpoints are under `/api`:

- `POST /api/createTodo` - Create a new TODO item
- `POST /api/getTodo` - Get TODO item(s)
- `POST /api/updateTodo` - Update a TODO item
- `POST /api/deleteTodo` - Delete a TODO item

## Testing

Run the tests with:
```
cd TodoApi.Tests
dotnet test
```

## Database

The application uses SQLite with a file-based database (`todos.db`) that is automatically created on startup.