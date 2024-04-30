using AuthService.Services;
using AuthService.Domain;
using AuthService.Application;
using AuthService.Infrastructure.Emails;
using AuthService.Infrastructure.Database.Settings;
using AuthService.Domain.Settings;

var builder = WebApplication.CreateBuilder(args);

// Additional configuration is required to successfully run gRPC on macOS.
// For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682

// Add services to the container.
builder.Services.AddGrpc();
builder.Services.Configure<Settings>(
    builder.Configuration);
builder.Services.Configure<MailSettings>(
    builder.Configuration.GetSection("MailSettings"));
builder.Services.Configure<JWTSettings>(
    builder.Configuration.GetSection("Jwt"));
builder.Services.AddTransient<MailManager>();
builder.Services.AddSingleton<Connection>();
builder.Services.AddTransient<UsersCore>();
builder.Services.AddSingleton<UserService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<AuthenticationService>();
app.MapGet("/", async (UserService userService) =>  await userService.GetAsync());

app.Run();
