# 📂 Woche 2: Advanced C# & ASP.NET Core Foundations

## 📝 Wochenübersicht
Woche 2 stellt einen wichtigen architektonischen Meilenstein im **BinX Tech Backend Development Internship Program** dar. Diese Woche schlug erfolgreich die Brücke zwischen **Phase 1 (Advanced C# Type Systems & Concurrency)** und **Phase 2 (ASP.NET Core Web API Foundations)**. 

Die Ergebnisse demonstrieren die Beherrschung von parametrisierten generischen Constraints, komplexer Datenumstrukturierung mit fortgeschrittenem LINQ, Thread-Pool-Optimierung für parallele Prozesse, Orchestrierung benutzerdefinierter Middleware-Pipelines und loser Kopplung durch Dependency Injection (DI).

---

## 📂 Ordnerstruktur Woche 2 (Flat Architecture)

Der Arbeitsbereich ist sauber als modulare Flat-Architecture strukturiert, wobei jedes Verzeichnis ein eigenständiges Visual Studio-Projekt enthält:

```text
Week02/
├── README.md                  # Diese umfassende Zusammenfassung von Woche 2
│
├── Day01/                     # Tag 01: Generics & Collection Interfaces
│   ├── Day01.sln              # Visual Studio Solution
│   ├── Day01.csproj           # C#-Projektdatei
│   ├── Program.cs             # Generisches Repository & Swap-Utility-Logik
│   └── README.md              # Dokumentation für Tag 01
│
├── Day02/                     # Tag 02: Advanced LINQ & Deferred Execution
│   ├── Day02.sln
│   ├── Day02.csproj
│   ├── Program.cs             # GroupBy, Join, SelectMany & Deferred Lab
│   └── README.md              # Dokumentation für Tag 02
│
├── Day03/                     # Tag 03: Async/Await Concurrency Basics
│   ├── Day03.sln
│   ├── Day03.csproj
│   ├── Program.cs             # Task.WhenAll & CancellationToken Lab
│   └── README.md              # Dokumentation für Tag 03
│
└── Day04/                     # Konsolidiertes Web-API-Projekt für Tag 04 & Tag 05
    ├── Day04.sln
    ├── Day04.csproj           # Ausgerichtetes Target Framework: .NET 9.0
    ├── Program.cs             # DI-Registrierungen, Custom Middleware & Minimal APIs
    ├── README.md              # Integrierte Dokumentation für Tag 04 & Tag 05
    ├── Controllers/           
    │   └── BooksController.cs # Controller mit Constructor Injection
    └── Services/              
        ├── IBookService.cs    # Service Contract Interface
        └── BookService.cs     # Konkreter Datenbank-Simulationsdienst
```

## 🗓️ Wöchentlicher Fortschritt & Lernerfolge

### 🔹 Modul 1: Generics & Interfaces (Tag 01)
* **Fokus**: Eliminierung von Boxing/Unboxing-Overhead, Durchsetzung der Typensicherheit zur Compile-Zeit und Anwendung generischer Constraints (`where T : class`).
* **Ergebnisse**: Erstellung eines generischen `Repository<T>` und Implementierung von Read-Only-Collections (`IReadOnlyList<T>`), um unbefugte externe Änderungen zu verhindern.

### 🔹 Modul 2: Advanced LINQ & Execution Models (Tag 02)
* **Fokus**: Datenumstrukturierung mittels `GroupBy` und `Join`, Verflachung geschachtelter Collections durch `SelectMany` und Analyse von Performance-Engpässen bei vorzeitiger Materialisierung.
* **Ergebnisse**: Entwicklung von Abfragen zur Demonstration von Deferred vs. Immediate Execution sowie Mapping relationaler Abfragen.

### 🔹 Modul 3: Concurrent Async Operations (Tag 03)
* **Fokus**: Beherrschung des Task-based Asynchronous Pattern (TAP), Analyse des Thread-Pool-Task-Schedulings, Implementierung paralleler Ausführung via `Task.WhenAll` und Nutzung kooperativer Stornierung.
* **Ergebnisse**: Entwicklung eines asynchronen Workflows, der die API-Ladelatenz aus mehreren Quellen um 50 % reduzierte, sowie sicheres Abbrechen langlaufender Tasks via `CancellationToken`.

### 🔹 Modul 4 & 5: ASP.NET Core Foundations, Middleware & DI (Tag 04 & 05)
* **Fokus**: Bereitstellung von Web-API-Hosting-Pipelines, Vergleich von Controllern vs. Minimal APIs, Orchestrierung der Middleware-Pipeline und Implementierung loser Kopplung mittels Constructor Dependency Injection.
* **Ergebnisse**: Entwicklung einer integrierten Web-API mit 4 aktiven Endpunkten (getestet in Postman), geschützt durch eine benutzerdefinierte Logging-Middleware und vollständig aufgelöst über einen als `AddScoped` registrierten Dienst.

---

## ⚙️ Technical Stack

* **Sprache & Runtime**: C# 12, .NET 9.0 SDK
* **Web Framework**: ASP.NET Core (Controllers & Minimal APIs)
* **API-Tests & Dokumentation**: Swagger UI, Postman
* **Versionsverwaltung**: Git & GitHub

---
*Erstellt von [Dein Name] als Teil des BinX Tech Backend Internship Programs.*
