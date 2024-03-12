using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using project.Domain.DTOs;
using project.Domain.Entities;
using project.Domain.Enums;
using project.Domain.Interfaces;
using project.Domain.ModelViews;
using project.Domain.Services;
using project.Infra.Db;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Runtime.Intrinsics.Arm;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks.Dataflow;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(option =>
{
    option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateActor = false,
        ValidateIssuer = false,
        ValidateLifetime = true,
        ValidateAudience = false,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
});

builder.Services.AddAuthorization();

builder.Services.AddScoped<IAdmin, AdminServices>();
builder.Services.AddScoped<IVehicle, VehicleServices>();

builder.Services.AddDbContext<DataContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Insert your JWT token"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new String[]{}
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/", () => Results.Json(new Home())).WithTags("Home").AllowAnonymous().WithTags("Home");

#region Adm
string GetTokenJwt(Admin admin)
{
    var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]));
    var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

    var claims = new List<Claim>()
    {
        new Claim("Email", admin.Email),
        new Claim("Profile", admin.Profile),
        new Claim(ClaimTypes.Role, admin.Profile)
    };

    var token = new JwtSecurityToken(
        claims: claims,
        expires: DateTime.Now.AddMinutes(5),
        signingCredentials: credentials
        );

    return new JwtSecurityTokenHandler().WriteToken(token);
}

app.MapPost("/admins/login", ([FromBody] LoginDTO loginDTO, IAdmin admin) =>
{
    var adm = admin.Login(loginDTO);
    if (adm != null)
    {
        string token = GetTokenJwt(adm);
        return Results.Ok(new AuthorizedAdmin
        {
            Email = adm.Email,
            Profile = adm.Profile,
            Token = token
        });
    }
    else
    {
        return Results.Unauthorized();
    }
}).AllowAnonymous().WithTags("Adm");

app.MapPost("/admins", ([FromBody] AdminDTO adminDTO, IAdmin admin) =>
{
    var messages = new ValidationError
    {
        Messages = new List<string>()
    };

    if (string.IsNullOrEmpty(adminDTO.Email))
        messages.Messages.Add("The make cannot be null!");

    if (string.IsNullOrEmpty(adminDTO.Password))
        messages.Messages.Add("The make cannot be null!");

    if (adminDTO.Profile == null)
        messages.Messages.Add("The make cannot be null!");

    if (messages.Messages.Count > 0)
        return Results.BadRequest(messages);

    var adm = new Admin
    {
        Email = adminDTO.Email,
        Password = adminDTO.Password,
        Profile = adminDTO.Profile.ToString() ?? Profile.Editor.ToString(),
    };

    admin.PostAdmin(adm);

    return Results.Created($"/admins/{adm.Id}", new AdminModelViews
    {
        Id = adm.Id,
        Email = adm.Email,
        Profile = adm.Profile,
    });
}).RequireAuthorization().RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" }).WithTags("Adm");

app.MapGet("/admins", ([FromQuery] int? page, IAdmin admin) =>
{
    var adms = new List<AdminModelViews>();
    var admins = admin.GetAdmins(page);

    foreach (var adm in admins)
    {
        adms.Add(new AdminModelViews
        {
            Id = adm.Id,
            Email = adm.Email,
            Profile = adm.Profile,
        });
    }
    return Results.Ok(adms);
}).RequireAuthorization().RequireAuthorization(new AuthorizeAttribute { Roles = "Adm"}).WithTags("Adm");

app.MapGet("/admins/{id}", ([FromQuery] int id, IAdmin admin) =>
{
    var adminInfo = admin.GetAdmin(id);

    if (adminInfo == null)
        return Results.NotFound();

    return Results.Ok(new AdminModelViews
    {
        Id = adminInfo.Id,
        Email = adminInfo.Email,
        Profile = adminInfo.Profile,
    });
}).RequireAuthorization().RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" }).WithTags("Adm");
#endregion

#region Vehicles
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
}).RequireAuthorization().RequireAuthorization(new AuthorizeAttribute { Roles = "Adm,Editor" }).WithTags("Vehicles");

app.MapGet("/vehicles", ([FromQuery] int? page, IVehicle vehicle) =>
{
    var vehicles = vehicle.GetVehicles(page);

    return Results.Ok(vehicles);
}).RequireAuthorization().RequireAuthorization(new AuthorizeAttribute { Roles = "Adm,Editor" }).WithTags("Vehicles");

app.MapGet("/vehicle/{id}", ([FromRoute] int id, IVehicle vehicle) =>
{
    var vehicleInfo = vehicle.GetVehicle(id);

    if (vehicleInfo == null)
        return Results.NotFound();

    return Results.Ok(vehicleInfo);
}).RequireAuthorization().RequireAuthorization(new AuthorizeAttribute { Roles = "Adm,Editor" }).WithTags("Vehicles");

app.MapPut("/vehicles/{id}", ([FromRoute] int id, VehicleDTO vehicleDTO, IVehicle vehicle) =>
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
}).RequireAuthorization().RequireAuthorization(new AuthorizeAttribute { Roles = "Adm,Editor" }).WithTags("Vehicles");

app.MapDelete("/vehicles/{id}", ([FromRoute] int id, IVehicle vehicle) =>
{
    var vehicleInfo = vehicle.GetVehicle(id);

    if (vehicleInfo == null)
        return Results.NotFound();

    vehicle.DeleteVehicle(vehicleInfo);

    return Results.NoContent();
}).RequireAuthorization().RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" }).WithTags("Vehicles");
#endregion

app.UseAuthentication();
app.UseAuthorization();

app.Run();