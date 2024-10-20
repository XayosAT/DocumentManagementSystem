using DocumentManagementSystem;
using DocumentManagementSystem.DTOs;
using DAL.Validators;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using DAL.Repositories;
using DAL.Data;
using DocumentManagementSystem.Entities;
using DocumentManagementSystem.Validators;

var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
builder.Services.AddControllers();

// Add FluentValidation services to the container.
builder.Services.AddControllers()
    .AddFluentValidation(config =>
    {
        config.RegisterValidatorsFromAssemblyContaining<DocumentValidator>();
        config.RegisterValidatorsFromAssemblyContaining<DocumentDTOValidator>();
    });

// Register FluentValidation manually if needed
builder.Services.AddScoped<IValidator<Document>, DocumentValidator>();

// Configure the PostgreSQL database connection using Entity Framework Core
builder.Services.AddDbContext<DocumentContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));


// Register the DocumentRepository and IDocumentRepository for dependency injection
builder.Services.AddScoped<IDocumentRepository, DocumentRepository>();

// Configure HttpClient for communication within the application
builder.Services.AddHttpClient("DAL", client =>
{
    client.BaseAddress = new Uri("http://localhost:8081");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

// Register AutoMapper to map between DTOs and entities
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// Configure Kestrel to listen on port 8081 for Docker
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(8081); // Set the port to 8081
});

// Configure CORS to allow all origins, methods, and headers
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policyBuilder =>
    {
        policyBuilder.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

// Add services to support API endpoint exploration (Swagger or OpenAPI)
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

// Apply CORS policy
app.UseCors();

// Apply authentication (optional, depending on your security setup)
app.UseAuthentication();

// Map the API controllers
app.MapControllers();

// Start the application
app.Run();
