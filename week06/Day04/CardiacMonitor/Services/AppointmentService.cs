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

        var doctorExists = await _context.Users.AnyAsync(u => u.Id == request.DoctorId);
        if (!doctorExists) return null;

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

        var doctorExists = await _context.Users.AnyAsync(u => u.Id == request.DoctorId);
        if (!doctorExists) return false;

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
}