# أسئلة الكود المتوقعة لتحديثات الأسبوع السابع

استخدم القاعدة التالية في الإجابة:

> عرّف المصطلح بجملة، اشرح لماذا استخدمته، ثم افتح الكود الذي يثبته.

## 1. ما هو Separation of Concerns؟

الإجابة:

> فصلت مسؤولية تسجيل الدخول عن معلومات الطبيب المهنية. `IdentityUser` للمصادقة، و`DoctorProfile` للترخيص والتخصص والقسم.

الكود الذي تعرضه من `Models/DoctorProfile.cs`:

```csharp
public class DoctorProfile
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string LicenseNumber { get; set; } = string.Empty;
    public string Specialty { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
}
```

## 2. أين طبقت One-to-One Relationship؟

الإجابة:

> كل حساب Doctor في Identity يمكن أن يمتلك DoctorProfile واحدًا فقط.

من `DoctorProfileConfiguration.cs`:

```csharp
builder.HasIndex(doctor => doctor.UserId).IsUnique();

builder.HasOne(doctor => doctor.User)
    .WithOne()
    .HasForeignKey<DoctorProfile>(doctor => doctor.UserId);
```

`HasForeignKey` ينشئ المفتاح الأجنبي، و`IsUnique` يمنع ربط أكثر من Profile بالحساب نفسه.

## 3. أين طبقت One-to-Many؟

الإجابة:

> الطبيب الواحد لديه عدة فترات دوام، وكل فترة تعود إلى طبيب واحد.

من `DoctorAvailabilityConfiguration.cs`:

```csharp
builder.HasOne(slot => slot.DoctorProfile)
    .WithMany(doctor => doctor.AvailabilitySlots)
    .HasForeignKey(slot => slot.DoctorProfileId);
```

## 4. ما الفرق بين Authentication وAuthorization؟

الإجابة:

> Authentication يثبت هوية المستخدم بواسطة JWT. Authorization يقرر هل دوره وملكيته للمورد يسمحان بتنفيذ العملية.

من `Program.cs`:

```csharp
builder.Services.AddAuthentication()
    .AddJwtBearer(options => { /* token validation */ });

app.UseAuthentication();
app.UseAuthorization();
```

## 5. أين طبقت RBAC؟

`RBAC` تعني Role-Based Access Control.

من `DoctorsController.cs`:

```csharp
[HttpPost("api/doctors/{doctorProfileId:int}/availability")]
[Authorize(Roles = "Admin,Doctor")]
public async Task<IActionResult> AddAvailability(...)
```

الإجابة:

> الـRole يسمح فقط للـAdmin أو Doctor بالدخول إلى العملية، لكنه لا يكفي وحده لتحديد الطبيب الذي يملكه المستخدم.

## 6. أين طبقت Resource-Based Authorization؟

من `DoctorScheduleService.cs`:

```csharp
if (user.IsInRole("Admin"))
{
    return true;
}

var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
return await _context.DoctorProfiles.AnyAsync(doctor =>
    doctor.Id == doctorProfileId &&
    doctor.UserId == userId &&
    doctor.IsActive);
```

الإجابة:

> بعد فحص Role، أتحقق أن DoctorProfile المطلوب يعود فعلًا إلى المستخدم الحالي. لذلك لا يستطيع طبيب تعديل جدول طبيب آخر.

## 7. أين طبقت Dependency Injection؟

من `Program.cs`:

```csharp
builder.Services.AddScoped<IDoctorScheduleService, DoctorScheduleService>();
builder.Services.AddScoped<IMedicalAlertService, MedicalAlertService>();
```

ومن `AppointmentService.cs`:

```csharp
public AppointmentService(
    AppDbContext context,
    IDoctorScheduleService doctorScheduleService)
```

الإجابة:

> الخدمة تعتمد على Interface، والـDI Container يزوّدها بالتنفيذ. هذا يقلل الترابط ويسهل الاختبار باستخدام Mock.

## 8. أين توجد Business Rule؟

من `AppointmentService.cs`:

