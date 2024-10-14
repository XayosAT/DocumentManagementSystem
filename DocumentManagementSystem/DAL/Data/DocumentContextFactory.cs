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
            var optionsBuilder = new DbContextOptionsBuilder<DocumentContext>();
            
            /*
            // Load configuration
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("dalsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile("dalsettings.Development.json", optional: true, reloadOnChange: true)
                .Build();
            */
            
            // Set the connection string
            //var connectionString = configuration.GetConnectionString("DefaultConnection");
            optionsBuilder.UseNpgsql("Host=localhost;Port=5430;Database=dms_db;Username=dms_user;Password=dms_password"); // Use Npgsql for PostgreSQL

            return new DocumentContext(optionsBuilder.Options);
        }
    }
}