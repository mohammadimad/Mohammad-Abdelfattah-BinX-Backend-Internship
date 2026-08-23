# Cardiac Monitor API

A secure RESTful API for managing cardiac-care data, built with ASP.NET Core 8 and SQL Server. The system provides role-based access to patient profiles, vital signs, medications, and medical appointments, with JWT authentication and refresh-token rotation.

## Features

- Patient profile management
- Cardiac vital-sign recording and history
- Medication and treatment tracking
- Medical appointment scheduling
- JWT bearer authentication
- Single-use refresh-token rotation
- Role-based authorization for `Admin`, `Doctor`, and `Patient`
- Patient ownership checks for protected medical data
- FluentValidation request validation
- Fixed-window rate limiting
- CORS policy configuration
- HTTPS redirection and production HSTS
- Swagger/OpenAPI documentation with JWT support
- Centralized exception handling with safe `ProblemDetails` responses
- Structured error logging with request and trace context
- Entity Framework Core migrations and seed data

## Technology Stack

| Component | Technology |
| --- | --- |
| Framework | ASP.NET Core 8 Web API |
| Language | C# |
| Database | Microsoft SQL Server |
| ORM | Entity Framework Core 8 |
| Authentication | ASP.NET Core Identity and JWT Bearer |
| Validation | FluentValidation |
| API documentation | Swagger / OpenAPI (Swashbuckle) |
| Rate limiting | ASP.NET Core Rate Limiting middleware |

## Project Structure

```text
CardiacMonitor/
├── Controllers/     # HTTP endpoints and authorization rules
├── Data/            # EF Core DbContext and migrations
├── DTOs/            # Request and response contracts
├── Models/          # Database entities
├── Services/        # Application and business logic
├── Validators/      # FluentValidation rules
├── Properties/      # Local launch profiles
├── Program.cs       # Service registration and middleware pipeline
└── appsettings.json # Application configuration
```

## Domain Model

- A patient may be linked one-to-one with an ASP.NET Core Identity user.
- A patient can have multiple vital-sign records.
- A patient can have multiple medications.
- A patient can have multiple appointments.
- Each appointment is assigned to one doctor.
- Each Identity user can have multiple stored refresh tokens.

Deleting a patient cascades to the patient's vital signs, medications, and appointments. Doctor deletion is restricted when the doctor is referenced by an appointment.

## Prerequisites

Install the following software before running the project:

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Microsoft SQL Server or SQL Server Express
- Optional: SQL Server Management Studio
- A valid ASP.NET Core HTTPS development certificate

Verify the installed SDK:

```powershell
dotnet --version
```

## Configuration

Update the connection string in `appsettings.json` for your SQL Server instance:

```json
{
  "ConnectionStrings": {
    "CardiacMonitorConnection": "Server=YOUR_SERVER;Database=CardiacMonitorDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
  }
}
```

The application also requires these JWT settings:

```json
{
  "Jwt": {
    "Key": "YOUR_LONG_RANDOM_SECRET_KEY",
    "Issuer": "CardiacMonitorAPI",
    "Audience": "CardiacMonitorAPI",
    "DurationInMinutes": 60
  }
}
```

> [!IMPORTANT]
> Never commit production database credentials or JWT signing keys. Use .NET User Secrets during local development and environment variables or a secret manager in production.

Example development secret:

```powershell
dotnet user-secrets init
dotnet user-secrets set "Jwt:Key" "YOUR_LONG_RANDOM_SECRET_KEY"
```

Update the `AllowFrontendOnly` CORS policy in `Program.cs` with the real origin of the frontend application. An origin must contain only the scheme, host, and port, without a trailing path.

## Database Setup

Restore dependencies and apply the existing EF Core migrations:

```powershell
dotnet restore
dotnet ef database update
```

If the `dotnet ef` command is unavailable, install the matching .NET 8 CLI tool:

```powershell
dotnet tool install --global dotnet-ef --version 8.*
```

The migrations seed the three application roles (`Admin`, `Doctor`, and `Patient`), sample patient and vital-sign data, and a development doctor account. Replace all seeded development credentials before deploying the application.

## Running the API

Ensure that the HTTPS development certificate is available:

```powershell
dotnet dev-certs https --trust
```

Run the project with the HTTPS launch profile:

```powershell
dotnet run --launch-profile https
```

Default development addresses:

- HTTPS: `https://localhost:7142`
- HTTP: `http://localhost:5142`
- Swagger UI: `https://localhost:7142/swagger`
- OpenAPI document: `https://localhost:7142/swagger/v1/swagger.json`

Swagger is enabled only when the application runs in the `Development` environment.

## Authentication

### Register

```http
POST /api/auth/register
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "StrongPassword123!",
  "role": "Patient"
}
```

### Log in

```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "StrongPassword123!"
}
```

A successful login returns an access token and a refresh token:

```json
{
  "token": "ACCESS_TOKEN",
  "refreshToken": "REFRESH_TOKEN",
  "message": "Tokens generated successfully."
}
```

Send the access token with protected requests:

```http
Authorization: Bearer ACCESS_TOKEN
```

In Swagger, select **Authorize** and enter the JWT access token. The Swagger security scheme adds the `Bearer` prefix automatically.

### Refresh an expired access token

```http
POST /api/auth/refresh
Content-Type: application/json

{
  "accessToken": "EXPIRED_ACCESS_TOKEN",
  "refreshToken": "REFRESH_TOKEN"
}
```

