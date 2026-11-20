using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using CloneUZ.Api.Data;
using CloneUZ.Api.Infrastructure;
using CloneUZ.Api.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Postgres")));


builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
{
    var config = ConfigurationOptions.Parse(builder.Configuration.GetConnectionString("Redis")!);
    config.AbortOnConnectFail = false;
    return ConnectionMultiplexer.Connect(config);
});
builder.Services.AddSingleton<ISeatLockService, RedisSeatLockService>();

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<ISeatRepository, SeatRepository>();
builder.Services.AddScoped<ISeatMapService, SeatMapService>();
builder.Services.AddScoped<IHoldService, HoldService>();
builder.Services.AddScoped<IReleaseService, ReleaseService>();
builder.Services.AddScoped<IExpiredHoldCleaner, ExpiredHoldCleaner>();
builder.Services.AddHostedService<ExpiredHoldSweeper>();
builder.Services.AddOptions<ExpiredHoldSweeperOptions>()
    .BindConfiguration(ExpiredHoldSweeperOptions.Section)
    .ValidateDataAnnotations()
    .ValidateOnStart();
builder.Services.AddOptions<SeatHoldOptions>()
    .BindConfiguration(SeatHoldOptions.Section)
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddAuthentication(DevelopmentAuthHandler.SchemeName)
    .AddScheme<AuthenticationSchemeOptions, DevelopmentAuthHandler>(DevelopmentAuthHandler.SchemeName, null);
builder.Services.AddAuthorization();

builder.Services.AddRateLimiter(o =>
{
    o.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    o.AddPolicy("booking", http => RateLimitPartition.GetFixedWindowLimiter(
        http.Request.Headers["X-Debug-User"].FirstOrDefault() ?? http.Connection.RemoteIpAddress?.ToString() ?? "anon",
        _ => new FixedWindowRateLimiterOptions { PermitLimit = 10, Window = TimeSpan.FromSeconds(10) }));
});

builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    using var scope = app.Services.CreateScope();
    await scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.MigrateAsync();
    await DevDataSeeder.SeedAsync(scope.ServiceProvider.GetRequiredService<AppDbContext>());
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

app.MapControllers();

await app.RunAsync();
