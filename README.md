# RaceDay – Comrades Marathon Event Management System

## Project Overview
RaceDay is a web-based event management system developed for the Comrades Marathon event environment. The system provides an ASP.NET Core Web API for managing users, events, routes, categories, enrolments, race results and event weather information.

Part 2 continues the RaceDay system developed in Part 1 by implementing the planned RESTful API functionality using ASP.NET Core, Entity Framework Core and SQL Server.

The project is designed around role-based access for two main users:

* **Organiser** – manages events, categories, enrolments and race-related information.
* **Participant** – registers, logs in, manages their profile, enrols in events and views their information.

## Technologies Used
* **C#**
* **ASP.NET Core Web API**
* **.NET 8**
* **Entity Framework Core**
* **Microsoft SQL Server**
* **SQL Server Management Studio (SSMS)**
* **Swagger**
* **xUnit**
* **ASP.NET Core Web Application Testing**
* **Entity Framework Core In-Memory Database for automated tests**
* **Git and GitHub**
* **GitHub Actions**

## Project Structure

```text
RaceDay/
│
├── .github/
│   └── workflows/
│       └── part2-ci.yml
│
├── docs/
│   ├── API_EndPoint_Plan.md
│   ├── RaceDay-ERD.png
│   ├── RaceDayDatabase.sql
│   └── CI-Success.png
│
├── src/
│   └── RaceDay.Api/
│       ├── Controllers/
│       ├── Data/
│       ├── DTOs/
│       ├── Extensions/
│       ├── Filters/
│       ├── Models/
│       ├── Program.cs
│       └── RaceDay.Api.csproj
│
├── tests/
│   └── RaceDay.Api.Tests/
│       ├── ApiTests.cs
│       ├── CustomWebApplicationFactory.cs
│       └── RaceDay.Api.Tests.csproj
│
├── RaceDay.sln
└── README.md
```


## Part 1 Continuation
Part 2 was developed as a continuation of the RaceDay system created in Part 1.

The following Part 1 documentation and database resources are retained in the `docs` folder:

* RaceDay Entity Relationship Diagram
* RaceDay SQL database script
* API endpoint planning documentation
* Continuous Integration documentation/evidence

The Part 2 implementation follows the entities and relationships established in the Part 1 database design.

## Database
The RaceDay system uses **Microsoft SQL Server** as its main database management system.

The database contains the following main entities:

1. Users
2. Routes
3. Events
4. Categories
5. Enrolments
6. Results
7. EventWeather

The database creation script is provided in:

```text
docs/RaceDayDatabase.sql
```

The Entity Framework Core database context is located at:

```text
src/RaceDay.Api/Data/RaceDayDbContext.cs
```

The database connection is configured through the application's connection string.

### SQL Server Setup
Before running the API against SQL Server:

1. Open SQL Server Management Studio.
2. Connect to the required SQL Server instance.
3. Open `docs/RaceDayDatabase.sql`.
4. Execute the script to create the RaceDay database and tables.
5. Check the connection string in the application's configuration.
6. Ensure that the SQL Server instance is running.
7. Build and run the API.

## API Functionality
The Part 2 API provides endpoints for the main RaceDay system functionality.

### Authentication

```text
POST /api/auth/register
POST /api/auth/login
POST /api/auth/logout
```

These endpoints allow users to register, log in and log out.

Passwords are handled using password hashing rather than storing plain-text passwords.


### User Profile

```text
GET /api/users/profile
PUT /api/users/profile
```

Authenticated users can retrieve and update their profile information.

### Events

```text
GET    /api/events
GET    /api/events/{id}
POST   /api/events
PUT    /api/events/{id}
DELETE /api/events/{id}
```

Events can be viewed and managed according to the user's role.

Organisers are responsible for creating and managing events.

### Categories
The API provides functionality for managing categories associated with events.

```text
GET    /api/events/{eventId}/categories
POST   /api/events/{eventId}/categories
PUT    /api/events/{eventId}/categories/{categoryId}
DELETE /api/events/{eventId}/categories/{categoryId}
```


### Enrolments
Participants can enrol in events and view their enrolment information.

Organisers can manage enrolments associated with their events.

The enrolment functionality supports statuses such as:
* Pending
* Confirmed
* Cancelled

### Results
The API provides functionality for creating, updating and viewing race results.
Results are associated with participant enrolments.

