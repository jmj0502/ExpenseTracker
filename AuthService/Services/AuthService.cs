using AuthService;
using AuthService.Application;
using AuthService.Application.ValueObjects;
using Grpc.Core;
using Microsoft.IdentityModel.Tokens;

namespace AuthService.Services;

public class AuthenticationService : Auth.AuthBase
{
    private readonly ILogger<AuthenticationService> _logger;
    private readonly UserService _userService;

    public AuthenticationService(ILogger<AuthenticationService> logger, UserService userService) 
    {
        _logger = logger;
        _userService = userService;
    }

    public override async Task<SignInReply> SignIn(
        SignInRequest request, ServerCallContext context)
    {
        _logger.LogInformation($"Received AuthMessage: {request}");
        var signedUser = await _userService.SignInAsync(
            new User(request.Email, request.Password));
        if (signedUser.Email.IsNullOrEmpty())
        {
            throw new RpcException(
                new Status(StatusCode.InvalidArgument, "Invalid credentials"));
        }
        return new SignInReply { 
            UserId = signedUser.UserId,
            Email = signedUser.Email,
            AccessToken = signedUser.AccessToken, 
            RefreshToken = signedUser.RefreshToken, 
        };
    }

    public async override Task<SignUpReply> SignUp(
        SignUpRequest request, ServerCallContext context)
    {
        _logger.LogInformation($"Received SignUp Message: {request}");
        var result = await _userService.RegisterAsync(
            new NewUser(request.FirstName, request.LastName, request.Password, request.Email));
        return new SignUpReply { Success = result.Success };
    }

    public async override Task<VerifyReply> Verify(
        VerifyRequest request, ServerCallContext context)
    {
        _logger.LogInformation($"Received Verify MessageL {request}");
        var verificationResult = await _userService.VerifyAsync(request.Token);
        return new VerifyReply { Success = verificationResult.Success };
    }
}
