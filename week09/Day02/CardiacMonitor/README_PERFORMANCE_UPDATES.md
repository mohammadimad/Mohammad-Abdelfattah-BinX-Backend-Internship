# تحديثات الاستعلامات المتقدمة وتحسين الأداء

يوثّق هذا الملف تحديثات الأداء الحالية وطريقة قياسها. القاعدة المتّبعة هي أن أي نتيجة أداء لا تُقبل دون قياس فعلي قبل التعديل وبعده على قاعدة بيانات وبيانات اختبار مماثلة.

## 1. تشخيص N+1 والقياس

تم تفعيل `LogTo(Console.WriteLine, LogLevel.Information)` في بيئة `Development` فقط. كما أضيف عدّاد لأوامر SQL وMiddleware يسجّل زمن الطلب وعدد الأوامر بهذا الشكل:

```text
Database profile GET /api/patients: 2 queries in 18.42 ms (HTTP 200).
```

المسارات المطلوب قياسها هي:

- `GET /api/patients`
- `GET /api/patients/{patientId}/vitals`
- `GET /api/nurses/me/patients`

شغّل كل مسار بعدد ثابت من مرات الإحماء، ثم سجّل متوسط خمس محاولات على الأقل. لا توجد أرقام قبلية موثوقة داخل المستودع، لذلك لم توضع أرقام افتراضية أو مصطنعة. يملأ الجدول التالي من سجلات بيئة SQL Server الفعلية عند مراجعة الأداء:

| المسار | قبل: عدد SQL | قبل: متوسط الزمن | بعد: عدد SQL | بعد: متوسط الزمن |
|---|---:|---:|---:|---:|
| `/api/patients` | يُقاس | يُقاس | يُقاس | يُقاس |
| `/api/patients/{id}/vitals` | يُقاس | يُقاس | يُقاس | يُقاس |
| `/api/nurses/me/patients` | يُقاس | يُقاس | يُقاس | يُقاس |

عدد الأوامر المتوقع من شكل الشيفرة بعد التحديث هو أمران لمسارات القوائم ذات pagination (`COUNT` ثم صفحة النتائج)، وأمر واحد لقائمة مرضى الممرض. قد يضيف فحص صلاحية الممرض أمرًا آخر قبل استعلام العلامات الحيوية.

> لا يُستخدم `EnableSensitiveDataLogging` افتراضيًا حتى في التطوير لأن القيم قد تتضمن بيانات مرضى. يمكن تفعيله مؤقتًا محليًا فقط بعد استخدام بيانات غير حقيقية.

## 2. Projection لمسارات القوائم

تستخدم المسارات التالية `.Select()` مباشرة إلى DTO مع `AsNoTracking()` بدل تحميل كائنات كاملة أو علاقات غير مطلوبة:

- `PatientService.GetAllPatientsAsync`
- `VitalSignService.GetVitalSignsByPatientIdAsync`
- `CareAssignmentService.GetAssignedPatientsAsync`
- `DoctorScheduleService.GetAvailabilityAsync`

بهذا تُجلب الأعمدة المطلوبة فقط، وتبقى التصفية والترتيب و`Skip/Take` داخل SQL.

## 3. Split Query لتفاصيل المريض

أضيف المسار:

```http
GET /api/patients/{id}/clinical-details
```

هذا المسار مخصص لشاشة تفاصيل واحدة ويحمّل مجموعات العلامات الحيوية والأدوية والمواعيد والتنبيهات وتكليفات الرعاية. يستخدم `AsSplitQuery()` لتجنب تضاعف الصفوف الناتج عن JOIN واحد بين مجموعات متعددة، ثم يحوّل النتيجة إلى `PatientClinicalDetailsResponse`.

المقايضة: ينفذ EF Core عدة أوامر SQL، وقد تتغير البيانات بين الأوامر عند وجود كتابة متزامنة. إذا احتاجت حالة استخدام لاحقة إلى Snapshot متّسق تمامًا، يجب تشغيل القراءة داخل Transaction بمستوى عزل مناسب مع تقييم تكلفة الأقفال.

## 4. الفهارس المركبة

الفهارس المطلوبة موجودة في إعدادات EF والهجرة `20260903203751_Week7CareTeamSchedulingAndAlerts`:

- `VitalSigns (PatientId, RecordedAt)` باسم `IX_VitalSigns_PatientId_RecordedAt`.
- `MedicalAlerts (PatientId, Status, CreatedAt)`؛ أول عمودين يغطّيان فلترة المريض والحالة، والثالث يدعم ترتيب أحدث التنبيهات.
- `Appointments (DoctorId, AppointmentDate)` باسم `UX_Appointments_DoctorId_AppointmentDate`، وهو فريد كذلك لمنع الحجز المزدوج.
- `PatientCareAssignments (NurseProfileId, IsActive)` باسم `IX_PatientCareAssignments_NurseProfileId_IsActive`.

للتحقق في SSMS، فعّل **Include Actual Execution Plan** و`SET STATISTICS IO, TIME ON`، ثم نفّذ SQL الملتقط من سجل EF لكل مسار. النتيجة المقبولة هي استخدام `Index Seek` على الفهرس المناسب بدل `Table Scan` أو `Index Scan`، مع حفظ الخطة وقراءات الصفحات ضمن توثيق الـ PR.

## 5. Redis Cache-Aside

تمت إضافة `Microsoft.Extensions.Caching.StackExchangeRedis` وربط `IDistributedCache` باتصال `Redis` الموجود في `ConnectionStrings`. القيمة الافتراضية المحلية:

```json
"Redis": "localhost:6379"
```

يُخزّن فقط جدول توفر الطبيب النشط بالمفتاح `doctor-availability:{doctorProfileId}` وTTL مقداره 20 دقيقة. عند Cache Miss تُقرأ البيانات من SQL وتُحفظ في Redis، وعند Cache Hit تعاد مباشرة. يُحذف المفتاح فور نجاح إضافة موعد توفر أو تعطيله.

لا يتم تخزين أي من البيانات التالية:

- العلامات الحيوية الحية.
- حالة التنبيهات الطبية.
- تكليفات الرعاية النشطة.

تستخدم الاختبارات `DistributedMemoryCache` بدل الاعتماد على Redis خارجي، بينما تستخدم بيئات التشغيل Redis الحقيقي.

## التحقق المنفذ

```text
Unit tests:        34 passed
Integration tests: 18 passed
Total:             52 passed
```
