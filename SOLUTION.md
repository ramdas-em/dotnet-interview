# Solution Documentation


**Candidate Name:** Ramdas Warhad
**Completion Date:** 25 Mrach 2026

---


## Problems Identified
- **Dynamic Access to Anonymous Types:** Tests used `dynamic` to access properties of anonymous objects returned by controllers, causing `RuntimeBinderException` at runtime.
- **Case Sensitivity in Authentication:** The authentication logic was case-sensitive for both username and password, which could lead to usability issues.
- **Lack of Layered Separation:** Some logic was not clearly separated into services, DTOs, and repositories.
- **No JWT Authentication Documentation:** API usage for authentication was not clearly documented.

## Architectural Decisions
- **Layered Architecture:** Clear separation between Controllers, Services, DTOs, and Repositories for maintainability and testability.
- **Case-Insensitive Authentication:** Usernames and passwords are now compared case-insensitively for better UX.
- **Anonymous Type Handling in Tests:** Tests now use reflection to access anonymous type properties, avoiding runtime errors.
- **JWT Authentication:** Secure endpoints with JWT, requiring tokens for all `/api/todos` endpoints.

## Trade-offs

- **Raw SQL over EF Core**: Kept raw SQL with parameterized queries rather than introducing Entity Framework Core. This keeps the change scope manageable while still fixing the SQL injection vulnerability. EF Core would be the next step for maintainability.
- **Synchronous API**: Kept synchronous methods to minimize the scope of changes. In production, all database calls should be async.
- **In-memory fake vs mocking library**: Used a simple hand-written `FakeTodoRepository` instead of adding a mocking framework (e.g., Moq). This keeps the test project dependency-free and the tests easy to understand.
- **No global error handling middleware**: Kept error handling simple. In production, a global exception handler middleware would provide consistent error responses.

---

## How to Run

### Prerequisites
- .NET 8 SDK

### Build
```bash
dotnet build
```

### Run
```bash
dotnet run --project TodoApi
```

The API will be available at the URL shown in console output (e.g., `https://localhost:5001`).  
Swagger UI is available at `/swagger` in development mode.

### Test
```bash
dotnet test
```

---

## API Documentation

### Endpoints

#### Create TODO
```
Method: POST
URL: /api/todos
Request Body:
{
  "title": "Buy groceries",
  "description": "Milk, eggs, bread"
}
Response (201 Created):
{
  "id": 1,
  "title": "Buy groceries",
  "description": "Milk, eggs, bread",
  "isCompleted": false,
  "createdAt": "2024-01-15T10:30:00Z"
}
```

#### Get All TODOs
```
Method: GET
URL: /api/todos
Response (200 OK):
[
  {
    "id": 1,
    "title": "Buy groceries",
    "description": "Milk, eggs, bread",
    "isCompleted": false,
    "createdAt": "2024-01-15T10:30:00Z"
  }
]
```

#### Get TODO by ID
```
Method: GET
URL: /api/todos/{id}
Response (200 OK):
{
  "id": 1,
  "title": "Buy groceries",
  "description": "Milk, eggs, bread",
  "isCompleted": false,
  "createdAt": "2024-01-15T10:30:00Z"
}
Response (404 Not Found) when ID does not exist
```

#### Update TODO
```
Method: PUT
URL: /api/todos/{id}
Request Body:
{
  "title": "Buy groceries",
  "description": "Milk, eggs, bread, butter",
  "isCompleted": true
}
Response (200 OK):
{
  "id": 1,
  "title": "Buy groceries",
  "description": "Milk, eggs, bread, butter",
  "isCompleted": true,
  "createdAt": "2024-01-15T10:30:00Z"
}
Response (404 Not Found) when ID does not exist
```

#### Delete TODO
```
Method: DELETE
URL: /api/todos/{id}
Response (204 No Content) on success
Response (404 Not Found) when ID does not exist
```

---

## Future Improvements

- **Async/await**: Make all data access methods asynchronous for better scalability under load.
- **Entity Framework Core**: Replace raw SQL with EF Core for better maintainability, migrations, and LINQ support.
- **Logging**: Add structured logging with `ILogger<T>` throughout the application.
- **Global exception handling**: Add middleware for consistent error responses with problem details (RFC 7807).
- **Pagination**: Add paging, sorting, and filtering to the `GET /api/todos` endpoint.
- **FluentValidation**: Replace data annotation validation with FluentValidation for complex validation rules.
- **Integration tests**: Add integration tests using `WebApplicationFactory<T>` to test the full HTTP pipeline.
- **Docker support**: Add a `Dockerfile` for containerized deployment.
- **Health checks**: Add health check endpoints for monitoring.
- **API versioning**: Add versioning support for future API evolution.
- **CQRS considerations**: Separate read and write models if the application grows in complexity.
