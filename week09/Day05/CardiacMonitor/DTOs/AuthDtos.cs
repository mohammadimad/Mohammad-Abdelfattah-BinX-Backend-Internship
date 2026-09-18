namespace CardiacMonitor.DTOs;

/// <summary>Public Patient registration details. Privileged role selection is not part of this contract.</summary>
public record RegisterRequest(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    DateTime DateOfBirth,
    string Gender,
    string ContactNumber);
/// <summary>Credentials for an existing Identity account.</summary>
public record LoginRequest(string Email, string Password);

//Record to hold both access and refresh tokens
/// <summary>An expired access token and its matching, unused refresh token.</summary>
public record TokenRequest(string AccessToken, string RefreshToken);

//returns the result of an authentication operation, including success status, message, and optional tokens
public record AuthResponse(
    bool IsSuccess,
    string Message,
    string? Token = null,
    string? RefreshToken = null
);
