namespace CardiacMonitor.DTOs;

public record RegisterRequest(
    string Email,
    string Password,
    string Role,
    string? FirstName = null,
    string? LastName = null,
    DateTime? DateOfBirth = null,
    string? Gender = null,
    string? ContactNumber = null
);

public record LoginRequest(string Email, string Password);

public record TokenRequest(string AccessToken, string RefreshToken);

public record AuthResponse(
    bool IsSuccess,
    string Message,
    string? Token = null,
    string? RefreshToken = null
);