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