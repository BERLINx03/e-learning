# E-Learning Platform Backend

A .NET backend for an e-learning platform with support for multiple user roles (Admin, Instructor, Student) and comprehensive course management features.

## Features

### Admin Features

- User management and reports
- User timeout functionality
- System-wide monitoring

### Instructor Features

- Course creation and management
- Lesson management (videos, documents, quizzes)
- Student progress tracking
- Course messaging system
- Course analytics

### Student Features

- Course enrollment
- Video and document access
- Quiz completion
- Progress tracking
- Certificate generation
- Course messaging

## Project Structure

The solution is organized into four main projects:

1. **ELearning.Data**

   - Entity models
   - Database context
   - Migrations

2. **ELearning.Repositories**

   - Repository interfaces
   - Repository implementations
   - Data access layer

3. **ELearning.Services**

   - Service interfaces
   - Service implementations
   - Business logic

4. **ELearning.API**
   - Controllers
   - API endpoints
   - Authentication/Authorization
   - Swagger documentation

## Prerequisites

- .NET 6.0 or later
- SQL Server
- Visual Studio 2022 or VS Code

## Setup

1. Clone the repository
2. Update the connection string in `appsettings.json`
3. Run the following commands:
   ```bash
   dotnet restore
   dotnet ef migrations add InitialCreate
   dotnet ef database update
   ```
4. Run the application:
   ```bash
   dotnet run --project ELearning.API
   ```

## API Documentation

Once the application is running, you can access the Swagger documentation at:

```
https://localhost:5001/swagger
```

## Security

- JWT-based authentication
- Role-based authorization
- Password hashing
- CORS configuration

## Contributing

1. Fork the repository
2. Create a feature branch
3. Commit your changes
4. Push to the branch
5. Create a Pull Request
