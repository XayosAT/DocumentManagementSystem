// include SampleDocument class
using DocumentManagementSystem;
using DocumentManagementSystem.DTOs;
using DocumentManagementSystem.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

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

// register dbcontext and repository
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));
});
builder.Services.AddScoped<IUserRepository, UserRepository>();

// Configure Kestrel to listen on port 8081
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(8081); // Set the port to 8081 for Docker
});

var app = builder.Build();

// Remove HTTPS redirection for local development in Docker
// app.UseHttpsRedirection();

// create a list of example documents
var documents = new List<SampleDocument>
{
    new SampleDocument { Id = 1, Name = "Document 1", Path = "/documents/1" },
    new SampleDocument { Id = 2, Name = "Document 2", Path = "/documents/2" },
    new SampleDocument { Id = 3, Name = "Document 3", Path = "/documents/3" }
};

app.UseCors();

app.MapGet("/", () => documents);

app.MapControllers();

app.Run();