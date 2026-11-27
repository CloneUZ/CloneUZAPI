using CloneUZ.BLL.Interfaces;
using CloneUZ.BLL.Services;
using CloneUZ.DAL;
using CloneUZ.DAL.Interfaces;
using CloneUZ.DAL.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddControllers();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDBContext>(options => 
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"),
        b=>b.MigrationsAssembly("CloneUZ.DAL")));

builder.Services.AddScoped<IRouteRepository, RouteRepository>();
builder.Services.AddScoped<ITrainRunRepository, TrainRunRepository>();

builder.Services.AddScoped<IRouteService, RouteService>();
builder.Services.AddScoped<ITrainRunService,TrainRunService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
