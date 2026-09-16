using System.Security.Claims;
using CardiacMonitor.Data;
using CardiacMonitor.DTOs;
using CardiacMonitor.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace CardiacMonitor.Services;

public sealed class DoctorScheduleService : IDoctorScheduleService
{
    private static readonly DistributedCacheEntryOptions AvailabilityCacheOptions =
        new()
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(20)
        };
    private readonly AppDbContext _context;
    private readonly IDistributedCache? _cache;

    // Stores the database context and optional distributed cache used by schedules.
    public DoctorScheduleService(
        AppDbContext context,
        IDistributedCache? cache = null)
    {
        _context = context;
        _cache = cache;
    }

    // Creates or reactivates a non-overlapping Doctor availability slot.
    public async Task<DoctorAvailabilityResult> AddAvailabilityAsync(
        int doctorProfileId,
        CreateDoctorAvailabilityRequest request)
    {
        var doctorExists = await _context.DoctorProfiles.AnyAsync(doctor =>
            doctor.Id == doctorProfileId && doctor.IsActive);
        if (!doctorExists)
        {
            return new DoctorAvailabilityResult(
                false,
                "Active Doctor profile was not found.");
        }

        var overlappingSlotExists = await _context.DoctorAvailabilitySlots
            .AnyAsync(slot =>
                slot.DoctorProfileId == doctorProfileId &&
                slot.DayOfWeek == request.DayOfWeek &&
                slot.IsActive &&
                slot.StartTime < request.EndTime &&
                request.StartTime < slot.EndTime);
        if (overlappingSlotExists)
        {
            return new DoctorAvailabilityResult(
                false,
                "The requested availability overlaps an active slot.");
        }

        var slot = await _context.DoctorAvailabilitySlots
            .FirstOrDefaultAsync(item =>
                item.DoctorProfileId == doctorProfileId &&
                item.DayOfWeek == request.DayOfWeek &&
                item.StartTime == request.StartTime &&
                item.EndTime == request.EndTime);

        if (slot == null)
        {
            slot = new DoctorAvailability
            {
                DoctorProfileId = doctorProfileId,
                DayOfWeek = request.DayOfWeek,
                StartTime = request.StartTime,
                EndTime = request.EndTime
            };
            _context.DoctorAvailabilitySlots.Add(slot);
        }

        slot.IsActive = true;
        await _context.SaveChangesAsync();
        await InvalidateAvailabilityCacheAsync(doctorProfileId);

        return new DoctorAvailabilityResult(
            true,
            "Doctor availability created successfully.",
            ToResponse(slot));
    }

    // Returns the weekly availability configured for a Doctor profile.
    public async Task<IReadOnlyList<DoctorAvailabilityResponse>> GetAvailabilityAsync(
        int doctorProfileId)
    {
        var cacheKey = GetAvailabilityCacheKey(doctorProfileId);
        if (_cache != null)
        {
            var cachedJson = await _cache.GetStringAsync(cacheKey);
            if (!string.IsNullOrWhiteSpace(cachedJson))
            {
                return JsonSerializer.Deserialize<List<DoctorAvailabilityResponse>>(
                    cachedJson) ?? [];
            }
        }

        var availability = await _context.DoctorAvailabilitySlots
            .AsNoTracking()
            .Where(slot => slot.DoctorProfileId == doctorProfileId && slot.IsActive)
            .OrderBy(slot => slot.DayOfWeek)
            .ThenBy(slot => slot.StartTime)
            .Select(slot => new DoctorAvailabilityResponse(
                slot.Id,
                slot.DoctorProfileId,
                slot.DayOfWeek,
                slot.StartTime,
                slot.EndTime,
                slot.IsActive))
            .ToListAsync();

        if (_cache != null)
        {
            await _cache.SetStringAsync(
                cacheKey,
                JsonSerializer.Serialize(availability),
                AvailabilityCacheOptions);
        }

        return availability;
    }

    // Returns one availability slot by its identifier.
    public async Task<DoctorAvailabilityResponse?> GetAvailabilityByIdAsync(int id)
    {
        return await _context.DoctorAvailabilitySlots
            .AsNoTracking()
            .Where(slot => slot.Id == id)
            .Select(slot => new DoctorAvailabilityResponse(
                slot.Id,
                slot.DoctorProfileId,
                slot.DayOfWeek,
                slot.StartTime,
                slot.EndTime,
                slot.IsActive))
            .FirstOrDefaultAsync();
    }

    // Deactivates an availability slot without deleting its history.
    public async Task<bool> DeactivateAvailabilityAsync(int id)
    {
        var slot = await _context.DoctorAvailabilitySlots
            .FirstOrDefaultAsync(item => item.Id == id && item.IsActive);
        if (slot == null)
        {
            return false;
        }

        slot.IsActive = false;
        await _context.SaveChangesAsync();
        await InvalidateAvailabilityCacheAsync(slot.DoctorProfileId);
        return true;
    }

    // Checks whether the current user may manage a Doctor profile schedule.
    public async Task<bool> CanManageDoctorAsync(
        ClaimsPrincipal user,
        int doctorProfileId)
    {
        if (user.IsInRole("Admin"))
        {
            return true;
        }

        if (!user.IsInRole("Doctor"))
        {
            return false;
        }

        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        return !string.IsNullOrWhiteSpace(userId) &&
               await _context.DoctorProfiles.AnyAsync(doctor =>
                   doctor.Id == doctorProfileId &&
                   doctor.UserId == userId &&
                   doctor.IsActive);
    }

    // Checks whether a Doctor identity is working at a requested UTC date and time.
    public async Task<bool> IsDoctorAvailableAsync(
        string doctorUserId,
        DateTime appointmentDate)
    {
        var appointmentTime = TimeOnly.FromDateTime(appointmentDate);
        return await _context.DoctorAvailabilitySlots.AnyAsync(slot =>
            slot.DoctorProfile.UserId == doctorUserId &&
            slot.DoctorProfile.IsActive &&
            slot.IsActive &&
            slot.DayOfWeek == appointmentDate.DayOfWeek &&
            slot.StartTime <= appointmentTime &&
            appointmentTime < slot.EndTime);
    }

    // Maps a Doctor availability entity to its API response.
    private static DoctorAvailabilityResponse ToResponse(
        DoctorAvailability slot)
    {
        return new DoctorAvailabilityResponse(
            slot.Id,
            slot.DoctorProfileId,
            slot.DayOfWeek,
            slot.StartTime,
            slot.EndTime,
            slot.IsActive);
    }

    // Builds the stable Redis key used for one Doctor's weekly availability.
    private static string GetAvailabilityCacheKey(int doctorProfileId)
    {
        return $"doctor-availability:{doctorProfileId}";
    }

    // Removes cached availability immediately after a successful schedule write.
    private async Task InvalidateAvailabilityCacheAsync(int doctorProfileId)
    {
        if (_cache != null)
        {
            await _cache.RemoveAsync(GetAvailabilityCacheKey(doctorProfileId));
        }
    }
}
