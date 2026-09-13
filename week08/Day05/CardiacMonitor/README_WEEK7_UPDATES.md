# Cardiac Monitor API — Week 7 Updates

This document describes the Week 7 care-team, Doctor scheduling, medical-alert, security, migration, testing, and Postman improvements.

## What Was Added

Week 7 is implemented as one connected workflow:

- A seeded `Nurse` Identity role.
- `NurseProfile` and `PatientCareAssignment` domain entities.
- `DoctorProfile` for professional Doctor data.
- `DoctorAvailability` for reusable weekly schedules.
- `MedicalAlert` for an auditable alert lifecycle.
- Admin-only staff account creation.
- Nurse access limited to actively assigned patients.
- Appointment validation against both the Doctor role and availability.
- Automatic alert generation from abnormal vital-sign readings.
- Alert acknowledgement and resolution with responsible user IDs and UTC times.
- Secure public Patient registration with no client-selected role.
- Domain claims in JWTs and centralized patient-resource authorization.
- Correlation-ID/request-timing middleware.
- Unit tests, integration tests, a reviewed migration, and a runnable Postman flow.

The core authorization principle is:

> A role defines what a user may do; ownership, professional profile, schedule, and active assignment define where and when the operation is allowed.

## Domain Design

```text
AspNetUsers 1 ───── 0..1 DoctorProfile 1 ───── * DoctorAvailability
     │
     └───────────── 0..1 NurseProfile  1 ───── * PatientCareAssignment * ───── 1 Patient
                                                                            │
                                                                            ├──── * VitalSign
                                                                            │        │
                                                                            │        └──── 0..1 MedicalAlert
                                                                            └──── * MedicalAlert
```

### Why `DoctorProfile` is separate from `AspNetUsers`

`AspNetUsers` remains responsible for authentication data such as email, password hash, security stamp, and lockout state. `DoctorProfile` holds domain data:

| Field | Meaning |
| --- | --- |
| `Id` | Numeric domain identifier. |
| `UserId` | Unique link to the Doctor's Identity account. |
| `FullName` | Professional display name. |
| `LicenseNumber` | Unique medical license. |
| `Specialty` | Medical specialty. |
| `Department` | Hospital department. |
| `IsActive` | Whether the profile may receive new appointments. |

```csharp
builder.HasOne(doctor => doctor.User)
    .WithOne()
    .HasForeignKey<DoctorProfile>(doctor => doctor.UserId)
    .OnDelete(DeleteBehavior.Cascade);
```

`Appointments.DoctorId` deliberately remains a string foreign key to `AspNetUsers.Id`. This preserves all existing appointments and IDs. The profile extends the existing Doctor identity instead of replacing it.

### Weekly Doctor availability

Each `DoctorAvailability` row stores `DoctorProfileId`, `DayOfWeek`, `StartTime`, `EndTime`, and `IsActive`. The database rejects invalid time ranges and duplicate slots; the service rejects overlaps.

```csharp
var overlappingSlotExists = await _context.DoctorAvailabilitySlots.AnyAsync(slot =>
    slot.DoctorProfileId == doctorProfileId &&
    slot.DayOfWeek == request.DayOfWeek &&
    slot.IsActive &&
    slot.StartTime < request.EndTime &&
    request.StartTime < slot.EndTime);
```

Appointment timestamps and schedule comparisons use UTC. The end time is exclusive: a `09:00–12:00` slot accepts `11:59`, not `12:00`.

### Nurse care assignments

`NurseProfile` separates professional data from authentication data. `PatientCareAssignment` represents the many-to-many care relationship and records who assigned the Nurse, when it started, when it ended, and optional notes.

Ending an assignment is a soft end:

```csharp
assignment.IsActive = false;
assignment.EndedAt = DateTime.UtcNow;
```

This removes access immediately while preserving audit history.

### Medical alerts

`MedicalAlert` stores:

- Patient and optional source vital-sign IDs.
- Severity: `Medium`, `High`, or `Critical`.
- Status: `Open`, `Acknowledged`, or `Resolved`.
- Human-readable message.
- Creation, acknowledgement, and resolution audit fields.

