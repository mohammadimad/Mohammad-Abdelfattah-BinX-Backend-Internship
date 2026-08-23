namespace CardiacMonitor.DTOs;

public record RegisterRequest(string Email, string Password, string Role);
public record LoginRequest(string Email, string Password);

//Record to hold both access and refresh tokens
public record TokenRequest(string AccessToken, string RefreshToken);

//returns the result of an authentication operation, including success status, message, and optional tokens
public record AuthResponse(
    bool IsSuccess,
    string Message,
    string? Token = null,
    string? RefreshToken = null
);