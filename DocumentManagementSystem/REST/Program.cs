// include SampleDocument class
using DocumentManagementSystem;
using DocumentManagementSystem.DTOs;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;

builder.Services.AddControllers();

// Configure CORS and enable all origins
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// register automapper
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// Configure Kestrel to listen on port 8081
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(8081); // Set the port to 8081 for Docker
});

builder.Services.AddHttpClient("DAL", client =>
{
    client.BaseAddress = new Uri("http://localhost:8081");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

var app = builder.Build();

app.UseCors();
// app.Urls.Add("http://*:8080");
app.UseHttpsRedirection();
app.UseAuthentication();
app.MapControllers();

app.Run();