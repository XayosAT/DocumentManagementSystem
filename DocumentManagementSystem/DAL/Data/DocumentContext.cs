using DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace DAL.Data;
public sealed class DocumentContext(DbContextOptions<DocumentContext> options) : DbContext(options)
{
    public DbSet<Document>? DocumentItems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Manuelle Konfiguration der Tabelle
        modelBuilder.Entity<Document>(entity =>
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