One vital-sign reading can have at most one generated alert. Deleting a vital-sign reading sets `VitalSignId` to null so the alert audit record remains. A patient with alert history is protected from accidental cascade deletion.

## Automatic Alert Rules

When a vital sign is created or updated, `VitalSignService` evaluates all four measurements and uses the highest triggered severity:

| Severity | Heart rate | Oxygen saturation | Systolic BP | Diastolic BP |
| --- | --- | --- | --- | --- |
| Critical | `< 40` or `> 180` | `< 85` | `< 80` or `> 200` | `< 50` or `> 120` |
| High | `< 50` or `> 140` | `< 90` | `< 90` or `> 180` | `< 60` or `> 110` |
| Medium | `< 60` or `> 100` | `< 95` | `< 100` or `> 140` | `< 65` or `> 90` |

```csharp
// Creates, reopens, updates, or automatically resolves the alert for a reading.
private void SynchronizeMedicalAlert(VitalSign vital)
{
    var evaluation = EvaluateVitalSign(vital);
    // The alert is created/reopened for abnormal values or resolved for normal values.
}
```

These thresholds are demonstration rules for the internship project. They are not clinical guidance, diagnosis, or a substitute for rules approved by medical professionals.

## Staff and Scheduling Endpoints

| Method | Route | Roles | Purpose |
| --- | --- | --- | --- |
| `POST` | `/api/staff/nurses` | Admin | Atomically create Nurse Identity, role, and profile. |
| `GET` | `/api/staff/nurses` | Admin, Doctor | List Nurse profiles. |
| `POST` | `/api/staff/doctors` | Admin | Atomically create Doctor Identity, role, and profile. |
| `GET` | `/api/staff/doctors` | Admin, Doctor, Nurse | List Doctor profiles. |
| `POST` | `/api/doctors/{doctorProfileId}/availability` | Admin, owning Doctor | Add a non-overlapping weekly slot. |
| `GET` | `/api/doctors/{doctorProfileId}/availability` | Authenticated care roles/Patient | List active schedule slots. |
| `DELETE` | `/api/doctor-availability/{id}` | Admin, owning Doctor | Deactivate a slot without deleting history. |
| `POST` | `/api/patients/{patientId}/care-assignments` | Admin, Doctor | Assign a Nurse to a Patient. |
| `GET` | `/api/patients/{patientId}/care-assignments` | Admin, Doctor | Read assignment history. |
| `GET` | `/api/nurses/me/patients` | Nurse | Return active assigned patients only. |
| `DELETE` | `/api/care-assignments/{id}` | Admin, Doctor | End an active assignment. |

Example availability request:

```json
{
  "dayOfWeek": "Monday",
  "startTime": "09:00:00",
  "endTime": "12:00:00"
}
```

`JsonStringEnumConverter` makes `"Monday"` readable in API requests and responses.

## Alert Endpoints

| Method | Route | Roles | Rule |
| --- | --- | --- | --- |
| `GET` | `/api/patients/{patientId}/alerts` | Admin, Doctor, assigned Nurse, owning Patient | Patient-resource access is enforced. |
| `GET` | `/api/alerts/{id}` | Admin, Doctor, assigned Nurse, owning Patient | Patient-resource access is enforced. |
| `PATCH` | `/api/alerts/{id}/acknowledge` | Admin, Doctor, assigned Nurse | Only an `Open` alert may be acknowledged. |
| `PATCH` | `/api/alerts/{id}/resolve` | Admin, Doctor | Resolves an active alert and records the user. |

## Secure Registration and JWT Claims

Public registration always creates a Patient. The request no longer accepts a role, preventing privilege escalation. Identity user creation, Patient role assignment, and Patient profile creation share one transaction.

Staff identities are created only through Admin routes. Login tokens may contain:

- `patient_id`
- `nurse_profile_id`
- `doctor_profile_id`

Claims make the domain identity explicit, but Nurse assignment is still checked in the database because it can end before a token expires.

```csharp
// Checks role, ownership, or active nurse assignment for patient access.
public async Task<bool> CanAccessPatientAsync(
    ClaimsPrincipal user,
    int patientId)
```

