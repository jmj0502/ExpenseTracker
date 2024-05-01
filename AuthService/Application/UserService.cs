using AuthService.Application.Repositories;
using AuthService.Application.ValueObjects;
using AuthService.Domain;
using AuthService.Domain.Settings;
using AuthService.Domain.ValueObjects;
using AuthService.Infrastructure.Database.Models;
using AuthService.Infrastructure.Database.Settings;
using AuthService.Infrastructure.Emails;
using Microsoft.Extensions.Options;
using UtilityLibraries;
using MongoDB.Driver;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace AuthService.Application;
public class UserService
{
    private readonly UsersCore _usersCore;
    private readonly ILogger<IUsersService> _logger;
    private readonly MongoClient _client;
    private readonly IMongoCollection<Users> _usersCollection;
    private readonly MailManager _mailManager;
    private readonly JWTSettings _jwtSettings;

    public UserService(
        Connection connection,
        ILogger<IUsersService> logger,
        UsersCore usersCore,
        IConfiguration appSettings,
        MailManager mailManager,
        IOptions<JWTSettings> jwtSettings)
    {

        var mongoClient = connection.Client;
        var mongoDatabase = mongoClient.GetDatabase(
           appSettings.GetValue<string>("AuthenticationDatabase:DatabaseName"));
        _logger = logger;
        _usersCore = usersCore;
        _usersCollection = mongoDatabase.GetCollection<Users>(
           appSettings.GetValue<string>("AuthenticationDatabase:Collection"));
        _jwtSettings = jwtSettings.Value;
        _client = mongoClient; 
        _mailManager = mailManager;
    }

    public async Task<List<Users>> GetAsync()
    {
        return await (
            await _usersCollection.FindAsync(Builders<Users>.Filter.Empty)).ToListAsync();
    }

    public async Task<SuccessResult> RegisterAsync(NewUser newUser)
    {
        // using var session = await _client.StartSessionAsync();
        // session.StartTransaction();
        try
        {
            var hashedPassword = _usersCore.HashPassword(newUser.Password);
            const int VERIFICATION_TOKEN_EXPIRATION_TIME_IN_MINUTES = 100;
            var verificationJwt = _usersCore.GenerateJWT(new JWTPayload(
                _jwtSettings.VerificationSecretKey,
                _jwtSettings.Issuer,
                _jwtSettings.Audience,
                VERIFICATION_TOKEN_EXPIRATION_TIME_IN_MINUTES,
                "", // Ignore this, in a real project a different implementation should be used.
                newUser.Email
            ));
            await _usersCollection.InsertOneAsync(
                new Users { 
                    Email = newUser.Email,
                    Password = hashedPassword,
                    FirstName = newUser.FirstName,
                    LastName  = newUser.LastName,
                    VerificationToken = verificationJwt,
                    HasValidVerificationToken = true,
                }
            );
            var url = $"https://localhost:5001/verify/{verificationJwt}";
            var _ = Task.Run(async () => await _mailManager.SendMailAsync(new MailData(
                    newUser.Email,
                    newUser.FirstName,
                    "Confirm Your Account",
                    $"<h1>Account Confirmation Mail</h1></br><p>Hi! {newUser.FirstName}</p></br><p>Open the following link to verify your account!</p></br><a style=\"text-decoration: none; background-color: teal; color: white; padding: .5rem; border-radius: 25px; font-weight: bold;\" href=\"{url}\">Verify</a>"
                )));
            var result = true;
        //  await session.CommitTransactionAsync();
            return new SuccessResult(result);
        }
        catch (Exception e)
        {
        //  await session.AbortTransactionAsync();
            _logger.LogError($"[UsersService-RegisterUser/Error]: {e.Message}");
            var result = false;
            return new SuccessResult(result);
        }
    }

    public async Task<SuccessResult> VerifyAsync(string verificationJWT)
    {
        try
        {
            var tokenValidator = new JWTValidator();
            var jwtConfig = new JwtConfig(
                verificationJWT, _jwtSettings.VerificationSecretKey, _jwtSettings.Issuer, _jwtSettings.Audience);
            var isValidToken = await tokenValidator.IsValidAsync(jwtConfig);
            if (!isValidToken)
                return new SuccessResult(isValidToken);

            var claims = await tokenValidator.GetClaimsAsync(jwtConfig);
            var filterBuilder = Builders<Users>.Filter;
            var mailFilter = filterBuilder.Eq(user => user.Email, claims[ClaimTypes.Email]);
            var tokenFilter = filterBuilder.Eq(user => user.HasValidVerificationToken, true);
            var verificationFilter = filterBuilder.Eq(user => user.IsActive, false);
            var filter = filterBuilder.And(mailFilter, tokenFilter, verificationFilter);
            var update = Builders<Users>.Update
                .Set("isActive", true)
                .Set("HasValidVerificationToken", false);

            var updateResult = await _usersCollection.UpdateOneAsync(
                filter,
                update
            );
            if (!updateResult.IsAcknowledged)
                return new SuccessResult(false);

            return new SuccessResult(updateResult.ModifiedCount > 0);
        }
        catch (SecurityTokenException e)
        {
            _logger.LogError($"[UserService-VerifyTokenAsync/Error]:{e.Message}");
            return new SuccessResult(false);
        }
    }

    public async Task<SignedUser> SignInAsync(User user)
    {
        var filterBuilder = Builders<Users>.Filter;
        var emailFilter = filterBuilder.Eq(u => u.Email, user.Email);
        var enabledFilter = filterBuilder.Eq(u => u.IsActive, true);
        var userFilter = filterBuilder.And(emailFilter, enabledFilter);
        var existingUser = await (
            await _usersCollection.FindAsync(userFilter)).FirstOrDefaultAsync();
        if (existingUser is null)
            return new SignedUser("", "", "", "");
        var hasValidPassword = _usersCore.VerifyPassword(
            user.Password, existingUser.Password);
        if (!hasValidPassword)
            return new SignedUser("", "", "", "");
        const int ACCESS_TOKEN_EXPIRATION_TIME_IN_MINS = 15;
        var accessToken = _usersCore.GenerateJWT(new JWTPayload(
            _jwtSettings.SecretKey,
            _jwtSettings.Issuer,
            _jwtSettings.Audience,
            ACCESS_TOKEN_EXPIRATION_TIME_IN_MINS,
            existingUser.Id,
            existingUser.Email
        ));
        var refreshToken = _usersCore.GenerateRefreshToken();
        return new SignedUser(
            existingUser.Id, existingUser.Email, accessToken, refreshToken);
    }
}
