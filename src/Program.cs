using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using project.Domain.DTOs;
using project.Domain.Entities;
using project.Domain.Interfaces;
using project.Domain.ModelViews;
using project.Domain.Services;
using project.Infra.Db;
using System.Threading.Tasks.Dataflow;

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

app.MapGet("/", () => Results.Json(new Home())).WithTags("Home");

app.MapPost("/login", ([FromBody]LoginDTO loginDTO, IAdmin admin) =>
{
    if (admin.Login(loginDTO) != null)
        return Results.Ok("Authorized");
    else
        return Results.Unauthorized();
}).WithTags("Adm");

// Vehicle

ValidationError validationDTO(VehicleDTO vehicleDTO)
{
    var messagesValidation = new ValidationError
    {
        Messages = new List<string>()
    };

    if (string.IsNullOrEmpty(vehicleDTO.Name))
        messagesValidation.Messages.Add("The name cannot be null!");

    if (string.IsNullOrEmpty(vehicleDTO.Make))
        messagesValidation.Messages.Add("The make cannot be null!");

    if (vehicleDTO.ModelYear < 1900)
        messagesValidation.Messages.Add("The vehicle's model year needs to be above 1900");

    if (string.IsNullOrEmpty(vehicleDTO.Color))
        messagesValidation.Messages.Add("The color cannot be null!");

    return messagesValidation;
}

app.MapPost("/vehicles", ([FromBody] VehicleDTO vehicleDTO, IVehicle vehicle) =>
{
    var messages = validationDTO(vehicleDTO);
    if (messages.Messages.Count > 0)
        return Results.BadRequest(messages);

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
}).WithTags("Vehicles");

app.MapGet("/vehicles", ([FromQuery]int? page, IVehicle vehicle) =>
{
    var vehicles = vehicle.GetVehicles(page);

    return Results.Ok(vehicles);
}).WithTags("Vehicles");

app.MapGet("/vehicle/{id}", ([FromRoute]int id, IVehicle vehicle) =>
{
    var vehicleInfo = vehicle.GetVehicle(id);

    if (vehicleInfo == null)
        return Results.NotFound();

    return Results.Ok(vehicleInfo);
}).WithTags("Vehicles");

app.MapPut("/vehicles/{id}", ([FromRoute]int id, VehicleDTO vehicleDTO, IVehicle vehicle) =>
{
    var vehicleInfo = vehicle.GetVehicle(id);
    if (vehicleInfo == null)
        return Results.NotFound();

    var messages = validationDTO(vehicleDTO);
    if (messages.Messages.Count > 0)
        return Results.BadRequest(messages);

    vehicleInfo.Name = vehicleDTO.Name;
    vehicleInfo.Make = vehicleDTO.Make;
    vehicleInfo.ModelYear = vehicleDTO.ModelYear;
    vehicleInfo.Color = vehicleDTO.Color;
    vehicleInfo.Description = vehicleDTO.Description;

    vehicle.UpdateVehicle(vehicleInfo);

    return Results.Ok(vehicleInfo);
}).WithTags("Vehicles");

app.MapDelete("/vehicles/{id}", ([FromRoute]int id, IVehicle vehicle) =>
{
    var vehicleInfo = vehicle.GetVehicle(id);

    if (vehicleInfo == null)
        return Results.NotFound();

    vehicle.DeleteVehicle(vehicleInfo);

    return Results.NoContent();
}).WithTags("Vehicles");

app.Run();