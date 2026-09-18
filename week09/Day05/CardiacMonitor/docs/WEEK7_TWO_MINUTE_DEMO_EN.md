# Week 7 — Two-Minute Demo Script

## Before the Demo

- Run the API and confirm that it is available at `https://localhost:7142`.
- Log in as the Demo Admin:
  - Email: `demo.admin@cardiac.local`
  - Password: `DemoAdmin123!`
- Save the returned JWT in the Postman collection variable `accessToken`.
- Set `patientId` to an existing Patient ID.
- Open `Week 7 - Doctor Scheduling and Medical Alerts` in Postman.
- Run requests 1–4 before presenting:
  1. `Admin Creates Doctor`
  2. `Doctor Login`
  3. `Admin Adds Tomorrow Availability`
  4. `Admin Lists Doctor Availability`
- Do not run request 5 before the live demo, because that could create an appointment conflict.

## Opening Statement

> In Week 7, I extended the project with three connected features: a professional Doctor profile, enforceable weekly availability, and an auditable medical-alert workflow. I kept ASP.NET Identity responsible for authentication, while DoctorProfile stores domain information such as the medical license, specialty, department, and active status. I also kept the existing Appointment DoctorId relationship unchanged to protect existing data.

> The important point is that these are not isolated database tables. Doctor availability is applied as a business rule when an appointment is created, and an abnormal vital-sign reading automatically creates a medical alert that can be acknowledged and resolved with a complete audit trail.

## Live Postman Flow

### 1. Prove Doctor availability enforcement

Run `5 - Admin Creates Appointment Inside Availability`.

Point to `201 Created` and say:

> This appointment is at 10:00 UTC, inside the Doctor's 09:00–12:00 availability, so it is accepted.

Run `6 - Admin Tries Appointment Outside Availability`.

Point to `400 Bad Request` and say:

> The second appointment is at 14:00 UTC, outside the configured schedule, so the service rejects it. This proves that availability is an enforced business rule, not just stored information.

### 2. Prove automatic medical-alert creation

Run requests 7 and 8:

- `Admin Creates Critical Vital Sign`
- `Admin Gets Patient Alerts`

Point to `severity: Critical` and `status: Open`, then say:

> I did not create this alert manually. VitalSignService evaluated the reading, selected the highest severity, and created a MedicalAlert linked to both the Patient and the source VitalSign. These thresholds are demonstration rules, not medical diagnosis.

### 3. Prove the auditable lifecycle

Run requests 9 and 10:

- `Admin Acknowledges Alert`
- `Doctor Resolves Alert`

Say:

> Acknowledged means that a staff member has accepted responsibility for the alert. Resolved means that an authorized Doctor has closed it. The API records the responsible user ID and UTC timestamp for both transitions.

## Closing Statement

> In summary, Profile defines the Doctor's professional identity, Availability defines when appointments are allowed, and MedicalAlert converts a dangerous reading into a traceable workflow. The positive and rejection scenarios are also covered by a total of 50 automated tests.

## Presentation Reminders

- Do not apply migrations during the presentation.
- Do not type emails, tokens, or IDs live.
- Never show the JWT secret, password hash, or connection string.
- Focus on `201` inside the schedule, `400` outside it, and `Open → Acknowledged → Resolved`.
- Avoid explaining every property or source file.
- If time is short, skip request 9 and resolve the alert directly with the Doctor.
