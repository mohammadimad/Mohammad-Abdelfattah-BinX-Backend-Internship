using CardiacMonitor.DTOs;
namespace CardiacMonitor.Services;

public interface IAppointmentService
{
    Task<IEnumerable<AppointmentResponse>> GetAppointmentsByPatientIdAsync(int patientId);
    Task<AppointmentResponse?> GetAppointmentByIdAsync(int id);
    Task<AppointmentResponse?> CreateAppointmentAsync(int patientId, CreateAppointmentRequest request);
    Task<bool> UpdateAppointmentAsync(int id, UpdateAppointmentRequest request);
    Task<bool> DeleteAppointmentAsync(int id);
}
