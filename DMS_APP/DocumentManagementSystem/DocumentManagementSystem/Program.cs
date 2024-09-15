// include SampleDocument class
using DocumentManagementSystem;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure Kestrel to listen on port 8081
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(8081); // Set the port to 8081 for Docker
});

var app = builder.Build();

// Remove HTTPS redirection for local development in Docker
// app.UseHttpsRedirection();

// define document class

// create a list of example documents
var documents = new List<SampleDocument>
{
    new SampleDocument { Id = 1, Name = "Document 1", Path = "/documents/1" },
    new SampleDocument { Id = 2, Name = "Document 2", Path = "/documents/2" },
    new SampleDocument { Id = 3, Name = "Document 3", Path = "/documents/3" }
};

app.MapGet("/", () => documents);

// Direct the user to the upload page
app.MapGet("/upload", () => "Upload a file");

app.Run();