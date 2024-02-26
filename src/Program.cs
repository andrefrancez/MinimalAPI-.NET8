using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using project.Domain.DTOs;
using project.Domain.Interfaces;
using project.Domain.Services;
using project.Infra.Db;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IAdmin, AdminServices>();

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

app.MapPost("/login", ([FromBody]LoginDTO loginDTO, IAdmin admin) =>
{
    if (admin.Login(loginDTO) != null)
        return Results.Ok("Authorized");
    else
        return Results.Unauthorized();
});

app.Run();