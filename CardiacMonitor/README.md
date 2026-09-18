# Cardiac Monitor API

[![CardiacMonitor CI](https://github.com/mohammadimad/Mohammad-Abdelfattah-BinX-Backend-Internship/actions/workflows/ci.yml/badge.svg?branch=main&event=push)](https://github.com/mohammadimad/Mohammad-Abdelfattah-BinX-Backend-Internship/actions/workflows/ci.yml)

A .NET 8 REST API for cardiac-care workflows: Patient profiles, vital signs, medications, appointments, Nurse care assignments, Doctor availability, and medical alerts. Identity and JWT authentication protect the API; role and Patient-resource checks control access.

## Technology Stack

| Area | Technology |
| --- | --- |
| Framework and language | ASP.NET Core 8, C# |
| Database and ORM | SQL Server, Entity Framework Core 8.0.11 |
| Identity | ASP.NET Core Identity, JWT Bearer, single-use refresh-token rotation |
| Validation | FluentValidation.AspNetCore 11.3.1 |
| Cache | Redis via IDistributedCache and StackExchange.Redis |
| Documentation | Swashbuckle.AspNetCore 6.6.2, XML comments, Swagger/OpenAPI |
| Tests | xUnit, Moq, WebApplicationFactory, SQLite and EF Core InMemory |

## Prerequisites

- .NET 8 SDK, Git, and a running SQL Server instance.
- Redis on localhost:6379, or a reachable managed Redis instance.
- An EF Core CLI tool matching this project's EF Core version.
- A trusted HTTPS development certificate.
- Postman is optional; Swagger can send requests without it.

The automated integration suite uses SQLite and distributed memory caching. It does not require SQL Server or Redis, and does not replace testing those production providers.

## Clone and Restore

```powershell
git clone https://github.com/mohammadimad/Mohammad-Abdelfattah-BinX-Backend-Internship.git
cd Mohammad-Abdelfattah-BinX-Backend-Internship/CardiacMonitor
dotnet restore CardiacMonitor.slnx
dotnet tool install --global dotnet-ef --version 8.0.11
dotnet dev-certs https --trust
```

If dotnet-ef is already installed, check `dotnet ef --version` and update it to a compatible 8.x version rather than reinstalling blindly.

## Configure Local Secrets

Run from the CardiacMonitor directory. Replace the example values with your actual local configuration; never use a published example signing key in a shared or production environment.

```powershell
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:CardiacMonitorConnection" "Server=localhost;Database=CardiacMonitorDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
dotnet user-secrets set "ConnectionStrings:Redis" "localhost:6379"
dotnet user-secrets set "Jwt:Key" "REPLACE_WITH_A_RANDOM_SECRET_OF_AT_LEAST_32_BYTES"
dotnet user-secrets set "Jwt:Issuer" "CardiacMonitorAPI"
dotnet user-secrets set "Jwt:Audience" "CardiacMonitorAPI"
dotnet user-secrets set "Jwt:DurationInMinutes" "60"
```

Windows integrated authentication requires your Windows account to have SQL Server database permissions. For SQL authentication, use a secret connection string containing your own User ID and Password. The local TrustServerCertificate option is not a production TLS policy.

For a disposable local Redis instance, if Docker is installed:

```powershell
docker run --name cardiacmonitor-redis -p 127.0.0.1:6379:6379 -d redis:7-alpine
```

If that container already exists, use `docker start cardiacmonitor-redis`. Do not expose an unauthenticated Redis port publicly.

### Environment variable equivalents

.NET configuration uses double underscores for nested environment keys. User Secrets apply to Development; production must supply values through environment variables or a secret manager.

| Variable | Purpose | Example or requirement |
| --- | --- | --- |
| ASPNETCORE_ENVIRONMENT | Runtime environment | Development locally; Production when deployed |
| ConnectionStrings__CardiacMonitorConnection | SQL Server connection | Your server, database, and credentials |
| ConnectionStrings__Redis | Redis endpoint | localhost:6379 locally; authentication/TLS as required remotely |
| Jwt__Key | HMAC signing secret | Random secret of at least 32 bytes; never commit it |
| Jwt__Issuer | Accepted issuer | CardiacMonitorAPI |
| Jwt__Audience | Accepted audience | CardiacMonitorAPI |
| Jwt__DurationInMinutes | Access-token lifetime | 60 |
| ASPNETCORE_URLS | Optional hosting bindings | Configure when not using a launch profile |

Keep signing and database secrets out of Git and Postman exports. Rotate the existing checked-in development credentials before any shared deployment. The named CORS policy currently permits `http://localhost:5142`; edit its origin in Program.cs if your frontend uses a different address.

## Apply Database Migrations

Ensure SQL Server is running and configured before applying the existing migrations:

```powershell
dotnet ef database update --project CardiacMonitor.csproj
```

Use the migration history already in `Data/Migrations/`; do not create a new migration merely to start the project. Migrations include reference roles, demonstration clinical data, and a development Doctor identity. The roles are Admin, Doctor, Patient, and Nurse. Development seed accounts are not production bootstrap credentials.

## Run and Open the API Documentation

```powershell
dotnet run --project CardiacMonitor.csproj --launch-profile https
```

- [Swagger UI](https://localhost:7142/swagger)
- [OpenAPI JSON](https://localhost:7142/swagger/v1/swagger.json)
- HTTP redirect address: `http://localhost:5142`

Swagger is available only in Development. XML documentation is generated during build and loaded by Swashbuckle. In Swagger's Authorize dialog, enter the raw access token; the HTTP bearer scheme supplies the Bearer prefix.

These URLs assume the checked-in HTTPS launch profile. If you change its ports or host bindings, use the address printed by the application.

## Authentication and Initial Roles

Public registration creates a **Patient** identity, assigns the Patient role, and creates its domain profile. It does not accept an Admin, Doctor, or Nurse role selection.

```http
POST /api/auth/register
Content-Type: application/json

{
  "email": "patient@example.com",
  "password": "ExamplePassword1!",
  "firstName": "Lina",
  "lastName": "Nasser",
  "dateOfBirth": "1995-06-15",
  "gender": "Female",
  "contactNumber": "+970599111111"
}
```

Then send `POST /api/auth/login` with the registered email and password. It returns token, refreshToken, and message. Protected requests require:

```http
Authorization: Bearer YOUR_ACCESS_TOKEN
```

Refresh with `POST /api/auth/refresh`, sending the **expired** access token and its matching unused refresh token. A still-valid access token is rejected. Refresh tokens expire after seven days and are consumed on successful rotation. Store the replacement pair.

### Bootstrap an Admin for disposable local development

The repository does not automatically provision an Admin password. Register a separate local account, such as `local-admin@example.com`, through Swagger. A trusted database operator can explicitly grant it the Admin role in the disposable local database:

```sql
USE CardiacMonitorDb;
DECLARE @Email nvarchar(256) = N'local-admin@example.com';

IF NOT EXISTS (SELECT 1 FROM AspNetUsers WHERE Email = @Email)
    THROW 50000, 'Register the local account first.', 1;

INSERT INTO AspNetUserRoles (UserId, RoleId)
SELECT u.Id, r.Id
FROM AspNetUsers u
CROSS JOIN AspNetRoles r
WHERE u.Email = @Email AND r.Name = N'Admin'
  AND NOT EXISTS (
      SELECT 1 FROM AspNetUserRoles ur
      WHERE ur.UserId = u.Id AND ur.RoleId = r.Id
  );
```

Log in again after granting the role so the new JWT contains it. This is an explicit local bootstrap step, not a public role-escalation endpoint or a production provisioning process. Admins then create Doctor and Nurse identities through `/api/staff/doctors` and `/api/staff/nurses`.

## Endpoint Groups and Access

The complete 39-operation inventory is in [REAMDE01.md](REAMDE01.md); the current contract is available in Swagger and the final Postman collection.

| Group | Routes and access |
| --- | --- |
| Authentication | Register, login, refresh; public with credential/token validation |
| Patients | Admin/Doctor list; Admin creates/deletes; Admin/Doctor updates; authorized resource reads |
| Vital signs | Authorized resource reads and recording; Admin/Doctor updates/deletes |
| Medications | Authorized resource reads; Admin/Doctor writes |
| Appointments | Authorized resource reads; Admin/Doctor booking and changes |
| Staff | Admin creates identities; role-protected staff lists |
| Care assignments | Admin/Doctor assignment management; Nurse reads own active Patient list |
| Doctor availability | Role-protected reads; Admin or owning Doctor manages slots |
| Medical alerts | Authorized resource reads; Admin/Doctor/Nurse acknowledges; Admin/Doctor resolves |

Patients can read only their own clinical resources; Nurses need active assignments for Patient-resource access. Admins and Doctors have wider clinical access. A Doctor's schedule-management ownership is checked separately from its role.

Appointment Doctor IDs are **Identity user ID strings**. Availability routes use **numeric Doctor profile IDs**. These IDs are not interchangeable. Nurse assignments likewise use numeric Nurse profile IDs.

## Final Postman Collection

Import both files:

- [Final collection](postman/CardiacMonitor.Final.postman_collection.json)
- [Local environment](postman/CardiacMonitor.Local.postman_environment.json)

The older root-level collection is retained for historical reference; use the Final collection for Sprint 4. It contains all 39 operations in 10 folders, a strict expected-status test for every request, and illustrative saved responses. Every body-bearing endpoint has a request example; 204 responses intentionally have no JSON body.

### Run the documented workflow

1. Start the API, SQL Server, and Redis against a **disposable** database.
2. Select the imported environment and set baseUrl to the running HTTPS address.
3. Bootstrap a local Admin as described above, log in, and put its real token in the private adminToken environment value.
4. Run Authentication to create and log in a separate Patient. Scripts store patientToken, refreshToken, and patientId.
5. Run Patients, Staff, and Doctor Availability in order. Scripts capture created record IDs and the Doctor's Identity user ID.
6. Log in the newly created Nurse using nurseEmail and staffPassword; manually store its returned token in nurseToken.
7. Continue Care Assignments, Vital Signs, Medications, Appointments, and Medical Alerts. A critical reading creates the alert used by subsequent requests.
8. Run cleanup last. It deletes created clinical records and the standalone Patient record, ends the assignment, and disables availability. It does not delete the registered Patient, Doctor, or Nurse identities.
9. Exclude Refresh Expired Patient Token from the initial runner pass. Run it manually after patientToken expires; it requires expiry and rotates the stored pair.

Use a runner delay of **2500 ms** to stay below the general 30-request-per-minute limit. Login has its own five-attempts-per-minute limit. Do not disable rate limiting merely to run the collection.

The environment generates synthetic unique emails and license suffixes from runId and UTC date variables. For another full run, clear runId and the three email values first. IDs are captured from responses, not assumed to be 1. Do not export an environment after filling real secrets unless you remove them.

The collection changes data. Its status scripts are runnable assertions, but their presence does **not** mean a live SQL Server/Redis Postman run has been completed.

## Errors and Validation

| Status | Meaning |
| --- | --- |
| 400 | Invalid input or a business-rule failure; includes field errors where applicable |
| 401 | Missing/invalid authentication, or invalid login credentials |
| 403 | Valid authentication but insufficient role/resource access |
| 404 | Requested record does not exist |
| 409 | An alert transition conflicts with its current state |
| 429 | Fixed-window rate limit exceeded |
| 500 | Safe ProblemDetails response for an unexpected error |

Validation examples include heart rate 30–250 bpm, oxygen saturation 50–100%, blood-pressure ranges, future appointment times, supported statuses, and contact-number formats. Request-specific constraints appear in validators. Errors use ProblemDetails or ValidationProblemDetails and include a trace ID.

## Build and Test

```powershell
dotnet build CardiacMonitor.slnx --no-restore
dotnet test CardiacMonitor.slnx --no-restore --verbosity minimal
```

Documentation tests verify XML summaries, the three Swagger example pairs, and exact operation parity between generated Swagger and the final Postman collection. See [REAMDE02.md](REAMDE02.md) for Day 2 verification and the peer walkthrough.

## Troubleshooting

- **SQL connection fails:** verify SQL Server is running, instance name and authentication are correct, and your account can create/update the target database.
- **dotnet ef is missing:** install the compatible CLI tool, open a new terminal if PATH changed, and run commands from CardiacMonitor.
- **Swagger is missing:** use the https launch profile and Development environment; rebuild to generate CardiacMonitor.xml.
- **HTTPS certificate is untrusted:** run dotnet dev-certs https --trust and retry the correct HTTPS port.
- **Availability requests fail:** ensure Redis is reachable and the numeric Doctor profile ID exists.
- **403 after granting a role:** log in again; an existing JWT does not acquire new role claims automatically.
- **Appointment creation returns 400:** check the Doctor Identity ID, future UTC timestamp, matching weekly slot, and booking conflicts.
- **Postman receives 429:** slow the runner and wait for the configured window to reset.
- **Refresh returns 400:** confirm the access token has expired and the original refresh token is unused and matches the access-token JTI.
- **Postman returns 404:** run creation steps first and check captured IDs and selected environment.

## Production Notes

Use managed secrets, verified TLS, trusted CORS origins, restricted Redis connectivity, and secure identity provisioning. Replace seeded credentials. Keep Swagger exposure an explicit deployment decision. SQL query-budget tests are not production latency benchmarks, and the SQLite test suite does not establish SQL Server execution-plan performance.

## License

No license file is included. Confirm usage and distribution rights before redistributing the project.
