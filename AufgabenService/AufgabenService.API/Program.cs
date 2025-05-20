using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// CORS konfigurieren
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
        policy =>
        {
            policy.WithOrigins("http://localhost:5101", "http://localhost:5102", "http://aufgaben-client", "http://pruefung-client")
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
});

// Add services to the container.
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
app.UseCors();

// In-Memory Datenquelle für Aufgaben
List<Aufgabe> aufgaben = new()
{
    new Aufgabe
    {
        Id = 1,
        Frage = "Was ist die Hauptstadt von Deutschland?",
        Antworten = new List<Antwort>
        {
            new() { Id = 1, Text = "Berlin", IstRichtig = true },
            new() { Id = 2, Text = "München", IstRichtig = false },
            new() { Id = 3, Text = "Hamburg", IstRichtig = false },
            new() { Id = 4, Text = "Köln", IstRichtig = false }
        }
    },
    new Aufgabe
    {
        Id = 2,
        Frage = "Welche Programmiersprache wird für ASP.NET Core verwendet?",
        Antworten = new List<Antwort>
        {
            new() { Id = 1, Text = "Java", IstRichtig = false },
            new() { Id = 2, Text = "C#", IstRichtig = true },
            new() { Id = 3, Text = "Python", IstRichtig = false },
            new() { Id = 4, Text = "JavaScript", IstRichtig = false }
        }
    },
    new Aufgabe
    {
        Id = 3,
        Frage = "Wie viele Bits hat ein Byte?",
        Antworten = new List<Antwort>
        {
            new() { Id = 1, Text = "4", IstRichtig = false },
            new() { Id = 2, Text = "8", IstRichtig = true },
            new() { Id = 3, Text = "16", IstRichtig = false },
            new() { Id = 4, Text = "32", IstRichtig = false }
        }
    }
};

// API-Endpunkte
app.MapGet("/api/aufgaben", () =>
{
    return Results.Ok(aufgaben);
})
.WithName("GetAufgaben")
.WithOpenApi();

app.MapGet("/api/aufgaben/{id}", (int id) =>
{
    var aufgabe = aufgaben.FirstOrDefault(a => a.Id == id);
    if (aufgabe == null)
    {
        return Results.NotFound();
    }
    return Results.Ok(aufgabe);
})
.WithName("GetAufgabeById")
.WithOpenApi();

// Neue Endpunkte für die Aufgabenverwaltung
app.MapPost("/api/aufgaben", (AufgabeErstellen aufgabeData) =>
{
    // Neue ID berechnen
    var neueId = aufgaben.Count > 0 ? aufgaben.Max(a => a.Id) + 1 : 1;
    
    // Neue Aufgabe erstellen
    var aufgabe = new Aufgabe
    {
        Id = neueId,
        Frage = aufgabeData.Frage,
        Antworten = new List<Antwort>()
    };
    
    // IDs für die Antworten generieren
    for (int i = 0; i < aufgabeData.Antworten.Count; i++)
    {
        var antwortData = aufgabeData.Antworten[i];
        aufgabe.Antworten.Add(new Antwort
        {
            Id = i + 1,
            Text = antwortData.Text,
            IstRichtig = antwortData.IstRichtig
        });
    }
    
    // Aufgabe zur Liste hinzufügen
    aufgaben.Add(aufgabe);
    
    return Results.Created($"/api/aufgaben/{aufgabe.Id}", aufgabe);
})
.WithName("CreateAufgabe")
.WithOpenApi();

app.MapPut("/api/aufgaben/{id}", (int id, AufgabeAktualisieren aufgabeData) =>
{
    // Aufgabe finden
    var aufgabe = aufgaben.FirstOrDefault(a => a.Id == id);
    if (aufgabe == null)
    {
        return Results.NotFound();
    }
    
    // Aufgabe aktualisieren
    aufgabe.Frage = aufgabeData.Frage;
    
    // Antworten komplett ersetzen
    aufgabe.Antworten.Clear();
    for (int i = 0; i < aufgabeData.Antworten.Count; i++)
    {
        var antwortData = aufgabeData.Antworten[i];
        aufgabe.Antworten.Add(new Antwort
        {
            Id = i + 1,
            Text = antwortData.Text,
            IstRichtig = antwortData.IstRichtig
        });
    }
    
    return Results.Ok(aufgabe);
})
.WithName("UpdateAufgabe")
.WithOpenApi();

app.MapDelete("/api/aufgaben/{id}", (int id) =>
{
    // Aufgabe finden
    var aufgabe = aufgaben.FirstOrDefault(a => a.Id == id);
    if (aufgabe == null)
    {
        return Results.NotFound();
    }
    
    // Prüfen, ob die Aufgabe gelöscht werden kann
    // - In einer realen Anwendung würde man hier prüfen, ob die Aufgabe noch verwendet wird
    
    // Aufgabe löschen
    aufgaben.Remove(aufgabe);
    
    return Results.NoContent();
})
.WithName("DeleteAufgabe")
.WithOpenApi();

app.Run();

// Datenmodell für Aufgaben
public class Aufgabe
{
    public int Id { get; set; }
    public string Frage { get; set; } = string.Empty;
    public List<Antwort> Antworten { get; set; } = new();
}

public class Antwort
{
    public int Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public bool IstRichtig { get; set; }
}

// DTOs für die API
public class AufgabeErstellen
{
    public string Frage { get; set; } = string.Empty;
    public List<AntwortErstellen> Antworten { get; set; } = new();
}

public class AntwortErstellen
{
    public string Text { get; set; } = string.Empty;
    public bool IstRichtig { get; set; }
}

public class AufgabeAktualisieren
{
    public string Frage { get; set; } = string.Empty;
    public List<AntwortErstellen> Antworten { get; set; } = new();
}