namespace AuthService.Application.ValueObjects;

public record SignedUser(
    string UserId, string Email, string AccessToken, string RefreshToken);
