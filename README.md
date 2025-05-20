# Service-Demo mit .NET 8 und Blazor WebAssembly

Diese Demo zeigt die Implementierung von zwei Mikroservices mit .NET 8 und Blazor WebAssembly, die in Docker-Containern laufen. Sie bietet eine komplette Anwendung zur Verwaltung von Multiple-Choice-Aufgaben und Prüfungen.

## Projektstruktur

- **AufgabenService**: Verwaltet eine Liste von Multiple-Choice-Aufgaben
  - API: `/api/aufgaben` - Endpunkte für Aufgabendaten (CRUD-Operationen)
  - Client: Blazor WebAssembly-Komponente zur Anzeige und Verwaltung der Aufgaben

- **PruefungService**: Verwaltet Prüfungen
  - API: `/api/pruefung` - Endpunkte für Prüfungsdaten (CRUD-Operationen)
  - Client: Blazor WebAssembly-Komponente zur Verwaltung und Durchführung von Prüfungen

## Architektur

Das Projekt folgt einer Mikroservice-Architektur mit folgenden Komponenten:

```
┌───────────────────┐    ┌───────────────────┐
│                   │    │                    │
│  AufgabenService  │    │  PruefungService   │
│      Client       │    │      Client        │
│    (Port 5101)    │    │    (Port 5102)     │
│                   │    │                    │
└────────┬──────────┘    └────────┬───────────┘
         │                        │
         │ HTTP                   │ HTTP
         │                        │
┌────────▼──────────┐    ┌────────▼───────────┐
│                   │    │                     │
│  AufgabenService  │◄───┤   PruefungService   │
│       API         │    │        API          │
│   (Port 5001)     │    │     (Port 5002)     │
│                   │    │                     │
└───────────────────┘    └─────────────────────┘
```

- **Kommunikationsfluss**:
  - Der PruefungService.API kommuniziert mit dem AufgabenService.API, um Aufgabeninformationen abzurufen
  - Beide Clients kommunizieren unabhängig mit ihren jeweiligen APIs
  - Alle Komponenten laufen in separaten Docker-Containern im selben Netzwerk

- **Technologiestack**:
  - Backend: ASP.NET Core 8 Minimal APIs
  - Frontend: Blazor WebAssembly
  - Containerisierung: Docker
  - Netzwerk: Docker Compose Network

## Datenmodell

### AufgabenService

```
┌─────────────┐       ┌─────────────┐
│   Aufgabe   │       │   Antwort   │
├─────────────┤       ├─────────────┤
│ Id: int     │1     *│ Id: int     │
│ Frage: str  ├───────┤ Text: str   │
└─────────────┘       │ IstRichtig: │
                      │   bool      │
                      └─────────────┘
```

- **Aufgabe**:
  - `Id` (int): Eindeutige ID der Aufgabe
  - `Frage` (string): Text der Frage
  - `Antworten` (List<Antwort>): Liste der möglichen Antworten

- **Antwort**:
  - `Id` (int): Eindeutige ID der Antwort innerhalb der Aufgabe
  - `Text` (string): Text der Antwort
  - `IstRichtig` (bool): Gibt an, ob dies die richtige Antwort ist

### PruefungService

```
┌─────────────┐       ┌────────────────┐
│   Pruefung  │       │AufgabenService │
├─────────────┤       │    Aufgabe     │
│ Id: int     │       ├────────────────┤
│ Titel: str  │    ┌──┤ Id: int        │
│ AufgabenIds:│----┘  │ Frage: str     │
│   List<int> │       │ Antworten      │
│ Datum: date │       └────────────────┘
│ Zeitlimit:  │
│   int (min) │
└─────────────┘
```

- **Pruefung**:
  - `Id` (int): Eindeutige ID der Prüfung
  - `Titel` (string): Titel der Prüfung
  - `AufgabenIds` (List<int>): Liste der IDs der Aufgaben in der Prüfung
  - `Datum` (DateTime): Geplantes Datum der Prüfung
  - `Zeitlimit` (int): Zeitlimit für die Prüfung in Minuten

## API-Endpunkte

### AufgabenService.API

| Methode | Endpunkt             | Beschreibung                             |
|---------|----------------------|------------------------------------------|
| GET     | /api/aufgaben        | Alle Aufgaben abrufen                    |
| GET     | /api/aufgaben/{id}   | Eine spezifische Aufgabe abrufen         |
| POST    | /api/aufgaben        | Neue Aufgabe erstellen                   |
| PUT     | /api/aufgaben/{id}   | Bestehende Aufgabe aktualisieren         |
| DELETE  | /api/aufgaben/{id}   | Aufgabe löschen                          |

### PruefungService.API

| Methode | Endpunkt                      | Beschreibung                                |
|---------|-------------------------------|---------------------------------------------|
| GET     | /api/pruefung                 | Alle Prüfungen abrufen                      |
| GET     | /api/pruefung/{id}            | Eine spezifische Prüfung abrufen            |
| GET     | /api/pruefung/{id}/aufgaben   | Aufgaben einer Prüfung abrufen              |
| GET     | /api/aufgaben                 | Alle Aufgaben vom AufgabenService abrufen   |
| POST    | /api/pruefung                 | Neue Prüfung erstellen                      |
| PUT     | /api/pruefung/{id}            | Prüfungsdaten aktualisieren                 |
| PUT     | /api/pruefung/{id}/aufgaben   | Aufgaben einer Prüfung aktualisieren        |
| DELETE  | /api/pruefung/{id}            | Prüfung löschen                             |

## Features und Funktionalitäten

### AufgabenService
- Erstellen neuer Multiple-Choice-Aufgaben mit beliebig vielen Antwortoptionen
- Bearbeiten bestehender Aufgaben (Frage und Antworten)
- Löschen von Aufgaben
- Markieren der richtigen Antwort

### PruefungService
- Erstellen neuer Prüfungen mit Titel, Datum und Zeitlimit
- Zuweisen von Aufgaben zu Prüfungen
- Durchführen von Prüfungen mit Countdown-Timer
- Auswerten von Ergebnissen

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

### Aufgaben verwalten
1. Öffne den AufgabenService Client unter http://localhost:5101
2. Verwende "Neue Aufgabe erstellen", um eine neue Multiple-Choice-Frage hinzuzufügen
3. Füge mehrere Antwortoptionen hinzu und markiere die richtige Antwort
4. Speichere die Aufgabe

### Prüfungen verwalten und durchführen
1. Öffne den PruefungService Client unter http://localhost:5102
2. Erstelle eine neue Prüfung mit "Neue Prüfung erstellen"
3. Weise bestehende Aufgaben der Prüfung zu
4. Starte die Prüfung mit dem "Prüfung starten"-Button
5. Beantworte die Fragen innerhalb des Zeitlimits
6. Schließe die Prüfung ab, um die Ergebnisse zu sehen

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
- Die Daten werden nur im Arbeitsspeicher (In-Memory) gespeichert

## Erweiterungsmöglichkeiten

- Persistente Datenspeicherung mit einer Datenbank
- Benutzerauthentifizierung und Berechtigungen
- Detaillierte Auswertung von Prüfungsergebnissen
- Export/Import von Aufgaben und Prüfungen