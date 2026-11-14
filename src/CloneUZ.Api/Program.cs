using System.Text.Json.Serialization;
using CloneUZ.Api.Data;
using CloneUZ.Api.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Postgres")));

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
    await DevDataSeeder.SeedAsync(scope.ServiceProvider.GetRequiredService<AppDbContext>());
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

await app.RunAsync();
