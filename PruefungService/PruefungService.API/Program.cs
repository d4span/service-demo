using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using System.Text.Json;

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

// HttpClient für die Kommunikation mit dem AufgabenService
builder.Services.AddHttpClient("AufgabenService", client =>
{
    // Die URL des AufgabenService aus der Umgebungsvariable oder Fallback
    var aufgabenApiUrl = builder.Configuration["AufgabenApiUrl"] ?? "http://localhost:5001";
    client.BaseAddress = new Uri(aufgabenApiUrl);
});

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

// Datenmodelle
public class Pruefung
{
    public int Id { get; set; }
    public string Titel { get; set; } = string.Empty;
    public int AktuelleAufgabeId { get; set; }
    public bool IstGestartet { get; set; }
    public DateTime? StartZeit { get; set; }
}

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

// In-Memory Datenquelle für Prüfungen
List<Pruefung> pruefungen = new()
{
    new Pruefung
    {
        Id = 1,
        Titel = "Allgemeinwissen Prüfung",
        AktuelleAufgabeId = 1,
        IstGestartet = false,
        StartZeit = null
    },
    new Pruefung
    {
        Id = 2,
        Titel = "Programmierung Prüfung",
        AktuelleAufgabeId = 2,
        IstGestartet = false,
        StartZeit = null
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

app.MapPost("/api/pruefung/{id}/start", async (int id, IHttpClientFactory httpClientFactory) =>
{
    var pruefung = pruefungen.FirstOrDefault(p => p.Id == id);
    if (pruefung == null)
    {
        return Results.NotFound();
    }

    pruefung.IstGestartet = true;
    pruefung.StartZeit = DateTime.Now;

    // Zugriff auf den AufgabenService, um die aktuelle Aufgabe zu holen
    try
    {
        var client = httpClientFactory.CreateClient("AufgabenService");
        var aufgabe = await client.GetFromJsonAsync<Aufgabe>($"api/aufgaben/{pruefung.AktuelleAufgabeId}");
        
        // Neue Antwort mit Prüfungs- und Aufgabeninformationen
        var response = new
        {
            Pruefung = pruefung,
            AktuelleAufgabe = aufgabe
        };
        
        return Results.Ok(response);
    }
    catch (Exception ex)
    {
        return Results.Problem($"Fehler beim Abrufen der Aufgabe: {ex.Message}");
    }
})
.WithName("StartPruefung")
.WithOpenApi();

app.Run();