namespace AuthService.Domain.ValueObjects;

public record JWTPayload(
    string SecretKey,
    string Issuer,
    string Audience,
    int ExpiresIn,
    string UserId,
    string Email);