Refresh tokens are valid for seven days and become unusable after a successful refresh. The API returns a new access-token and refresh-token pair.

## Roles and Permissions

| Capability | Admin | Doctor | Patient |
| --- | :---: | :---: | :---: |
| List all patients | Yes | Yes | No |
| View a patient | Yes | Yes | Own profile only |
| Create a patient record | Yes | No | No |
| Update a patient record | Yes | Yes | No |
| Delete a patient record | Yes | No | No |
| View vitals, medications, and appointments | Yes | Yes | Own records only |
| Create vital signs | Yes | Yes | Own records only |
| Update or delete vital signs | Yes | Yes | No |
| Create, update, or delete medications | Yes | Yes | No |
| Create, update, or delete appointments | Yes | Yes | No |

## API Endpoints

### Authentication

| Method | Endpoint | Access | Description |
| --- | --- | --- | --- |
| `POST` | `/api/auth/register` | Public | Register an Identity user with an existing role |
| `POST` | `/api/auth/login` | Public | Authenticate and receive a token pair |
| `POST` | `/api/auth/refresh` | Public | Rotate an expired access token and refresh token |

### Patients

| Method | Endpoint | Access | Description |
| --- | --- | --- | --- |
| `GET` | `/api/patients` | Admin, Doctor | List all patients |
| `GET` | `/api/patients/{id}` | Authenticated | Retrieve a patient; patients are limited to their own profile |
| `POST` | `/api/patients` | Admin | Create a patient record |
| `PUT` | `/api/patients/{id}` | Admin, Doctor | Update a patient record |
| `DELETE` | `/api/patients/{id}` | Admin | Delete a patient record |

### Vital Signs

| Method | Endpoint | Access | Description |
| --- | --- | --- | --- |
| `GET` | `/api/patients/{patientId}/vitals` | Authenticated | List a patient's vital signs |
| `POST` | `/api/patients/{patientId}/vitals` | Admin, Doctor, owning Patient | Record vital signs |
| `GET` | `/api/vitals/{id}` | Authenticated | Retrieve one vital-sign record |
| `PUT` | `/api/vitals/{id}` | Admin, Doctor | Update a vital-sign record |
| `DELETE` | `/api/vitals/{id}` | Admin, Doctor | Delete a vital-sign record |

### Medications

| Method | Endpoint | Access | Description |
| --- | --- | --- | --- |
| `GET` | `/api/patients/{patientId}/medications` | Authenticated | List a patient's medications |
| `POST` | `/api/patients/{patientId}/medications` | Admin, Doctor | Add a medication |
| `GET` | `/api/medications/{id}` | Authenticated | Retrieve one medication |
| `PUT` | `/api/medications/{id}` | Admin, Doctor | Update a medication |
| `DELETE` | `/api/medications/{id}` | Admin, Doctor | Delete a medication |

### Appointments

| Method | Endpoint | Access | Description |
| --- | --- | --- | --- |
| `GET` | `/api/patients/{patientId}/appointments` | Authenticated | List a patient's appointments |
| `POST` | `/api/patients/{patientId}/appointments` | Admin, Doctor | Schedule an appointment |
| `GET` | `/api/appointments/{id}` | Authenticated | Retrieve one appointment |
| `PUT` | `/api/appointments/{id}` | Admin, Doctor | Update an appointment |
| `DELETE` | `/api/appointments/{id}` | Admin, Doctor | Delete an appointment |

For authenticated read endpoints, users in the `Patient` role can access only records connected to their own patient profile. Unauthorized ownership attempts return `403 Forbidden`.

## Validation Rules

The API automatically validates incoming requests. Examples include:

- Heart rate: 30–250 bpm
- Oxygen saturation: 50–100%
- Systolic blood pressure: 70–220 mmHg
- Diastolic blood pressure: 40–130 mmHg
- Appointment dates must be in the future
- Appointment status must be `Scheduled`, `Completed`, or `Cancelled`
- Medication end dates must be later than their start dates
- Patient contact numbers must contain 10–15 digits, with an optional leading `+`

Invalid requests receive an automatic `400 Bad Request` response containing validation details.

## Global Error Handling

Unhandled exceptions are caught by `GlobalExceptionMiddleware` and returned as an RFC-style `ProblemDetails` response with HTTP status `500`. The response contains a safe generic message and a trace ID, while the complete exception is logged on the server with the HTTP method and request path.

In `Development` and `Testing` only, the following diagnostic endpoint deliberately throws an exception so the middleware can be verified:

```http
GET /api/diagnostics/unhandled-error
```

The diagnostic endpoint returns `404 Not Found` in other environments and is hidden from Swagger.

## Rate Limiting

The API defines two fixed-window policies:

- `GeneralPolicy`: 30 requests per minute, with a queue of 2 requests
- `StrictLoginPolicy`: 5 login attempts per minute with no queue

Rejected requests receive `429 Too Many Requests`.

## Security Notes

- Use a strong JWT key stored outside source control.
- Restrict CORS to trusted frontend origins.
- Replace all seeded credentials before production deployment.
- Serve production traffic exclusively over HTTPS.
- Configure HSTS only after confirming that every production subdomain supports HTTPS.
- Restrict public user registration or role selection before production use. The current registration contract accepts a requested existing role.
- Consider revoking outstanding refresh tokens after password changes or account compromise.

## Build

```powershell
dotnet build
```

## License

No license file is currently included. Add a license before distributing or accepting external contributions.
