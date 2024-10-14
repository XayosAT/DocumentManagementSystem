using DAL.Data;
using DAL.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Migrations und Datenbankerstellung anwenden
// using (var scope = app.Services.CreateScope())
// {
//     var context = scope.ServiceProvider.GetRequiredService<DocumentContext>();
//
//     // Verbindungstest zur Datenbank
//     try
//     {
//         Console.WriteLine("Versuche, eine Verbindung zur Datenbank herzustellen...");
//
//         // Warte, bis die Datenbank bereit ist
//         while (!context.Database.CanConnect())
//         {
//             Console.WriteLine("Datenbank ist noch nicht bereit, warte...");
//             Thread.Sleep(1000); // Warte 1 Sekunde
//         }
//
//         Console.WriteLine("Verbindung zur Datenbank erfolgreich.");
//
//         // Migrations anwenden und die Datenbank erstellen/aktualisieren
//         context.Database.EnsureCreated();
//         // context.Database.Migrate();
//         Console.WriteLine("Datenbankmigrationen erfolgreich angewendet.");
//     }
//     catch (Exception ex)
//     {
//         Console.WriteLine($"Fehler bei der Anwendung der Migrationen: {ex.Message}");
//     }
// }
