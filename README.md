# Eventmanagement
# Event Management System

## 1. Project Overview

The Event Management System is a web-based application developed using C# and ASP.NET Core. The purpose of this project is to simplify the management of sporting events, participants, organisers, enrolments and event results.

The system provides a central platform where organisers can create and manage events, while participants can register, log in, browse available events and enrol in competitions.

The application uses an ASP.NET Core Web API to handle requests and communicate with a Microsoft SQL Server database. An ASP.NET Core MVC website provides the user interface.

## 2. Project Objectives

The main objectives of the Event Management System are to:

- Develop a system for managing sporting events.
- Allow users to register and log in securely.
- Allow organisers to create, update and manage events.
- Allow participants to enrol in available events.
- Manage event categories and participant information.
- Store and manage event results.
- Implement role-based access control.
- Provide a reliable database structure.
- Test the application to ensure that its functionalities work correctly.

## 3. Main Features and Functionalities

### User Registration and Login

Users can create accounts and log in to the system. Authentication protects restricted features and ensures that users access the system according to their assigned roles.

### Event Management

Organisers can create new events, view existing events, update event information and delete events when necessary.

### Participant Management

The system stores participant information and allows participants to access their accounts and relevant event information.

### Event Categories

Events can be organised into different categories. This helps organisers manage competitions and participants more effectively.

### Participant Enrolments

Participants can enrol in available events. The system stores enrolment information so that organisers can manage and review registrations.

### Results Management

The system supports recording and managing event results. Participants can also view their own results.

### Event Image Uploads

The application supports event banner image uploads using Azure Blob Storage.

## 4. User Roles

The system supports different user roles with different access permissions.

| User Role | Responsibilities |
|---|---|
| Participant | Register, log in, browse events, enrol and view personal results. |
| Organiser | Create and manage events, review enrolments and manage results. |
| Admin | Access authorised administrative functionalities. |

Role-based authorisation helps protect the system from unauthorised access.

## 5. Technologies Used

| Technology | Purpose |
|---|---|
| C# | Main programming language |
| .NET 10 | Development framework |
| ASP.NET Core Web API | Backend API development |
| ASP.NET Core MVC | Website development |
| Microsoft SQL Server | Database management |
| Entity Framework Core | Database communication |
| JWT Authentication | Secure API authentication |
| Cookie Authentication | MVC user authentication |
| Swagger / OpenAPI | API documentation and testing |
| Azure Blob Storage | Image storage |
| xUnit | Automated unit testing |
| Docker | Application containerisation |

## 6. Project Structure

The solution is organised into three main projects.

```text
EventManagementApi/
│
├── EventManagementApi/
│   ├── Controllers/
│   ├── Data/
│   ├── DTOs/
│   ├── Mappings/
│   ├── Middleware/
│   ├── Models/
│   │   └── Entities/
│   ├── Services/
│   ├── Properties/
│   ├── Dockerfile
│   ├── Program.cs
│   └── appsettings.json
│
├── EventManagement.Mvc/
│   ├── Controllers/
│   ├── Models/
│   │   └── ViewModels/
│   ├── Services/
│   ├── Views/
│   ├── wwwroot/
│   ├── Dockerfile
│   ├── Program.cs
│   └── appsettings.json
│
├── EventManagementApi.Tests/
│   ├── Controllers/
│   └── Helpers/
│
├── docker-compose.yml
└── EventManagementApi.slnx
```

### Folder Descriptions

**Controllers:** Handle incoming API and website requests.

**Data:** Contains the database context and Entity Framework Core configuration.

**DTOs:** Contains data transfer objects used when sending and receiving information.

**Models:** Contains the application entity classes.

**Services:** Contains business logic and services used by the application.

**Views:** Contains the Razor pages displayed on the MVC website.

**wwwroot:** Stores static website files such as CSS, JavaScript and images.

**EventManagementApi.Tests:** Contains automated tests for checking application functionality.

## 7. System Requirements

Before running the application, make sure the following software is installed:

- .NET 10 SDK.
- Visual Studio 2022 with ASP.NET and web development tools.
- Microsoft SQL Server.
- SQL Server Management Studio (SSMS).
- Docker Desktop (for Docker deployment).
- Internet connection for restoring NuGet packages.

Check the installed .NET version using:

```bash
dotnet --version
```

## 8. Database Configuration

The application uses Microsoft SQL Server to store information.

The database manages the following entities:

- Users
- Organisers
- Participants
- Events
- Categories
- EventCategories
- Enrolments
- Results

Entity Framework Core is used to communicate with the database.

Configure the database connection string in the API application settings.

Example:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=YOUR_DATABASE;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

Replace `YOUR_SERVER` and `YOUR_DATABASE` with your actual SQL Server details.

The database schema must exist and match the application's entity configuration before running the application. Database migration files are not included in the supplied project.

