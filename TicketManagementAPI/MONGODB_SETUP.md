# MongoDB Migration - Ticket Management System

## Overview
This project has been successfully migrated from SQLite to MongoDB.

## MongoDB Configuration

### Connection Details
- **Connection String**: `mongodb://localhost:27017`
- **Database Name**: `TicketManagementDB`
- **Collections**: 
  - `Tickets` - Main ticket documents
  - `Comments` - Comment documents

### Viewing Data in MongoDB Compass

1. **Open MongoDB Compass**
2. **Connect using**: `mongodb://localhost:27017`
3. **Navigate to Database**: Select `TicketManagementDB` from the database list
4. **View Collections**: 
   - Click on `Tickets` to view all tickets
   - Click on `Comments` to view all comments

### Data Structure

#### Ticket Document
```json
{
  "_id": "ObjectId",
  "title": "string",
  "description": "string",
  "category": "string",
  "priority": "string",
  "status": "string",
  "assignedTo": "string",
  "reportedBy": "string",
  "createdAt": "ISODate",
  "updatedAt": "ISODate",
  "resolvedAt": "ISODate (optional)",
  "comments": [
    {
      "_id": "ObjectId",
      "ticketId": "string",
      "author": "string",
      "text": "string",
      "createdAt": "ISODate"
    }
  ]
}
```

#### Comment Document
```json
{
  "_id": "ObjectId",
  "ticketId": "string",
  "author": "string",
  "text": "string",
  "createdAt": "ISODate"
}
```

## Key Changes Made

1. **Removed Dependencies**:
   - Microsoft.EntityFrameworkCore
   - Microsoft.EntityFrameworkCore.Sqlite
   - Microsoft.EntityFrameworkCore.SqlServer

2. **Added Dependencies**:
   - MongoDB.Driver (v3.5.2)

3. **Updated Files**:
   - `Models/Ticket.cs` - Added MongoDB BSON attributes
   - `Models/Comment.cs` - Added MongoDB BSON attributes
   - `Data/MongoDbService.cs` - New MongoDB service layer
   - `Controllers/TicketsController.cs` - Updated to use MongoDB queries
   - `Controllers/CommentsController.cs` - Updated to use MongoDB queries
   - `Program.cs` - Configured MongoDB service
   - `appsettings.json` - Updated with MongoDB connection string
   - `appsettings.Development.json` - Updated with MongoDB connection string

4. **Deleted Files**:
   - `Data/TicketDbContext.cs` - Old Entity Framework DbContext
   - All `.db` SQLite database files

## Running the Application

1. **Ensure MongoDB is running**:
   ```bash
   # MongoDB should be running on localhost:27017
   ```

2. **Start the API**:
   ```bash
   cd TicketManagementAPI
   dotnet run
   ```

3. **The API will**:
   - Connect to MongoDB at `mongodb://localhost:27017`
   - Create the `TicketManagementDB` database if it doesn't exist
   - Seed initial data on first run
   - Be available at `https://localhost:5001` or `http://localhost:5000`

4. **View in MongoDB Compass**:
   - Open MongoDB Compass
   - Connect to `mongodb://localhost:27017`
   - Browse the `TicketManagementDB` database

## API Endpoints (Unchanged)

All API endpoints remain the same:
- `GET /api/Tickets` - Get all tickets
- `GET /api/Tickets/{id}` - Get ticket by ID
- `GET /api/Tickets/stats` - Get ticket statistics
- `GET /api/Tickets/search?term={term}` - Search tickets
- `POST /api/Tickets` - Create new ticket
- `PUT /api/Tickets/{id}` - Update ticket
- `DELETE /api/Tickets/{id}` - Delete ticket
- `GET /api/Comments/ticket/{ticketId}` - Get comments for ticket
- `POST /api/Comments` - Add comment
- `DELETE /api/Comments/{id}` - Delete comment

## Notes

- Comments are stored both within the Ticket document (embedded) and as separate documents in the Comments collection for querying flexibility
- All dates are stored as UTC
- MongoDB ObjectIds are used for document IDs
- The Angular frontend requires no changes as the API interface remains the same
