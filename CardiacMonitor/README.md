# نظام مراقبة مرضى القلب — Cardiac Monitor API

واجهة خلفية RESTful متكاملة لإدارة بيانات مرضى القلب، مبنية باستخدام **ASP.NET Core 8 Web API** و**SQL Server**. يتيح النظام إدارة ملفات المرضى، القياسات الحيوية، الأدوية، والمواعيد الطبية، مع تطبيق المصادقة بواسطة JWT، والصلاحيات حسب الدور، وحماية خصوصية المريض، والتحقق من المدخلات، وتحديد معدل الطلبات.

> النسخة الإنجليزية متوفرة في [README.en.md](README.en.md).

## جدول المحتويات

1. [فكرة المشروع](#فكرة-المشروع)
2. [المزايا الأساسية](#المزايا-الأساسية)
3. [التقنيات المستخدمة](#التقنيات-المستخدمة)
4. [كيف يعمل النظام؟](#كيف-يعمل-النظام)
5. [بنية المشروع](#بنية-المشروع)
6. [طبقات المشروع ومسؤولية كل طبقة](#طبقات-المشروع-ومسؤولية-كل-طبقة)
7. [نماذج البيانات والعلاقات](#نماذج-البيانات-والعلاقات)
8. [المصادقة والتفويض](#المصادقة-والتفويض)
9. [الأدوار والصلاحيات](#الأدوار-والصلاحيات)
10. [حماية ملكية بيانات المريض](#حماية-ملكية-بيانات-المريض)
11. [توثيق نقاط النهاية](#توثيق-نقاط-النهاية-api-endpoints)
12. [التحقق من المدخلات](#التحقق-من-المدخلات)
13. [Rate Limiting](#تحديد-معدل-الطلبات-rate-limiting)
14. [CORS وHTTPS وHSTS](#cors-وhttps-وhsts)
15. [إعداد وتشغيل المشروع](#إعداد-وتشغيل-المشروع)
16. [تجربة المشروع عمليًا](#تجربة-المشروع-عمليًا)
17. [رموز استجابة HTTP](#رموز-استجابة-http)
18. [ملاحظات أمنية مهمة](#ملاحظات-أمنية-مهمة)

## فكرة المشروع

المشروع يمثل الجزء الخلفي لنظام صحي لمتابعة مرضى القلب. تستطيع جهة طبية استخدامه لتنفيذ السيناريوهات التالية:

- يحتفظ المسؤول بملفات المرضى ويضيف أو يحذف المرضى.
- يطّلع الطبيب على بيانات المرضى ويحدّثها.
- يسجل الطبيب أو المريض قياسات حيوية مثل نبض القلب، نسبة الأكسجين وضغط الدم.
- يضيف الطبيب الأدوية ومواعيد تناولها ومدتها.
- يجدول الطبيب المواعيد الطبية ويتابع حالتها.
- يسجل المستخدم دخوله ويحصل على Access Token للوصول الآمن إلى البيانات.
- يستطيع المريض مشاهدة بياناته فقط، ولا يمكنه الاطلاع على بيانات مريض آخر.

المشروع لا يقتصر على عمليات CRUD، بل يطبق عدة مفاهيم مهمة في بناء Back-end حقيقي: فصل المسؤوليات، Dependency Injection، DTOs، التحقق من المدخلات، المصادقة، التفويض، حماية ملكية البيانات، العلاقات بين الجداول، Migrations، Rate Limiting، CORS وSwagger.

## المزايا الأساسية

- إدارة بيانات المرضى بالكامل.
- تسجيل وعرض وتحديث وحذف القياسات الحيوية.
- إدارة الأدوية وخطط العلاج.
- إدارة المواعيد الطبية وربطها بالمريض والطبيب.
- إنشاء المستخدمين وإسناد الأدوار إليهم.
- تسجيل الدخول بواسطة البريد الإلكتروني وكلمة المرور.
- إصدار JWT Access Token قصير العمر.
- إصدار Refresh Token صالح لمدة سبعة أيام.
- تدوير Refresh Token ومنع إعادة استخدامه.
- صلاحيات مختلفة لأدوار `Admin` و`Doctor` و`Patient`.
- Ownership Check لمنع المريض من قراءة بيانات مريض آخر.
- قواعد FluentValidation وإرجاع أخطاء واضحة للمدخلات غير الصحيحة.
- سياسة خاصة للحد من محاولات تسجيل الدخول.
- Swagger UI مع دعم زر Authorize لإدخال JWT.
- تحويل HTTP إلى HTTPS واستخدام HSTS في الإنتاج.
- Seed Data للأدوار وبعض البيانات التجريبية.

## التقنيات المستخدمة

| الجزء | التقنية | وظيفتها |
| --- | --- | --- |
| إطار العمل | ASP.NET Core 8 Web API | استقبال طلبات HTTP وبناء REST API |
| اللغة | C# | كتابة منطق المشروع |
| قاعدة البيانات | Microsoft SQL Server | تخزين بيانات النظام |
| ORM | Entity Framework Core 8 | التعامل مع قاعدة البيانات باستخدام كائنات C# وLINQ |
| إدارة المستخدمين | ASP.NET Core Identity | المستخدمون وكلمات المرور والأدوار |
| المصادقة | JWT Bearer | إثبات هوية المستخدم في الطلبات المحمية |
| تجديد الجلسة | Refresh Tokens | إصدار Access Token جديد دون تسجيل الدخول مجددًا |
| التحقق | FluentValidation | التحقق من صحة DTOs قبل دخولها إلى الخدمة |
| التوثيق | Swagger / OpenAPI | استكشاف وتجربة الـ API من المتصفح |
| الحماية | Rate Limiting | تقليل إساءة الاستخدام ومحاولات تسجيل الدخول المتكررة |
| إدارة الاعتمادات | Dependency Injection | حقن الخدمات بدل إنشائها يدويًا |

## كيف يعمل النظام؟

عندما يرسل العميل طلبًا، يمر الطلب داخل ASP.NET Core عبر سلسلة من Middleware بالترتيب الموجود في `Program.cs`:

```text
Client / Swagger / Frontend
           |
           v
Swagger Middleware (في بيئة Development)
           |
           v
HTTPS Redirection
           |
           v
CORS Policy
           |
           v
Rate Limiting
           |
           v
Authentication: قراءة JWT والتحقق منه
           |
           v
Authorization: فحص الدور والصلاحيات
           |
           v
Controller Endpoint
           |
           v
Service: منطق العمل
           |
           v
AppDbContext / Entity Framework Core
           |
           v
SQL Server
```

مثال: عند طلب `GET /api/patients/1/vitals` يحدث الآتي:

1. يستقبل الخادم الطلب.
2. تطبق سياسة CORS إذا جاء الطلب من متصفح يعمل على origin مختلف.
3. تحسب سياسة Rate Limiting عدد طلبات العميل.
4. تقرأ Authentication Middleware قيمة `Authorization: Bearer ...`.
5. تتحقق من توقيع JWT والمصدر والجمهور وتاريخ الانتهاء.
6. يتحقق `[Authorize]` من أن المستخدم مسجل الدخول.
7. إذا كان المستخدم مريضًا، يقارن Controller رقم مستخدمه بالمريض المطلوب.
8. يستدعي Controller خدمة `IVitalSignService`.
9. تستخدم الخدمة `AppDbContext` للاستعلام من SQL Server.
10. تحول الخدمة البيانات إلى Response DTO.
11. يعيد Controller استجابة `200 OK` بصيغة JSON.

## بنية المشروع

```text
CardiacMonitor/
├── Controllers/
│   ├── AuthControllerr.cs
│   ├── PatientsController.cs
│   ├── VitalSignsController.cs
│   ├── MedicationsController.cs
│   └── AppointmentsController.cs
├── Data/
│   ├── AppDbContext.cs
│   └── Migrations/
├── DTOs/
│   ├── AuthDtos.cs
│   ├── PatientDtos.cs
│   ├── VitalSignDtos.cs
│   ├── MedicationDtos.cs
│   └── AppointmentDtoscs.cs
├── Models/
│   ├── Patient.cs
│   ├── VitalSign.cs
│   ├── Medication.cs
│   ├── Appointment.cs
│   └── RefreshToken.cs
├── Services/
│   ├── Interfaces
│   └── Implementations
├── Validators/
├── Properties/
│   └── launchSettings.json
├── Program.cs
├── appsettings.json
└── CardiacMonitor.csproj
```

## طبقات المشروع ومسؤولية كل طبقة

### Models

تمثل الجداول التي سيُنشئها Entity Framework Core داخل قاعدة البيانات. مثال: `Patient` يمثل صفًا في جدول المرضى، ويحتوي على الخصائص والعلاقات الأساسية.

لا يُفضّل إرسال Models مباشرة إلى العميل؛ لأن Model يعبر عن شكل التخزين، وقد يحتوي مستقبلًا على خصائص داخلية لا يجب كشفها.

### DTOs

اختصار **Data Transfer Objects**، وهي العقود التي تحدد البيانات الداخلة إلى الـ API والخارجة منه.

يوجد عادة ثلاثة أنواع:

- `Create...Request`: الحقول اللازمة لإنشاء سجل.
- `Update...Request`: الحقول التي يمكن تعديلها.
- `...Response`: الشكل الآمن والمنظم الذي يعاد إلى العميل.

استخدام DTOs يفصل شكل قاعدة البيانات عن شكل الـ API، ويمنع مشكلة Over-posting، ويجعل التحقق والتطوير أكثر وضوحًا.

### Validators

تحتوي على قواعد FluentValidation الخاصة بطلبات الإنشاء. عند وصول Body غير صالح، يوقف ASP.NET Core تنفيذ الطلب ويعيد `400 Bad Request` مع تفاصيل الحقول غير الصحيحة.

### Services

تحتوي على منطق العمل والوصول إلى البيانات. لكل خدمة Interface مثل `IPatientService` وتنفيذ مثل `PatientService`.

فوائد هذا الأسلوب:

- يبقى Controller صغيرًا ومسؤولًا عن HTTP فقط.
- يمكن تبديل التنفيذ دون تغيير Controller.
- يسهل عمل Unit Tests باستخدام Mock للـ Interface.
- تقل الارتباطات المباشرة بين أجزاء المشروع.

تسجل الخدمات في Dependency Injection داخل `Program.cs` باستخدام `AddScoped`. معنى Scoped أن ASP.NET Core ينشئ نسخة واحدة من الخدمة لكل HTTP Request ثم يتخلص منها بعد انتهاء الطلب.

### Controllers

تربط عناوين HTTP بالخدمات. مسؤوليات Controller تشمل:

- استقبال route parameters وrequest body.
- تطبيق `[Authorize]` وتحديد الأدوار المسموحة.
- إجراء Ownership Check عند الحاجة.
- استدعاء Service المناسبة.
- تحويل النتيجة إلى Status Code مثل `200` أو `201` أو `404`.

### AppDbContext

هو بوابة Entity Framework Core إلى SQL Server، ويرث من `IdentityDbContext<IdentityUser>` حتى يجمع جداول المشروع مع جداول Identity في قاعدة بيانات واحدة.

يعرض الجداول التالية:

```csharp
DbSet<Patient> Patients
DbSet<VitalSign> VitalSigns
DbSet<Medication> Medications
DbSet<Appointment> Appointments
DbSet<RefreshToken> RefreshTokens
```

كما يحدد العلاقات، سلوك الحذف، دقة الأرقام العشرية، الأدوار، والبيانات الأولية داخل `OnModelCreating`.

### Program.cs

هو نقطة بداية التطبيق، وله قسمان أساسيان:

1. **تسجيل الخدمات:** قاعدة البيانات، Identity، JWT، Swagger، CORS، Rate Limiting، FluentValidation وخدمات المشروع.
2. **بناء HTTP Pipeline:** ترتيب Middleware ثم ربط Controllers وتشغيل الخادم بواسطة `app.Run()`.

ترتيب Middleware مهم؛ فمثلًا يجب تنفيذ `UseAuthentication()` قبل `UseAuthorization()` حتى يعرف النظام هوية المستخدم قبل فحص صلاحياته.

## نماذج البيانات والعلاقات

### Patient

يمثل المريض ويحتوي على:

- `Id`: المفتاح الأساسي.
- `UserId`: رابط اختياري مع مستخدم Identity.
- `FirstName` و`LastName`.
- `DateOfBirth`.
- `Gender`.
- `ContactNumber`.
- مجموعة `VitalSigns`.

العلاقة بين `Patient` و`IdentityUser` هي واحد إلى واحد. كون `UserId` اختياريًا يسمح بوجود سجل طبي لمريض لم يحصل بعد على حساب دخول.

### VitalSign

يمثل قراءة طبية في وقت معين:

- نبض القلب `HeartRate`.
- تشبع الأكسجين `OxygenSaturation`.
- الضغط الانقباضي `SystolicBP`.
- الضغط الانبساطي `DiastolicBP`.
- وقت التسجيل `RecordedAt`.
- مفتاح المريض `PatientId`.

العلاقة: مريض واحد يمتلك قراءات عديدة. حذف المريض يحذف قراءاته تلقائيًا بواسطة Cascade Delete.

### Medication

يمثل دواءً موصوفًا للمريض:

- اسم الدواء والجرعة والتكرار.
- تاريخ بداية العلاج ونهايته.
- `IsActive` لتحديد استمرار العلاج.
- `PatientId` لربطه بالمريض.

العلاقة: مريض واحد لديه أدوية عديدة، مع Cascade Delete عند حذف المريض.

### Appointment

يمثل موعدًا بين مريض وطبيب:

- `PatientId`: المريض.
- `DoctorId`: مستخدم Identity الذي يمثل الطبيب.
- `AppointmentDate`: موعد الزيارة.
- `Status`: إحدى قيم `Scheduled` أو `Completed` أو `Cancelled`.
- `Notes`: ملاحظات اختيارية.

الحذف من جهة المريض Cascade، أما حذف الطبيب فهو Restrict حتى لا تختفي أو تنكسر المواعيد المرتبطة به دون معالجة واضحة.

### RefreshToken

يخزن بيانات رمز التجديد:

- القيمة العشوائية للرمز.
- `JwtId` لربطه بالـ Access Token الأصلي.
- `IsUsed` لمنع استخدامه مرتين.
- `IsRevoked` لإبطال الرمز إداريًا.
- تاريخ الإنشاء والانتهاء.
- المستخدم صاحب الرمز.

## المصادقة والتفويض

هناك فرق مهم بين المفهومين:

- **Authentication — المصادقة:** من هو المستخدم؟ تتم بواسطة البريد وكلمة المرور ثم JWT.
- **Authorization — التفويض:** ماذا يُسمح لهذا المستخدم أن يفعل؟ يتم بواسطة الأدوار وOwnership Check.

### تسجيل مستخدم

يرسل العميل:

```http
POST /api/auth/register
Content-Type: application/json

{
  "email": "patient@example.com",
  "password": "StrongPassword123!",
  "role": "Patient"
}
```

تقوم `AuthService` بالخطوات التالية:

1. التأكد من عدم تسجيل البريد مسبقًا.
2. إنشاء `IdentityUser`؛ وتقوم Identity بتشفير كلمة المرور Hash بدل تخزينها كنص.
3. التأكد من وجود الدور المطلوب.
4. إسناد الدور إلى المستخدم.

### تسجيل الدخول وإصدار الرموز

```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "patient@example.com",
  "password": "StrongPassword123!"
}
```

بعد نجاح كلمة المرور، ينشئ النظام Access Token يحتوي على Claims مهمة:

- `sub`: معرف المستخدم.
- `jti`: معرف فريد للرمز.
- البريد الإلكتروني.
- `NameIdentifier`: معرف المستخدم المستخدم في Ownership Check.
- Role Claims: أدوار المستخدم.

يوقّع الرمز باستخدام HMAC SHA-256 والمفتاح الموجود في إعدادات JWT. يتحقق الخادم عند كل طلب من:

- صحة التوقيع.
- `Issuer`.
- `Audience`.
- مدة الصلاحية.

### استخدام Access Token

يوضع الرمز في Header لكل طلب محمي:

```http
Authorization: Bearer ACCESS_TOKEN
```

### لماذا نحتاج Refresh Token؟

Access Token قصير العمر لتقليل الضرر عند سرقته. بدل مطالبة المستخدم بكلمة المرور بعد انتهائه، يستخدم Refresh Token طويل نسبيًا لإصدار زوج جديد.

```http
POST /api/auth/refresh
Content-Type: application/json

{
  "accessToken": "EXPIRED_ACCESS_TOKEN",
  "refreshToken": "REFRESH_TOKEN"
}
```

تفحص الخدمة ما يلي:

1. صحة توقيع Access Token حتى إن كان منتهي الصلاحية.
2. أن خوارزمية التوقيع هي HMAC SHA-256.
3. أن Access Token انتهى فعلًا؛ فلا حاجة لتجديد رمز ما زال صالحًا.
4. وجود Refresh Token في قاعدة البيانات.
5. أنه لم يُستخدم ولم يُلغَ ولم تنتهِ صلاحيته.
6. تطابق `JwtId` مع `jti` الموجود في Access Token.
7. تعليم Refresh Token القديم بأنه مستخدم.
8. إصدار Access Token وRefresh Token جديدين.

هذه الآلية تسمى **Refresh Token Rotation** وتمنع إعادة استخدام الرمز القديم بعد نجاح عملية التجديد.

## الأدوار والصلاحيات

| العملية | Admin | Doctor | Patient |
| --- | :---: | :---: | :---: |
| عرض جميع المرضى | نعم | نعم | لا |
| عرض ملف مريض | نعم | نعم | ملفه فقط |
| إنشاء سجل مريض | نعم | لا | لا |
| تحديث سجل مريض | نعم | نعم | لا |
| حذف سجل مريض | نعم | لا | لا |
| عرض القياسات | نعم | نعم | قياساته فقط |
| إضافة قياسات | نعم | نعم | لنفسه فقط |
| تعديل أو حذف القياسات | نعم | نعم | لا |
| عرض الأدوية | نعم | نعم | أدويته فقط |
| إدارة الأدوية | نعم | نعم | لا |
| عرض المواعيد | نعم | نعم | مواعيده فقط |
| إدارة المواعيد | نعم | نعم | لا |

تستخدم Controllers صفات مثل:

```csharp
[Authorize]
```

وهذا يعني أن أي مستخدم يحمل JWT صالحًا يمكنه الدخول، ثم قد يُطبق فحص إضافي داخل Action.

```csharp
[Authorize(Roles = "Admin,Doctor")]
```

وهذا يسمح للمسؤول أو الطبيب فقط. عند غياب JWT تكون النتيجة غالبًا `401 Unauthorized`، وعند وجود مستخدم مصادق عليه لكنه لا يحمل الدور المطلوب تكون النتيجة `403 Forbidden`.

## حماية ملكية بيانات المريض

الدور وحده غير كافٍ. لو استُخدم `[Authorize(Roles = "Patient")]` فقط، فقد يستطيع أي مريض تغيير رقم المسار وطلب بيانات مريض آخر. لذلك ينفذ المشروع Ownership Check:

```text
JWT NameIdentifier
        |
        v
هوية المستخدم الحالي
        |
        v
Patient.UserId الخاص بالسجل المطلوب
        |
        +-- متطابق --> السماح
        |
        +-- غير متطابق --> 403 Forbidden
```

يطبق هذا الفحص على ملف المريض وقياساته وأدويته ومواعيده. أما `Admin` و`Doctor` فيستطيعان الوصول وفق صلاحياتهما الطبية والإدارية.

## توثيق نقاط النهاية API Endpoints

### Authentication

| Method | Endpoint | الحماية | الوظيفة |
| --- | --- | --- | --- |
| `POST` | `/api/auth/register` | Public | إنشاء مستخدم وإسناد دور موجود إليه |
| `POST` | `/api/auth/login` | Public + Strict Rate Limit | إصدار Access Token وRefresh Token |
| `POST` | `/api/auth/refresh` | Public | استبدال زوج رموز منتهي/صالح بزوج جديد |

### Patients

| Method | Endpoint | الصلاحية | الاستجابة الناجحة |
| --- | --- | --- | --- |
| `GET` | `/api/patients` | Admin, Doctor | `200 OK` وقائمة المرضى |
| `GET` | `/api/patients/{id}` | مستخدم مصادق؛ المريض لملفه فقط | `200 OK` |
| `POST` | `/api/patients` | Admin | `201 Created` مع السجل الجديد |
| `PUT` | `/api/patients/{id}` | Admin, Doctor | `204 No Content` |
| `DELETE` | `/api/patients/{id}` | Admin | `204 No Content` |

مثال إنشاء مريض:

```json
{
  "firstName": "Ahmad",
  "lastName": "Ali",
  "dateOfBirth": "1990-05-12",
  "gender": "Male",
  "contactNumber": "+970599123456"
}
```

### Vital Signs

| Method | Endpoint | الصلاحية | الوظيفة |
| --- | --- | --- | --- |
| `GET` | `/api/patients/{patientId}/vitals` | Authenticated؛ المريض لنفسه | جميع قراءات المريض |
| `POST` | `/api/patients/{patientId}/vitals` | Admin, Doctor، أو المريض لنفسه | تسجيل قراءة جديدة |
| `GET` | `/api/vitals/{id}` | Authenticated؛ المريض لنفسه | قراءة واحدة |
| `PUT` | `/api/vitals/{id}` | Admin, Doctor | تعديل القراءة |
| `DELETE` | `/api/vitals/{id}` | Admin, Doctor | حذف القراءة |

مثال تسجيل قراءة:

```json
{
  "heartRate": 78,
  "oxygenSaturation": 98.5,
  "systolicBP": 120,
  "diastolicBP": 80
}
```

وقت `RecordedAt` لا يرسله العميل عند الإنشاء؛ تضيفه الخدمة من وقت الخادم.

### Medications

| Method | Endpoint | الصلاحية | الوظيفة |
| --- | --- | --- | --- |
| `GET` | `/api/patients/{patientId}/medications` | Authenticated؛ المريض لنفسه | قائمة أدوية المريض |
| `POST` | `/api/patients/{patientId}/medications` | Admin, Doctor | إضافة دواء |
| `GET` | `/api/medications/{id}` | Authenticated؛ المريض لنفسه | عرض دواء |
| `PUT` | `/api/medications/{id}` | Admin, Doctor | تعديل دواء |
| `DELETE` | `/api/medications/{id}` | Admin, Doctor | حذف دواء |

مثال إضافة دواء:

```json
{
  "name": "Aspirin",
  "dosage": "81 mg",
  "frequency": "Once daily",
  "startDate": "2026-08-15T08:00:00Z",
  "endDate": null,
  "isActive": true
}
```

### Appointments

| Method | Endpoint | الصلاحية | الوظيفة |
| --- | --- | --- | --- |
| `GET` | `/api/patients/{patientId}/appointments` | Authenticated؛ المريض لنفسه | قائمة مواعيد المريض |
| `POST` | `/api/patients/{patientId}/appointments` | Admin, Doctor | إنشاء موعد |
| `GET` | `/api/appointments/{id}` | Authenticated؛ المريض لنفسه | عرض موعد |
| `PUT` | `/api/appointments/{id}` | Admin, Doctor | تعديل الموعد |
| `DELETE` | `/api/appointments/{id}` | Admin, Doctor | حذف الموعد |

مثال إنشاء موعد:

```json
{
  "doctorId": "doctor-id-123",
  "appointmentDate": "2026-09-01T10:30:00Z",
  "status": "Scheduled",
  "notes": "Routine cardiac follow-up"
}
```

## التحقق من المدخلات

تسجل Validators بواسطة:

```csharp
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<CreatePatientRequestValidator>();
```

وبذلك يكتشف FluentValidation جميع Validators في Assembly نفسه.

### قواعد المريض

- الاسم الأول والأخير مطلوبان وبطول حرفين على الأقل.
- تاريخ الميلاد يجب أن يكون في الماضي.
- الجنس يجب أن يكون `Male` أو `Female`.
- رقم الاتصال من 10 إلى 15 رقمًا، ويمكن أن يبدأ بعلامة `+`.

### قواعد القياسات الحيوية

| القياس | المجال المقبول |
| --- | --- |
| Heart Rate | من 30 إلى 250 bpm |
| Oxygen Saturation | من 50 إلى 100% |
| Systolic BP | من 70 إلى 220 mmHg |
| Diastolic BP | من 40 إلى 130 mmHg |

هذه مجالات تحقق برمجية عامة وليست تشخيصًا طبيًا.

### قواعد الأدوية

- الاسم والجرعة والتكرار وتاريخ البداية حقول مطلوبة.
- اسم الدواء لا يتجاوز 100 حرف.
- إذا وُجد تاريخ نهاية فيجب أن يكون بعد تاريخ البداية.

### قواعد المواعيد

- `DoctorId` مطلوب.
- التاريخ يجب أن يكون في المستقبل.
- الحالة إحدى: `Scheduled`, `Completed`, `Cancelled`.

## تحديد معدل الطلبات Rate Limiting

يستخدم المشروع Fixed Window Limiter، أي عدادًا ثابتًا داخل نافذة زمنية مدتها دقيقة:

### GeneralPolicy

- الحد: 30 طلبًا في الدقيقة.
- Queue Limit: طلبان.
- ترتيب الانتظار: الأقدم أولًا.
- مطبقة على Controllers الطبية.

### StrictLoginPolicy

- الحد: 5 محاولات في الدقيقة.
- لا توجد Queue.
- مطبقة على endpoint تسجيل الدخول.

عند تجاوز الحد يعيد الخادم:

```http
HTTP/1.1 429 Too Many Requests
```

الهدف هو حماية موارد الخادم وتقليل Brute-force على كلمات المرور. في الأنظمة الموزعة الكبيرة يفضّل استخدام مخزن مركزي أو API Gateway حتى تتشارك كل نسخ التطبيق العدادات نفسها.

## CORS وHTTPS وHSTS

### CORS

CORS سياسة متصفح تحدد أي Frontend origin يستطيع استدعاء الـ API. لا تعد CORS بديلًا عن Authentication؛ فالعملاء غير المتصفحية يمكنهم إرسال طلبات بغض النظر عنها.

السياسة الحالية اسمها `AllowFrontendOnly`، وتسمح بالـ headers والـ methods من origin محدد. يجب استبداله بعنوان الواجهة الحقيقي، مثل:

```csharp
policy.WithOrigins("https://localhost:3000")
      .AllowAnyHeader()
      .AllowAnyMethod();
```

لا تضع `/` في نهاية origin ولا تضف مسارًا مثل `/login`.

### HTTPS Redirection

`UseHttpsRedirection()` يحول طلب HTTP إلى HTTPS لحماية البيانات والرموز أثناء النقل.

### HSTS

يعمل HSTS خارج بيئة Development فقط. يخبر المتصفح باستخدام HTTPS مستقبلًا لمدة سنة، ويتضمن subdomains، ويطلب preload. يجب التأكد من دعم HTTPS في كل النطاقات الفرعية قبل تفعيل هذه الخيارات في بيئة حقيقية.

## إعداد وتشغيل المشروع

### المتطلبات

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server أو SQL Server Express
- Visual Studio 2022 أو VS Code أو Rider — اختياري
- أداة `dotnet-ef`

### 1. التأكد من إصدار .NET

```powershell
dotnet --version
```

يجب أن يظهر إصدار 8 أو إصدار SDK أحدث يدعم استهداف .NET 8.

### 2. استعادة الحزم

من داخل مجلد المشروع:

```powershell
dotnet restore
```

### 3. إعداد Connection String

عدّل `appsettings.json` وفق SQL Server لديك:

```json
{
  "ConnectionStrings": {
    "CardiacMonitorConnection": "Server=YOUR_SERVER;Database=CardiacMonitorDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
  }
}
```

إذا كنت تستخدم SQL Server Authentication:

```text
Server=YOUR_SERVER;Database=CardiacMonitorDb;User Id=YOUR_USER;Password=YOUR_PASSWORD;TrustServerCertificate=True
```

لا تحفظ بيانات إنتاج حقيقية داخل Git.

### 4. إعداد JWT

المفاتيح المطلوبة:

```json
{
  "Jwt": {
    "Key": "A_LONG_RANDOM_SECRET_WITH_AT_LEAST_32_CHARACTERS",
    "Issuer": "CardiacMonitorAPI",
    "Audience": "CardiacMonitorAPI",
    "DurationInMinutes": 60
  }
}
```

الأفضل استخدام User Secrets محليًا:

```powershell
dotnet user-secrets init
dotnet user-secrets set "Jwt:Key" "YOUR_LONG_RANDOM_SECRET"
dotnet user-secrets set "ConnectionStrings:CardiacMonitorConnection" "YOUR_CONNECTION_STRING"
```

### 5. تثبيت EF Core CLI عند الحاجة

```powershell
dotnet tool install --global dotnet-ef --version 8.*
```

أو تحديثها إذا كانت مثبتة:

```powershell
dotnet tool update --global dotnet-ef --version 8.*
```

### 6. إنشاء أو تحديث قاعدة البيانات

```powershell
dotnet ef database update
```

يقرأ EF Core مجلد `Data/Migrations` وينفذ فقط Migrations التي لم تُطبق بعد. كما ينشئ جدول `__EFMigrationsHistory` لتتبع حالة قاعدة البيانات.

### 7. إصلاح شهادة التطوير

إذا فشل HTTPS بسبب شهادة مفقودة أو منتهية:

```powershell
dotnet dev-certs https --clean
dotnet dev-certs https --trust
```

### 8. بناء المشروع

```powershell
dotnet build
```

### 9. تشغيل المشروع

```powershell
dotnet run --launch-profile https
```

العناوين الافتراضية:

| الخدمة | العنوان |
| --- | --- |
| HTTPS API | `https://localhost:7142` |
| HTTP API | `http://localhost:5142` |
| Swagger UI | `https://localhost:7142/swagger` |
| OpenAPI JSON | `https://localhost:7142/swagger/v1/swagger.json` |

Swagger يعمل فقط عندما تكون البيئة `Development`.

## تجربة المشروع عمليًا

### باستخدام Swagger

1. شغل المشروع وافتح `/swagger`.
2. نفذ `POST /api/auth/register` لإنشاء مستخدم تجريبي، أو استخدم مستخدمًا موجودًا.
3. نفذ `POST /api/auth/login`.
4. انسخ قيمة `token` فقط.
5. اضغط زر **Authorize** أعلى Swagger.
6. الصق Access Token؛ إعداد Swagger يضيف مخطط Bearer تلقائيًا.
7. جرّب endpoint مسموحًا لدور المستخدم.
8. لاحظ الفرق بين `401` و`403` عند غياب الرمز أو نقص الصلاحية.

### باستخدام curl

تسجيل الدخول:

```bash
curl -X POST "https://localhost:7142/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{"email":"user@example.com","password":"StrongPassword123!"}'
```

طلب محمي:

```bash
curl "https://localhost:7142/api/patients" \
  -H "Authorization: Bearer ACCESS_TOKEN"
```

### ترتيب مقترح لفهم المشروع من الكود

إذا كنت تدرس المشروع، اقرأ الملفات بهذا الترتيب:

1. `Program.cs` لفهم الخدمات وHTTP Pipeline.
2. `Models/` لفهم البيانات.
3. `Data/AppDbContext.cs` لفهم العلاقات وSeed Data.
4. `DTOs/` لفهم عقود الـ API.
5. `Validators/` لفهم قواعد الإدخال.
6. Interfaces داخل `Services/` لفهم العمليات المتاحة.
7. Implementations داخل `Services/` لفهم منطق قاعدة البيانات.
8. `Controllers/` لفهم routes والصلاحيات والاستجابات.
9. `Data/Migrations/` في النهاية، لأن ملفاتها مولدة غالبًا بواسطة EF Core.

## رموز استجابة HTTP

| الرمز | المعنى في المشروع |
| --- | --- |
| `200 OK` | نجح العرض أو تسجيل الدخول أو التسجيل |
| `201 Created` | تم إنشاء سجل جديد، مع رابط المورد غالبًا |
| `204 No Content` | نجح التحديث أو الحذف ولا يوجد Body مطلوب |
| `400 Bad Request` | المدخلات غير صحيحة أو تعذر تنفيذ العملية |
| `401 Unauthorized` | الرمز غائب أو غير صالح، أو بيانات الدخول خاطئة |
| `403 Forbidden` | المستخدم معروف لكنه لا يملك الدور أو ملكية السجل |
| `404 Not Found` | السجل المطلوب غير موجود |
| `429 Too Many Requests` | تم تجاوز Rate Limit |
| `500 Internal Server Error` | خطأ غير معالج داخل الخادم |

## بيانات Seed

يضيف `AppDbContext` عند تطبيق Migrations:

- أدوار `Admin` و`Doctor` و`Patient`.
- مستخدم طبيب للتطوير.
- مرضى تجريبيين.
- قراءات حيوية تجريبية.

بيانات Seed مفيدة للتجربة، لكن أي بيانات دخول ثابتة يجب تغييرها أو حذفها قبل النشر. لا تستخدم كلمات مرور تجريبية في Production.

## ملاحظات أمنية مهمة

المشروع يطبق أساسًا أمنيًا جيدًا، لكن قبل استخدامه في Production يجب الانتباه إلى التالي:

1. **JWT Key:** انقله إلى Secret Manager أو Environment Variables واجعله عشوائيًا وقويًا.
2. **Connection String:** لا تحفظ كلمة مرور قاعدة البيانات داخل المستودع.
3. **Public Registration:** endpoint التسجيل يسمح للعميل بطلب دور موجود. يجب منع إنشاء `Admin` أو `Doctor` علنًا وقصر ذلك على مسؤول موثوق.
4. **Seed Credentials:** غيّر أو أزل المستخدم وكلمة المرور التجريبيين.
5. **Refresh Tokens:** أضف endpoint للإلغاء وتخلص دوريًا من الرموز المنتهية.
6. **CORS:** استبدل origin الحالي بعنوان Frontend الحقيقي، ولا تستخدم `AllowAnyOrigin` مع بيانات حساسة.
7. **HTTPS:** استخدم شهادة موثوقة في الإنتاج، وليس Development Certificate.
8. **Rate Limiting:** فكر في partitioning حسب IP أو user identity وسياسة موزعة عند تشغيل أكثر من نسخة.
9. **Logging:** لا تسجل JWT أو كلمات المرور أو البيانات الطبية الحساسة.
10. **Medical Privacy:** طبق تشفيرًا وAudit Logs وسياسة احتفاظ بالبيانات بما يتوافق مع قوانين البلد المستهدف.
11. **Error Handling:** أضف Global Exception Handler لإرجاع Problem Details موحدة دون كشف stack traces.
12. **Testing:** أضف Unit Tests للخدمات وIntegration Tests للصلاحيات وOwnership Check.

## أفكار لتطوير المشروع مستقبلًا

- ربط حساب المريض بسجله الطبي ضمن workflow إداري آمن.
- إضافة Pagination وFiltering وSorting للقوائم.
- إضافة تنبيهات عند تجاوز القياسات حدودًا يحددها الطبيب.
- إضافة سجل Audit لكل عملية قراءة أو تعديل للبيانات الطبية.
- إرسال تذكيرات بالمواعيد والأدوية.
- إضافة إلغاء Refresh Token وتسجيل الخروج من جميع الأجهزة.
- إضافة Global Error Handling باستخدام `IExceptionHandler`.
- إضافة Health Checks لقاعدة البيانات.
- إضافة اختبارات آلية وCI/CD.
- نقل الإعدادات الحساسة إلى Azure Key Vault أو خدمة أسرار مشابهة.
- إضافة Docker وملف `docker-compose` لتشغيل API وSQL Server معًا.

## الترخيص

لا يوجد ملف ترخيص حاليًا. أضف ترخيصًا مناسبًا قبل توزيع المشروع أو قبول مساهمات خارجية.