### Weather
The API provides functionality for viewing and adding weather information associated with an event.

Weather information includes:

* Weather date
* Temperature
* Weather condition
* Humidity

## Role-Based Access
RaceDay uses two main roles:

### Organiser
Organisers can perform administrative event-management operations such as:

* Creating events
* Updating events
* Deleting events
* Managing event categories
* Managing participant enrolments
* Managing race information

### Participant
Participants can:

* Register
* Log in
* View and update their profile
* View events
* Enrol in events
* View their own enrolments
* View relevant race information

Access restrictions are implemented in the API to prevent users from performing operations outside their assigned role.

## Running the Application

### Requirements

The following software is required:

* .NET 8 SDK
* Visual Studio 2022 or later
* SQL Server
* SQL Server Management Studio
* Git

### Step 1 – Open the Project

Open:

```text
RaceDay.sln
```

in Visual Studio.

### Step 2 – Restore NuGet Packages
Visual Studio should restore the required NuGet packages automatically.

If required, use:

```text
Build → Rebuild Solution
```

### Step 3 – Configure the Database
Ensure that the SQL Server database has been created using:

```text
docs/RaceDayDatabase.sql
```

Check the application's database connection configuration before running the API.

### Step 4 – Run the API
Run the project from Visual Studio.

The application should start and open the Swagger interface.

Swagger provides an interactive interface for viewing and testing the available API endpoints.

## Swagger / OpenAPI
Swagger is configured for the RaceDay API.

When the application is running in the Development environment, the Swagger interface can be used to:

* View available endpoints
* View HTTP methods
* View request parameters
* View request and response structures
* Send requests to the API
* Examine API responses

Swagger is available through the application's local Swagger URL displayed when the API starts.

## Automated Testing
The RaceDay API includes an automated test project located in `tests/RaceDay.Api.Tests/`.

The project uses:

* xUnit for automated testing.
* ASP.NET Core `WebApplicationFactory` for API integration testing.
* Entity Framework Core In-Memory Database for isolated test data.

The automated tests were executed in Visual Studio using Test Explorer. **All tests passed successfully.**

To run the tests again, open the solution in Visual Studio and select:

`Test → Test Explorer → Run All`

Alternatively, run the following command from the solution directory:

```bash
dotnet test
```

The test database is separate from the main SQL Server database and does not require a separate SQL Server database named `RaceDayTests`.


### Running Tests
In Visual Studio:

```text
Test
→ Test Explorer
→ Run All
```

The tests can also be executed using the .NET CLI:

```bash
dotnet test
```

## Continuous Integration
A GitHub Actions workflow is included in:

```text
.github/workflows/part2-ci.yml
```

The workflow is intended to build and test the project automatically when changes are pushed to GitHub.

This provides automated verification of the project during development.

## API Documentation
The API endpoint planning from Part 1 is retained in:

```text
docs/API_EndPoint_Plan.md
```

The document provides the planned API routes and functionality used as the basis for the Part 2 implementation.


## Entity Relationship Diagram
The RaceDay database design is documented using the Entity Relationship Diagram retained from Part 1:

```text
docs/RaceDay-ERD.png
```

The ERD shows the relationships between the RaceDay database entities.

## Security
The application includes basic security measures appropriate for the API implementation, including:

* Password hashing
* Authentication checks
* Role-based access restrictions
* Session-based authentication
* Protected user profile operations
* Validation of API requests

Passwords are not intended to be stored as plain text.

## Development and Version Control
The project is maintained using Git and GitHub.

Development commits should document meaningful changes to the project, such as:

* API implementation
* Database integration
* Controller development
* Authentication
* Authorisation
* Testing
* Bug fixes
* Documentation
* Continuous Integration

## Project Status
### Part 1

The following Part 1 resources have been retained:

* Database design
* SQL database script
* ERD
* API endpoint plan
* Project documentation

### Part 2
The Part 2 project contains:

* ASP.NET Core Web API
* Entity Framework Core database context
* SQL Server configuration
* API controllers
* Data models
* DTOs
* Authentication functionality
* Role-based access controls
* Automated API tests
* Swagger/OpenAPI documentation
* GitHub Actions workflow

The API should be built and tested in Visual Studio before final submission.

## Author
**RaceDay – Comrades Marathon Event Management System**
**Unanne Rofulufhela Munaka**
Programming POE – Part 2
