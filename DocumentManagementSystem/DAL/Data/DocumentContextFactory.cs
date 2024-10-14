using DAL.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace DAL.Data
{
    public class DocumentContextFactory : IDesignTimeDbContextFactory<DocumentContext>
    {
        public DocumentContext CreateDbContext(string[] args)
        {
            // Assuming the REST project is the parent directory of DAL,
            // navigate to the REST folder where appsettings.json resides
            var basePath = Path.Combine(Directory.GetCurrentDirectory(), "..", "REST");

            // Build configuration to read from the REST project's appsettings.json
            var config = new ConfigurationBuilder()
                .SetBasePath(basePath)  // Point to the REST project's folder
                .AddJsonFile("appsettings.json")
                .Build();

            // Read the connection string from the appsettings.json in the REST project
            var connectionString = config.GetConnectionString("DefaultConnection");

            // Configure the DbContext with the connection string
            var optionsBuilder = new DbContextOptionsBuilder<DocumentContext>();
            optionsBuilder.UseNpgsql(connectionString);

            return new DocumentContext(optionsBuilder.Options);
        }
    }
}