```csharp
if (!await IsDoctorAsync(request.DoctorId)) return null;

if (!await _doctorScheduleService.IsDoctorAvailableAsync(
        request.DoctorId,
        request.AppointmentDate)) return null;

if (await HasSchedulingConflictAsync(
        request.DoctorId,
        request.AppointmentDate)) return null;
```

الإجابة:

> الموعد لا يكفي أن يكون Valid JSON؛ يجب أن يكون المستخدم طبيبًا، والوقت داخل دوامه، وألا يوجد تعارض مع موعد آخر.

## 9. كيف منعت تداخل فترات الدوام؟

من `DoctorScheduleService.cs`:

```csharp
slot.StartTime < request.EndTime &&
request.StartTime < slot.EndTime
```

الإجابة:

> فترتان تتداخلان إذا بدأت كل واحدة قبل نهاية الأخرى. الفترة `09:00–12:00` تتداخل مع `11:00–13:00`، لكنها لا تتداخل مع فترة تبدأ عند `12:00`.

## 10. أين طبقت Data Integrity؟

من `DoctorAvailabilityConfiguration.cs`:

```csharp
builder.HasIndex(slot => new
{
    slot.DoctorProfileId,
    slot.DayOfWeek,
    slot.StartTime,
    slot.EndTime
}).IsUnique();
```

الإجابة:

> استخدمت Unique Index لمنع تكرار الفترة نفسها حتى لو وصل الإدخال من مكان آخر غير الـAPI.

## 11. ما هو Check Constraint وأين استخدمته؟

```csharp
table.HasCheckConstraint(
    "CK_DoctorAvailability_TimeRange",
    "[StartTime] < [EndTime]");
```

وفي التنبيهات:

```csharp
table.HasCheckConstraint(
    "CK_MedicalAlerts_Status",
    "[Status] IN ('Open', 'Acknowledged', 'Resolved')");
```

الإجابة:

> Check Constraint هو خط حماية داخل قاعدة البيانات يمنع تخزين حالة أو فترة غير صحيحة، حتى لو تم تجاوز Validation في التطبيق.

## 12. أين طبقت Transaction أو Atomic Operation؟

من `StaffService.cs` داخل `CreateDoctorAsync`:

```csharp
await using var transaction =
    await _context.Database.BeginTransactionAsync();

var creationResult =
    await _userManager.CreateAsync(user, request.Password);

var roleResult =
    await _userManager.AddToRoleAsync(user, "Doctor");

_context.DoctorProfiles.Add(doctor);
await _context.SaveChangesAsync();
await transaction.CommitAsync();
```

الإجابة:

> إنشاء IdentityUser والـRole والـDoctorProfile يجب أن ينجح كله أو يفشل كله. لا أريد حساب Doctor دون ملف مهني.

## 13. أين يوجد Automatic Alert Generation؟

من `VitalSignService.cs`:

```csharp
_context.VitalSigns.Add(vital);
SynchronizeMedicalAlert(vital);
await _context.SaveChangesAsync();
```

الإجابة:

> العميل ينشئ VitalSign فقط. الخدمة تقيم القيم وتنشئ MedicalAlert تلقائيًا قبل عملية الحفظ نفسها.

## 14. كيف اخترت أعلى Severity؟

من `VitalSignService.cs` داخل `GetHighestSeverity`:

```csharp
if (/* critical thresholds */)
{
    return "Critical";
}

if (/* high thresholds */)
{
    return "High";
}

if (/* medium thresholds */)
{
    return "Medium";
}
```

الإجابة:

> أفحص Critical أولًا، ثم High ثم Medium، لذلك إذا تجاوزت القراءة أكثر من مستوى يحصل التنبيه على أعلى مستوى.

تنبيه يجب قوله:

> هذه Thresholds تجريبية للمشروع وليست تشخيصًا طبيًا.

## 15. ما هو Audit Trail؟

من `MedicalAlertService.cs`:

