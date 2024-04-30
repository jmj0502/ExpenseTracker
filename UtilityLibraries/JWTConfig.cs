namespace UtilityLibraries;

public record JwtConfig(
    string Token, string SecretKey, string Issuer, string Audience);