## Authorization Matrix

| Operation | Admin | Doctor | Nurse | Patient |
| --- | :---: | :---: | :---: | :---: |
| Create Doctor/Nurse account | Yes | No | No | No |
| Manage own Doctor schedule | Yes | Own profile | No | No |
| Create appointment | Yes | Yes | No | No |
| Book outside Doctor schedule | No | No | No | No |
| Read patient resource | Yes | Yes | Assigned only | Own only |
| Record vital signs | Yes | Yes | Assigned only | Own only |
| Acknowledge alert | Yes | Yes | Assigned only | No |
| Resolve alert | Yes | Yes | No | No |
| Create/end Nurse assignment | Yes | Yes | No | No |

`401 Unauthorized` means authentication is missing or invalid. `403 Forbidden` means authentication succeeded but the role/ownership/assignment rule rejected the operation.

## Migration and Existing Data Safety

The reviewed migration is:

```text
20260903203751_Week7CareTeamSchedulingAndAlerts
```

It creates `NurseProfiles`, `PatientCareAssignments`, `DoctorProfiles`, `DoctorAvailabilitySlots`, and `MedicalAlerts`; seeds the `Nurse` role; and adds foreign keys, indexes, and check constraints.

It does not renumber, delete, or edit existing Patient, VitalSign, Medication, or Appointment rows. Existing Patient IDs such as 20 remain valid. Existing users in the Doctor role are automatically backfilled into `DoctorProfiles` with a deterministic `LEGACY-...` license value. Review and replace those placeholder professional details later through a controlled administration process—not by editing primary keys.

No availability is invented for existing Doctors. Add real slots after migration; until then, new appointments for that Doctor are rejected. Existing appointment rows remain untouched.

The migration was generated and reviewed but intentionally not applied to the real database. Before applying it:

```powershell
# 1. Back up the real database.
# 2. Review the SQL script.
dotnet ef migrations script --project CardiacMonitor.csproj

# 3. Apply only after review.
dotnet ef database update --project CardiacMonitor.csproj
```

## Middleware and Error Responses

`RequestCorrelationMiddleware` accepts or generates `X-Correlation-ID`, places it in `HttpContext.TraceIdentifier`, returns it in the response header, and logs elapsed time. ProblemDetails and server logs can therefore be connected with the same identifier.

```csharp
// Adds a correlation ID and records the total request execution time.
public async Task InvokeAsync(HttpContext context)
```

## Verification

The verified suite contains:

- 32 unit tests.
- 18 integration tests.
- 50 tests in total.

Coverage includes Doctor role enforcement, overlapping schedule rejection, inside/outside schedule booking, automatic Critical alerts, alert audit transitions, secure Patient registration, Nurse assignment access, deliberate `403` cases, validation, and correlation IDs.

```powershell
dotnet build CardiacMonitor.slnx --no-restore
dotnet test CardiacMonitor.slnx --no-build --no-restore
dotnet ef migrations has-pending-model-changes --project CardiacMonitor.csproj --no-build
```

## Postman

Import `Cardiac Patient Monitoring API.postman_collection.json` and configure:

- `baseUrl`
- `accessToken` with an Admin JWT
- `patientId` with an existing patient ID

The collection includes two ordered Week 7 folders:

1. `Week 7 - Nurse Care Team`
2. `Week 7 - Doctor Scheduling and Medical Alerts`

Scripts save staff IDs, tokens, the calculated next-day appointment time, availability ID, vital-sign ID, and alert ID. Use fresh emails/license numbers when repeating the demo.

## Method Comment Convention

Every newly written method has a short English `//` comment explaining its responsibility:

```csharp
// Checks whether a Doctor identity is working at a requested UTC date and time.
public async Task<bool> IsDoctorAvailableAsync(...)
```

## Final Result

The API now demonstrates a realistic authorization and monitoring story: professional staff profiles are separated from Identity, Nurses are scoped to assigned patients, Doctors have enforceable weekly schedules, appointments cannot bypass those schedules, abnormal measurements create traceable alerts, and every sensitive transition is verified by automated tests and Postman requests.
