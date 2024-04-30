using Amazon.Runtime.Internal.Auth;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AuthService.Domain.ValueObjects;

namespace AuthService.Domain;

public class UsersCore
{
    private readonly ILogger<UsersCore> _logger;
    public UsersCore(ILogger<UsersCore> logger)
    {
        _logger = logger;
    }

    public string HashPassword(string plainPassword)
    {
        const int SALT_WORK_FACTOR = 10;
        var hashSalt = BCrypt.Net.BCrypt.GenerateSalt(SALT_WORK_FACTOR);
        return BCrypt.Net.BCrypt.HashPassword(plainPassword, hashSalt); 
    }

    public bool VerifyPassword(
        string plainPassword, string hashedPassword)
    {
        return BCrypt.Net.BCrypt.EnhancedVerify(
            plainPassword, hashedPassword);
    }

    public string GenerateJWT(JWTPayload payload)
    {
        var key = Encoding.UTF8.GetBytes(payload.SecretKey);
        var simmetricKey = new SymmetricSecurityKey(key);
        var signedCredentials = new SigningCredentials(
            simmetricKey, SecurityAlgorithms.HmacSha256Signature);
        var claims = new ClaimsIdentity(new []
        {
            new Claim(JwtRegisteredClaimNames.Sid, payload.UserId),
            new Claim(JwtRegisteredClaimNames.Email, payload.Email),
        });
        var expiresIn = DateTime.UtcNow.AddMinutes(payload.ExpiresIn);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = claims,
            Expires = expiresIn,
            Issuer = payload.Issuer,
            SigningCredentials = signedCredentials,
        };
        var tokenHandler = new JwtSecurityTokenHandler();
        var tokenDescription = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(tokenDescription);
    }

    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }
}
