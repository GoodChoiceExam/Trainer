using FitLife.Trainer.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using NLog;
using NLog.Web;

var logger = LogManager.Setup().LoadConfigurationFromFile("NLog.config").GetCurrentClassLogger();

try
{
    MongoDB.Bson.Serialization.BsonSerializer.RegisterSerializer(
        new MongoDB.Bson.Serialization.Serializers.GuidSerializer(MongoDB.Bson.GuidRepresentation.Standard));

    var builder = WebApplication.CreateBuilder(args);

    builder.Logging.ClearProviders();
    builder.Host.UseNLog();

    var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "fitlife-identity";
    var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "fitlife";
    var jwksUrl = builder.Configuration["Jwt:JwksUrl"] ?? "http://localhost:5244/.well-known/jwks.json";

    builder.Services
        .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwtIssuer,
                ValidateAudience = true,
                ValidAudience = jwtAudience,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKeyResolver = (_, _, _, _) => JwksSigningKeyResolver.GetSigningKeys(jwksUrl)
            };

            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {
                    logger.Warn(context.Exception, "JWT authentication failed");
                    return Task.CompletedTask;
                }
            };
        });

    builder.Services.AddAuthorization();

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("Frontend", policy =>
            policy.WithOrigins("http://localhost:5271")
                  .AllowAnyHeader()
                  .AllowAnyMethod());
    });

    builder.Services.AddSingleton<ITrainerService, TrainerService>();
    builder.Services.AddControllers()
        .AddJsonOptions(options =>
            options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter()));
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    var app = builder.Build();

    app.UseSwagger();
    app.UseSwaggerUI();

    app.UseCors("Frontend");
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapGet("/healthz", () => Results.Ok(new { status = "healthy" }));
    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    logger.Fatal(ex, "Application failed to start");
    throw;
}
finally
{
    LogManager.Shutdown();
}

static class JwksSigningKeyResolver
{
    private static readonly HttpClient Client = new();
    private static DateTime _expiresAt = DateTime.MinValue;
    private static IReadOnlyCollection<SecurityKey> _cachedKeys = [];

    public static IEnumerable<SecurityKey> GetSigningKeys(string jwksUrl)
    {
        if (_cachedKeys.Count > 0 && DateTime.UtcNow < _expiresAt)
            return _cachedKeys;

        var json = Client.GetStringAsync(jwksUrl).GetAwaiter().GetResult();
        var jwks = new JsonWebKeySet(json);

        _cachedKeys = jwks.Keys.Cast<SecurityKey>().ToArray();
        _expiresAt = DateTime.UtcNow.AddMinutes(5);
        return _cachedKeys;
    }
}