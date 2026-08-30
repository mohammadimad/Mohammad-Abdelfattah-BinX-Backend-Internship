using CardiacMonitor.Data;
using CardiacMonitor.DTOs;
using CardiacMonitor.Models;
using Microsoft.EntityFrameworkCore;

namespace CardiacMonitor.Services;

public class AppointmentService : IAppointmentService
{
    private readonly AppDbContext _context;

    public AppointmentService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<AppointmentResponse>> GetAppointmentsByPatientIdAsync(int patientId)
    {
        return await _context.Appointments
            .AsNoTracking()
            .Where(a => a.PatientId == patientId)
            .Select(a => new AppointmentResponse(a.Id, a.PatientId, a.DoctorId, a.AppointmentDate, a.Status, a.Notes))
            .ToListAsync();
    }

    public async Task<AppointmentResponse?> GetAppointmentByIdAsync(int id)
    {
        var app = await _context.Appointments
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id);

        if (app == null) return null;

        return new AppointmentResponse(app.Id, app.PatientId, app.DoctorId, app.AppointmentDate, app.Status, app.Notes);
    }

    public async Task<AppointmentResponse?> CreateAppointmentAsync(int patientId, CreateAppointmentRequest request)
    {
        var patientExists = await _context.Patients.AnyAsync(p => p.Id == patientId);
        if (!patientExists) return null;

        if (!await IsDoctorAsync(request.DoctorId)) return null;
        if (await HasSchedulingConflictAsync(
            request.DoctorId,
            request.AppointmentDate)) return null;

        var appointment = new Appointment
        {
            PatientId = patientId,
            DoctorId = request.DoctorId,
            AppointmentDate = request.AppointmentDate,
            Status = request.Status,
            Notes = request.Notes
        };

        _context.Appointments.Add(appointment);
        await _context.SaveChangesAsync();

        return new AppointmentResponse(appointment.Id, appointment.PatientId, appointment.DoctorId, appointment.AppointmentDate, appointment.Status, appointment.Notes);
    }

    public async Task<bool> UpdateAppointmentAsync(int id, UpdateAppointmentRequest request)
    {
        var app = await _context.Appointments.FirstOrDefaultAsync(a => a.Id == id);
        if (app == null) return false;

        if (!await IsDoctorAsync(request.DoctorId)) return false;
        if (await HasSchedulingConflictAsync(
            request.DoctorId,
            request.AppointmentDate,
            id)) return false;

        app.DoctorId = request.DoctorId;
        app.AppointmentDate = request.AppointmentDate;
        app.Status = request.Status;
        app.Notes = request.Notes;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAppointmentAsync(int id)
    {
        var app = await _context.Appointments.FirstOrDefaultAsync(a => a.Id == id);
        if (app == null) return false;

        _context.Appointments.Remove(app);
        await _context.SaveChangesAsync();
        return true;
    }

    // Checks that the selected identity user belongs to the Doctor role.
    private async Task<bool> IsDoctorAsync(string userId)
    {
        return await (
            from userRole in _context.UserRoles
            join role in _context.Roles on userRole.RoleId equals role.Id
            where userRole.UserId == userId && role.NormalizedName == "DOCTOR"
            select userRole).AnyAsync();
    }

    // Checks whether the doctor already has an appointment at the requested time.
    private async Task<bool> HasSchedulingConflictAsync(
        string doctorId,
        DateTime appointmentDate,
        int? excludedAppointmentId = null)
    {
        return await _context.Appointments.AnyAsync(appointment =>
            appointment.DoctorId == doctorId &&
            appointment.AppointmentDate == appointmentDate &&
            (!excludedAppointmentId.HasValue ||
             appointment.Id != excludedAppointmentId.Value));
    }
}