## 9. Application Configuration

The application requires configuration for database connectivity, JWT authentication and Azure Blob Storage.

### JWT Configuration

JWT is used to authenticate API requests.

The required settings are:

- Jwt:Key
- Jwt:Issuer
- Jwt:Audience

For local development, configure the signing key using .NET User Secrets.

```bash
dotnet user-secrets init
dotnet user-secrets set "Jwt:Key" "YOUR_SECRET_KEY"
```

Never publish your actual JWT signing key on GitHub.

### Azure Blob Storage

The application uses Azure Blob Storage for event banners and profile pictures.

Configure the following setting:

```text
AzureBlobStorage:ConnectionString
```

Use a secure storage connection string and do not commit it to GitHub.

## 10. How to Run the Application

### Step 1: Clone the Repository

```bash
git clone YOUR_GITHUB_REPOSITORY_URL
```

### Step 2: Open the Project

Open the `EventManagementApi.slnx` solution in Visual Studio.

### Step 3: Restore NuGet Packages

Run the following command in the solution directory:

```bash
dotnet restore
```

### Step 4: Configure the Database

Make sure SQL Server is running and configure the `DefaultConnection` connection string.

### Step 5: Run the Web API

```bash
dotnet run --project EventManagementApi/EventManagementApi.csproj
```

The API is configured to use:

```text
http://localhost:5237
```

### Step 6: Open Swagger

Open the following address in your browser:

```text
http://localhost:5237/swagger
```

Swagger allows developers to view and test available API endpoints.

### Step 7: Run the MVC Website

Open another terminal and execute:

```bash
dotnet run --project EventManagement.Mvc/EventManagement.Mvc.csproj
```

The MVC website is available at:

```text
http://localhost:5057
```

Make sure the API is running before using the website features that communicate with it.

## 11. API Endpoints

The system provides REST API endpoints for different application functionalities.

| HTTP Method | Endpoint | Description |
|---|---|---|
| POST | /api/Auth/register | Register a new user |
| POST | /api/Auth/login | Log in to the system |
| GET | /api/Events | Retrieve events |
| GET | /api/Events/{id} | Retrieve a specific event |
| POST | /api/Events | Create an event |
| PUT | /api/Events/{id} | Update an event |
| DELETE | /api/Events/{id} | Delete an event |
| GET | /api/Enrolments/my | View personal enrolments |
| POST | /api/Enrolments | Create an enrolment |
| GET | /api/Results/my | View personal results |
| GET | /api/Results | Retrieve results |
| POST | /api/Results | Create a result |

Some endpoints require authentication and specific user roles.

## 12. Testing

The project includes an automated testing project using xUnit.

Testing helps ensure that the application behaves correctly and that API controllers return the expected responses.

The tests cover different areas, including:

- Events
- Enrolments
- Categories
- Organisers
- Participants
- Results
- Users

Run the tests using:

```bash
dotnet test EventManagementApi.Tests/EventManagementApi.Tests.csproj
```

To build the complete solution, execute:

```bash
dotnet build EventManagementApi.slnx
```

## 13. Docker Deployment

Docker is used to package and run the application in a containerised environment.

The project includes Dockerfiles for the API and MVC applications, as well as a Docker Compose configuration for the MVC website.

To build and run the MVC container, execute:

```bash
docker compose up --build
```

The MVC website is configured to run at:

```text
http://localhost:8081
```

The Docker configuration connects the MVC website to the API using:

```text
http://host.docker.internal:5237/
```

Make sure the API and database are running and accessible.

To stop the Docker container, execute:

```bash
docker compose down
```

## 14. Security

Security is an important part of the Event Management System.

The application uses JWT authentication to secure API requests and role-based authorisation to restrict access to protected functionalities.

Security measures include:

- User authentication.
- Role-based access control.
- Protected API endpoints.
- Secure database configuration.
- Restricted access to participant information.
- Secure storage of application credentials.

Passwords, database credentials, JWT keys and storage connection strings should never be committed to a public repository.

## 15. Future Improvements

The system can be improved by introducing additional functionalities in future development stages.

Possible improvements include:

- Email notifications for successful enrolments.
- Online event payments.
- Advanced event search and filtering.
- Event attendance tracking.
- More detailed reporting and analytics.
- Improved mobile responsiveness.
- Complete Docker Compose deployment for the API and MVC website.
- Continuous Integration and Continuous Deployment using GitHub Actions.

## 16. Conclusion

The Event Management System provides a centralised platform for managing sporting events, organisers, participants, enrolments and results.

The project demonstrates the use of modern software development technologies, including C#, ASP.NET Core, Entity Framework Core, SQL Server, JWT authentication, xUnit and Docker.

The application aims to improve event administration, organise participant information and provide a more efficient way of managing sporting events.

---

