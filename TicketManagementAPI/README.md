# Enterprise Ticket Management System - ASP.NET Core API

## Overview
Backend API for the Enterprise Ticket Management System built with ASP.NET Core Web API.

## Technology Stack
- **Framework**: ASP.NET Core 8.0
- **ORM**: Entity Framework Core 8.0
- **Database**: SQLite (default) / SQL Server
- **API Documentation**: Swagger/OpenAPI

## Features
- RESTful API endpoints for ticket management
- CRUD operations for tickets and comments
- Real-time statistics
- Search functionality
- CORS enabled for Angular frontend
- Entity Framework Core with Code-First approach
- Swagger UI for API testing

## Prerequisites
- .NET 8.0 SDK or later
- SQL Server (optional, SQLite is default)

## Database Configuration
The application supports both SQLite and SQL Server.

### SQLite (Default)
No additional configuration needed. Database file will be created automatically.

### SQL Server
1. Update `appsettings.json`:
   ```json
   "DatabaseProvider": "SqlServer"
   ```
2. Update connection string if needed

## Running the API

### Using Command Line
```bash
cd TicketManagementAPI
dotnet restore
dotnet run
```

### Using Visual Studio
1. Open `TicketManagementAPI.csproj`
2. Press F5 to run

The API will start at:
- HTTP: http://localhost:5000
- Swagger UI: http://localhost:5000/swagger

## API Endpoints

### Tickets
- `GET /api/tickets` - Get all tickets
- `GET /api/tickets/{id}` - Get ticket by ID
- `GET /api/tickets/stats` - Get ticket statistics
- `GET /api/tickets/search?term={searchTerm}` - Search tickets
- `POST /api/tickets` - Create new ticket
- `PUT /api/tickets/{id}` - Update ticket
- `DELETE /api/tickets/{id}` - Delete ticket

### Comments
- `GET /api/comments/ticket/{ticketId}` - Get comments for a ticket
- `POST /api/comments` - Add new comment
- `DELETE /api/comments/{id}` - Delete comment

## Database Models

### Ticket
- Id (string)
- Title (string, required)
- Description (string, required)
- Category (string)
- Priority (string)
- Status (string)
- AssignedTo (string)
- ReportedBy (string)
- CreatedAt (DateTime)
- UpdatedAt (DateTime)
- ResolvedAt (DateTime?)
- Comments (Collection)

### Comment
- Id (string)
- TicketId (string)
- Author (string)
- Text (string)
- CreatedAt (DateTime)

## CORS Configuration
CORS is configured to allow requests from the Angular frontend running on `http://localhost:4200`.

## Sample Data
The database is seeded with 5 sample tickets on first run.
