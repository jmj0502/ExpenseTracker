using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace UtilityLibraries;
public class JWTValidator
{
    public async Task<bool> IsValidAsync(JwtConfig jwtConfig)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var validationParameters = getTokenParams(jwtConfig);
        var validationResult = await tokenHandler.ValidateTokenAsync(jwtConfig.Token, validationParameters);
        return validationResult.IsValid;
    }

    public async Task<IDictionary<string,object>> GetClaimsAsync(JwtConfig jwtConfig)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var validationParameters = getTokenParams(jwtConfig);
        var validationResult = await tokenHandler.ValidateTokenAsync(jwtConfig.Token, validationParameters);
        if (!validationResult.IsValid)
        {
            throw new SecurityTokenException("Invalid token provided");
        }
        return validationResult.Claims;
    }

    private TokenValidationParameters getTokenParams(JwtConfig jwtConfig) 
    {
        return new TokenValidationParameters
        {
            ValidateLifetime = false,
            ValidateAudience = false,
            ValidateIssuer = false,
            ValidAudience = jwtConfig.Audience,
            ValidIssuer = jwtConfig.Issuer,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtConfig.SecretKey))
        };
    }
}