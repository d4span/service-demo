using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
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

// HttpClient für die Kommunikation mit AufgabenService
builder.Services.AddHttpClient("AufgabenAPI", client =>
{
    // API URL aus der Umgebungsvariable oder Fallback
    var apiUrl = builder.Configuration["AufgabenApiUrl"] ?? "http://localhost:5001";
    client.BaseAddress = new Uri(apiUrl);
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

// In-Memory Datenquelle für Prüfungen
List<Pruefung> pruefungen = new()
{
    new Pruefung 
    { 
        Id = 1, 
        Titel = "Grundlagen der Informatik", 
        AufgabenIds = new List<int> { 1, 2, 3 },
        Datum = DateTime.Now.AddDays(7),
        Zeitlimit = 15
    },
    new Pruefung 
    { 
        Id = 2, 
        Titel = "Programmierung mit C#", 
        AufgabenIds = new List<int> { 2, 3 },
        Datum = DateTime.Now.AddDays(14),
        Zeitlimit = 20
    }
};

// API-Endpunkte
app.MapGet("/api/pruefung", () =>
{
    return Results.Ok(pruefungen);
})
.WithName("GetPruefungen")
.WithOpenApi();

app.MapGet("/api/pruefung/{id}", (int id) =>
{
    var pruefung = pruefungen.FirstOrDefault(p => p.Id == id);
    if (pruefung == null)
    {
        return Results.NotFound();
    }
    return Results.Ok(pruefung);
})
.WithName("GetPruefungById")
.WithOpenApi();

app.MapGet("/api/pruefung/{id}/aufgaben", async (int id, IHttpClientFactory clientFactory) =>
{
    // Prüfung finden
    var pruefung = pruefungen.FirstOrDefault(p => p.Id == id);
    if (pruefung == null)
    {
        return Results.NotFound("Prüfung nicht gefunden");
    }

    // HttpClient für AufgabenService erstellen
    var client = clientFactory.CreateClient("AufgabenAPI");
    
    // Aufgaben für die Prüfung abrufen
    try
    {
        var alleAufgaben = await client.GetFromJsonAsync<List<Aufgabe>>("api/aufgaben");
        if (alleAufgaben == null)
        {
            return Results.Problem("Keine Aufgaben gefunden");
        }

        // Nur die Aufgaben zurückgeben, die in der Prüfung enthalten sind
        var pruefungsAufgaben = alleAufgaben.Where(a => pruefung.AufgabenIds.Contains(a.Id)).ToList();
        
        return Results.Ok(pruefungsAufgaben);
    }
    catch (Exception ex)
    {
        return Results.Problem($"Fehler beim Abrufen der Aufgaben: {ex.Message}");
    }
})
.WithName("GetAufgabenFuerPruefung")
.WithOpenApi();

app.MapPost("/api/pruefung", (Pruefung pruefung) =>
{
    // Neue ID festlegen
    pruefung.Id = pruefungen.Count > 0 ? pruefungen.Max(p => p.Id) + 1 : 1;
    
    // Prüfung zur Liste hinzufügen
    pruefungen.Add(pruefung);
    
    return Results.Created($"/api/pruefung/{pruefung.Id}", pruefung);
})
.WithName("CreatePruefung")
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

// Datenmodell für Prüfungen
public class Pruefung
{
    public int Id { get; set; }
    public string Titel { get; set; } = string.Empty;
    public List<int> AufgabenIds { get; set; } = new();
    public DateTime Datum { get; set; } = DateTime.Now;
    public int Zeitlimit { get; set; } = 30; // in Minuten
}