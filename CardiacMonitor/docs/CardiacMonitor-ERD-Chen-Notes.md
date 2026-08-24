# مراجعة ERD بنمط Chen

## نتيجة مراجعة الرسم القديم

الرسم القديم صحيح في فكرته العامة، لكنه يحتاج التصحيحات التالية حتى يطابق الكود:

1. علاقة `AspNetUsers` مع `Patients` اختيارية من الجانبين وليست إلزامية `(1,1)`، لأن `Patients.UserId` يقبل `null` ومقيد كعلاقة one-to-one.
2. كل `VitalSign` يحتوي `PatientId` إلزاميًا ويرتبط بمريض واحد، بينما قد يمتلك المريض صفرًا أو عدة قراءات.
3. كل `Medication` يحتوي `PatientId` إلزاميًا، كما أن الخاصية `IsActive` كانت مفقودة في الرسم.
4. كل `Appointment` يحتوي مفتاحين خارجيين: `PatientId` و`DoctorId`.
5. علاقة المستخدم بالمواعيد تمثل المستخدم الذي يؤدي دور الطبيب، لذلك سميت `ASSIGNED AS DOCTOR` بدل الاسم العام `Relationship`.
6. المريض قد يمتلك صفرًا أو عدة مواعيد، لكن كل موعد يجب أن ينتمي إلى مريض واحد.
7. المستخدم قد يكون طبيبًا في صفر أو عدة مواعيد، لكن كل موعد يجب أن يرتبط بمستخدم طبيب واحد.

## العلاقات النهائية

- `AspNetUsers (0,1) — HAS PROFILE — (0,1) Patients`
- `Patients (0,N) — RECORDS — (1,1) VitalSigns`
- `Patients (0,N) — RECEIVES — (1,1) Medications`
- `Patients (0,N) — BOOKS — (1,1) Appointments`
- `AspNetUsers (0,N) — ASSIGNED AS DOCTOR — (1,1) Appointments`

تم حذف `AspNetRoles` و`AspNetUserRoles` و`RefreshTokens` من نسخة العرض حسب الطلب. وجودها الفعلي في قاعدة البيانات لا يتغير؛ الصورة فقط تتعمد إخفاءها لتبسيط العرض.
