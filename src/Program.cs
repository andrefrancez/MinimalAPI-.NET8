using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using project.Domain.DTOs;
using project.Domain.Entities;
using project.Domain.Interfaces;
using project.Domain.ModelViews;
using project.Domain.Services;
using project.Infra.Db;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IAdmin, AdminServices>();
builder.Services.AddScoped<IVehicle, VehicleServices>();

builder.Services.AddDbContext<DataContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/", () => Results.Json(new Home()));

app.MapPost("/login", ([FromBody]LoginDTO loginDTO, IAdmin admin) =>
{
    if (admin.Login(loginDTO) != null)
        return Results.Ok("Authorized");
    else
        return Results.Unauthorized();
});

// Vehicle

app.MapPost("/vehicles", ([FromBody] VehicleDTO vehicleDTO, IVehicle vehicle) =>
{
    var vehicleData = new Vehicle
    {
        Name = vehicleDTO.Name,
        Make = vehicleDTO.Make,
        ModelYear = vehicleDTO.ModelYear,
        Color = vehicleDTO.Color,
        Description = vehicleDTO.Description,
    };

    vehicle.PostVehicle(vehicleData);

    return Results.Created($"/vehicles/{vehicleData.Id}", vehicleData);
});

app.Run();