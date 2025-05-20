# Service-Demo mit .NET 8 und Blazor WebAssembly

Diese Demo zeigt die Implementierung von zwei Mikroservices mit .NET 8 und Blazor WebAssembly, die in Docker-Containern laufen.

## Projektstruktur

- **AufgabenService**: Verwaltet eine Liste von Multiple-Choice-Aufgaben
  - API: `/api/aufgaben` - Endpunkt für Aufgabendaten
  - Client: Blazor WebAssembly Komponente zur Anzeige der Aufgaben

- **PruefungService**: Verwaltet Prüfungen
  - API: `/api/pruefung` - Endpunkt für Prüfungsdaten
  - Client: Blazor WebAssembly Komponente mit "Prüfung starten"-Button

## Voraussetzungen

- [Docker](https://www.docker.com/products/docker-desktop)
- [Docker Compose](https://docs.docker.com/compose/install/)

## Starten der Anwendung

1. Navigiere zum Hauptverzeichnis (wo sich die `docker-compose.yml`-Datei befindet)
2. Führe folgenden Befehl aus:

```bash
docker-compose up --build
```

3. Nach dem erfolgreichen Start sind die Services unter folgenden URLs erreichbar:
   - AufgabenService API: http://localhost:5001/api/aufgaben
   - AufgabenService Client: http://localhost:5101
   - PruefungService API: http://localhost:5002/api/pruefung
   - PruefungService Client: http://localhost:5102

## Nutzung

1. Öffne den AufgabenService Client unter http://localhost:5101, um die verfügbaren Aufgaben anzusehen.
2. Öffne den PruefungService Client unter http://localhost:5102, um eine Prüfung zu starten und Aufgaben zu bearbeiten.

## Dienste herunterfahren

Um alle Container zu stoppen, führe folgenden Befehl aus:

```bash
docker-compose down
```

## Technische Details

- Die Services laufen in separaten Docker-Containern
- Die Kommunikation zwischen den Services erfolgt über REST-APIs
- Die Frontends sind mit Blazor WebAssembly entwickelt
- Das Backend verwendet ASP.NET Core Minimal APIs