```csharp
alert.Status = "Acknowledged";
alert.AcknowledgedByUserId = userId;
alert.AcknowledgedAt = DateTime.UtcNow;
```

وعند الحل:

```csharp
alert.Status = "Resolved";
alert.ResolvedByUserId = userId;
alert.ResolvedAt = DateTime.UtcNow;
```

الإجابة:

> لا أحفظ الحالة النهائية فقط؛ أحفظ من استلم التنبيه ومن حله ووقت كل انتقال.

## 16. هل هذا State Machine؟

الإجابة:

> هي دورة حالات بسيطة: `Open → Acknowledged → Resolved`. الخدمة تمنع استلام Alert ليست Open، وتمنع حل Alert محلولة مسبقًا.

```csharp
if (alert.Status != "Open")
{
    return new MedicalAlertActionResult(
        false,
        "Only an open alert can be acknowledged.");
}
```

## 17. لماذا استخدمت DTO؟

من `DTOs/DoctorDtos.cs`:

```csharp
public sealed record CreateDoctorRequest(
    string Email,
    string Password,
    string FullName,
    string LicenseNumber,
    string Specialty,
    string Department);
```

الإجابة:

> الـDTO يحدد عقد الـAPI ولا يكشف Entity أو PasswordHash أو Navigation Properties مباشرة للعميل.

## 18. أين طبقت Validation؟

من `CreateDoctorAvailabilityRequestValidator.cs`:

```csharp
RuleFor(request => request.DayOfWeek).IsInEnum();
RuleFor(request => request.StartTime)
    .LessThan(request => request.EndTime);
```

ومن `Program.cs`:

```csharp
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<
    CreatePatientRequestValidator>();
```

الإجابة:

> FluentValidation يرفض الطلب مبكرًا برسالة واضحة، بينما Database Constraints توفر حماية أخيرة للبيانات.

## 19. لماذا استخدمت Async/Await؟

```csharp
var doctorExists = await _context.DoctorProfiles.AnyAsync(...);
await _context.SaveChangesAsync();
```

الإجابة:

> عمليات قاعدة البيانات I/O-bound. استخدام async يمنع حجز Thread أثناء انتظار SQL Server ويحسن قدرة API على خدمة طلبات متزامنة.

## 20. ما هي LINQ؟

```csharp
return await _context.DoctorAvailabilitySlots.AnyAsync(slot =>
    slot.DoctorProfile.UserId == doctorUserId &&
    slot.DayOfWeek == appointmentDate.DayOfWeek &&
    slot.StartTime <= appointmentTime &&
    appointmentTime < slot.EndTime);
```

الإجابة:

> LINQ تسمح بكتابة الاستعلام بـC#، ويحوّله EF Core إلى SQL وينفذه داخل قاعدة البيانات.

## 21. لماذا استخدمت AsNoTracking؟

```csharp
return await _context.DoctorAvailabilitySlots
    .AsNoTracking()
    .Where(slot => slot.IsActive)
    .ToListAsync();
```

الإجابة:

> هذا استعلام قراءة فقط، لذلك لا أحتاج Change Tracking. يقل استهلاك الذاكرة ويحسن أداء القراءة.

## 22. ما الفرق بين Cascade وRestrict وSetNull؟

من `MedicalAlertConfiguration.cs`:

```csharp
.OnDelete(DeleteBehavior.Restrict);
```

تعني منع الحذف إذا كانت هناك بيانات Audit مرتبطة.

```csharp
.OnDelete(DeleteBehavior.SetNull);
```

تعني أن حذف VitalSign يجعل `VitalSignId` داخل Alert بقيمة null مع إبقاء سجل التنبيه.

```csharp
.OnDelete(DeleteBehavior.Cascade);
```

تعني حذف الأبناء تلقائيًا عند حذف الأب، وتستخدم فقط عندما يكون ذلك آمنًا.

## 23. ما هو Soft Delete؟

من `DoctorScheduleService.cs`:

```csharp
slot.IsActive = false;
await _context.SaveChangesAsync();
```

الإجابة:

> لا أحذف فترة الدوام فعليًا، بل أعطلها باستخدام IsActive حتى أحافظ على التاريخ.

## 24. ما هي Dependency Inversion؟

```csharp
private readonly IDoctorScheduleService _doctorScheduleService;
```

الإجابة:

> `AppointmentService` يعتمد على abstraction وهو Interface، وليس على `DoctorScheduleService` مباشرة. هذا جزء من مبدأ Dependency Inversion ويسهل استخدام Mock في الاختبارات.

## 25. ما هو Middleware؟

من `Program.cs`:

```csharp
app.UseMiddleware<RequestCorrelationMiddleware>();
```

الإجابة:

> Middleware ينفذ منطقًا مشتركًا على جميع الطلبات. هنا يضيف Correlation ID ويقيس زمن الطلب بدل تكرار هذا الكود داخل كل Controller.

## 26. لماذا تستخدم UTC؟

```csharp
alert.AcknowledgedAt = DateTime.UtcNow;
alert.ResolvedAt = DateTime.UtcNow;
```

الإجابة:

> UTC يمنع اختلاف النتائج بين السيرفر والمستخدمين في مناطق زمنية مختلفة. التحويل للتوقيت المحلي يكون عند العرض.

## 27. ما الفرق بين Unit Test وIntegration Test؟

الإجابة:

> Unit Test يفحص Service أو Rule بصورة معزولة. Integration Test يشغل HTTP pipeline والـController والـDI وEF Core والصلاحيات معًا.

اختبارات تعرضها:

```text
DoctorSchedule_AllowsInsideAppointment_AndRejectsOutsideAppointment
CriticalVitalSign_CreatesAlert_ThatCanBeAcknowledgedAndResolved
```

## 28. لماذا أبقيت Appointment.DoctorId مرتبطًا بـIdentityUser؟

الإجابة:

> لحماية البيانات القديمة وتجنب Migration تغير كل المواعيد. أصل إلى DoctorProfile عن طريق UserId، ولذلك أضفت الميزة دون كسر التصميم الموجود.

هذه إجابة قوية لأنها تظهر اهتمامك بـBackward Compatibility وحماية البيانات.

## 29. إذا سألك: هل استخدمت Repository Pattern؟

الإجابة الصادقة:

> لا يوجد Custom Repository في المشروع. أستخدم `AppDbContext` داخل Service Layer لأن EF Core DbContext يوفر سلوك Repository وUnit of Work. يمكن إضافة Repository إذا أصبح المشروع أكبر، لكن إضافته الآن قد تكون abstraction غير ضرورية.

لا تدّعِ وجود Pattern غير موجود.

## 30. إذا سألك: ما أقوى نقطة في التحديث؟

قل:

> أقوى نقطة أن الـEntities الجديدة مرتبطة بسلوك فعلي: Availability تمنع الموعد المخالف، وVitalSign ينشئ Alert تلقائيًا، وAlert يمتلك دورة حياة موثقة وصلاحيات واضحة.

## خريطة الملفات السريعة

| إذا سأل عن | افتح |
| --- | --- |
| Entity وProfile | `Models/DoctorProfile.cs` |
| Relationships وConstraints | `Data/Configurations/` |
| تداخل الدوام | `Services/DoctorScheduleService.cs` |
| تطبيق الدوام على الموعد | `Services/AppointmentService.cs` |
| إنشاء التنبيه الآلي | `Services/VitalSignService.cs` |
| Audit Trail | `Services/MedicalAlertService.cs` |
| Roles والصلاحيات | `Controllers/DoctorsController.cs` |
| Transaction إنشاء الطبيب | `Services/StaffService.cs` |
| Dependency Injection وJWT | `Program.cs` |
| الاختبارات الكاملة | `tests/CardiacMonitor.IntegrationTests/ApiEndpointsTests.cs` |

## جملة ختامية تحفظها

> I separated authentication from domain data, enforced scheduling through business rules, protected data with authorization and database constraints, and converted abnormal vital signs into an auditable alert workflow.
