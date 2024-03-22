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
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using IHostingEnvironment = Microsoft.Extensions.Hosting.IHostingEnvironment;

namespace project
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; set; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthentication(option =>
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
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Jwt:Key"]))
                };
            });

            services.AddAuthorization();

            services.AddScoped<IAdmin, AdminServices>();
            services.AddScoped<IVehicle, VehicleServices>();

            services.AddDbContext<DataContext>(options =>
            {
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"));
            });

            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(options =>
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
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/error");
                app.UseHsts();
            }

            // Configuração do Swagger UI
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
            });

            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                #region Home
                endpoints.MapGet("/", () => Results.Json(new Home())).WithTags("Home").AllowAnonymous().WithTags("Home");
                #endregion

                #region Adm
                string GetTokenJwt(Admin admin)
                {
                    var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Jwt:Key"]));
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

                endpoints.MapPost("/admins/login", ([FromBody] LoginDTO loginDTO, IAdmin admin) =>
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

                endpoints.MapPost("/admins", ([FromBody] AdminDTO adminDTO, IAdmin admin) =>
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
                }).RequireAuthorization().WithTags("Adm");

                endpoints.MapGet("/admins", ([FromQuery] int? page, IAdmin admin) =>
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
                }).RequireAuthorization().WithTags("Adm");

                endpoints.MapGet("/admins/{id}", ([FromQuery] int id, IAdmin admin) =>
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
                }).RequireAuthorization().WithTags("Adm");
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

                endpoints.MapPost("/vehicles", ([FromBody] VehicleDTO vehicleDTO, IVehicle vehicle) =>
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
                }).RequireAuthorization().WithTags("Vehicles");

                endpoints.MapGet("/vehicles", ([FromQuery] int? page, IVehicle vehicle) =>
                {
                    var vehicles = vehicle.GetVehicles(page);

                    return Results.Ok(vehicles);
                }).RequireAuthorization().WithTags("Vehicles");

                endpoints.MapGet("/vehicle/{id}", ([FromRoute] int id, IVehicle vehicle) =>
                {
                    var vehicleInfo = vehicle.GetVehicle(id);

                    if (vehicleInfo == null)
                        return Results.NotFound();

                    return Results.Ok(vehicleInfo);
                }).RequireAuthorization().WithTags("Vehicles");

                endpoints.MapPut("/vehicles/{id}", ([FromRoute] int id, VehicleDTO vehicleDTO, IVehicle vehicle) =>
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
                }).RequireAuthorization().WithTags("Vehicles");

                endpoints.MapDelete("/vehicles/{id}", ([FromRoute] int id, IVehicle vehicle) =>
                {
                    var vehicleInfo = vehicle.GetVehicle(id);

                    if (vehicleInfo == null)
                        return Results.NotFound();

                    vehicle.DeleteVehicle(vehicleInfo);

                    return Results.NoContent();
                }).RequireAuthorization().WithTags("Vehicles");
                #endregion
            });
        }

    }
}
