using Microsoft.EntityFrameworkCore;
using SharedData.EntitiesDAL;

namespace DAL.Data;
public class DocumentContext : DbContext
{
    public DbSet<DocumentDAL>? DocumentItems { get; set; }
    
    public DocumentContext(DbContextOptions<DocumentContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Manuelle Konfiguration der Tabelle
        modelBuilder.Entity<DocumentDAL>(entity =>
        {
            entity.ToTable("Documents");  // Setzt den Tabellennamen

            entity.HasKey(e => e.Id);  // Setzt den Primärschlüssel

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100);  // Konfiguriert den "Name"-Spalten

            entity.Property(e => e.Path);  // Konfiguriert die "IsComplete"-Spalte
            
            entity.Property(e => e.FileType);
        });

        base.OnModelCreating(modelBuilder);
    }
}