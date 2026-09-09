

## العلاقات النهائية

- `AspNetUsers (0,1) — HAS PROFILE — (0,1) Patients`
- `Patients (0,N) — RECORDS — (1,1) VitalSigns`
- `Patients (0,N) — RECEIVES — (1,1) Medications`
- `Patients (0,N) — BOOKS — (1,1) Appointments`
- `AspNetUsers (0,N) — ASSIGNED AS DOCTOR — (1,1) Appointments`


