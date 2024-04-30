using MimeKit.Cryptography;

namespace AuthService.Domain.Settings;

public class JWTSettings
{
    public string SecretKey {  get; set; }
    public string VerificationSecretKey { get; set; }
    public string Audience {  get; set; }
    public string Issuer { get; set; }
}
