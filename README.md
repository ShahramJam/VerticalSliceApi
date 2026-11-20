# VerticalSliceApi

## UML diagrams

Editable PlantUML diagrams for the architecture, domain model, and key request flows are in [`docs/diagrams`](docs/diagrams/):

- [Component architecture](docs/diagrams/component-architecture.puml)
- [Domain model](docs/diagrams/domain-model.puml)
- [Create student sequence](docs/diagrams/create-student-sequence.puml)
- [Enroll student sequence](docs/diagrams/enroll-student-sequence.puml)

Open the `.puml` files with a PlantUML-compatible renderer to view them.

En ASP.NET Core Web API för hantering av studenter, kurser och kursregistreringar. Projektet är utformat som ett lärandeexempel på **Vertical Slice Architecture** med **CQRS**, **MediatR** och EF Core.

## Teknisk översikt

- **.NET:** 10
- **Webb-API:** ASP.NET Core Minimal APIs
- **Arkitektur:** Vertical Slice Architecture
- **CQRS:** Kommandon och frågor separeras per feature
- **Mediatekod:** MediatR 12
- **Validering:** FluentValidation med MediatR pipeline behavior
- **Databas:** Microsoft SQL Server
- **ORM:** Entity Framework Core 10
- **API-dokumentation:** OpenAPI och Scalar

## Funktioner

- Skapa och lista studenter
- Skapa och lista kurser
- Registrera en student på en kurs
- Automatisk databasmigrering i utvecklingsmiljö
- Automatisk seedning av exempeldata
- Valideringsfel returneras som `400 Bad Request`

## Arkitektur

Applikationen är organiserad efter användningsfall, inte efter tekniska lager. Varje **vertical slice** innehåller den information som behövs för ett självständigt anrop: endpoint, kommando eller fråga, MediatR-handler, validering och eventuell responsemodell.

```text
src/VerticalSliceApi/
├── Domain/
│   └── Entities/              # Student och Course
├── Features/
│   ├── Common/                # Gemensamt endpointkontrakt och validering
│   ├── Students/              # Studentbaserade anrop
│   └── Courses/               # Kursbaserade anrop
├── Infrastructure/
│   └── Persistence/           # DbContext och dataseedning
├── Migrations/                # EF Core-migrationer
└── Program.cs                 # Dependency injection och applikationsstart
```

### CQRS med MediatR

Projektet använder MediatRs `IRequest` och `IRequestHandler<TRequest, TResponse>`:

- **Commands** ändrar data, till exempel att skapa en student eller kurs.
- **Queries** läser data och använder i läsrika endpoints `AsNoTracking()`.
- **Handlers** innehåller use-case-logiken för respektive slice.
- **Endpoints** skickar kommandon eller frågor via `ISender`.

Exempel på flödet för att skapa en kurs:

```text
HTTP POST
  -> CreateCourse.Endpoint
  -> CreateCourse.Command
  -> ValidationBehavior
  -> CreateCourse.Validator
  -> CreateCourse.Handler
  -> SchoolDbContext
  -> SQL Server
```

### Vertikala features

`Features/Students` och `Features/Courses` innehåller självständiga filer för varje användning. Till exempel:

- `CreateStudent.cs` — kommando, validator, handler och endpoint
- `GetStudents.cs` — fråga, responsemodell, handler och endpoint
- `EnrollStudent.cs` — registrering av student på kurs
- `CreateCourse.cs` — kommando, validator, handler och endpoint
- `GetCourses.cs` — fråga, responsemodell, handler och endpoint

### Validering

Alla FluentValidation-validatorer registreras automatiskt vid uppstart. `ValidationBehavior` kör före respektive MediatR-handler och kastar en `ValidationException` om indata är ogiltig.

API:t fångar undantaget och returnerar:

```json
{
  "title": "Validation Failed",
  "errors": [
    {
      "propertyName": "Email",
      "errorMessage": "'Email' is not a valid email address."
    }
  ]
}
```

## API-endpoints

### Studenter

| Metod | Endpoint | Beskrivning | Success |
|---|---|---|---|
| `GET` | `/api/students` | Hämtar alla studenter och deras kurser | `200 OK` |
| `POST` | `/api/students` | Skapar en ny student | `201 Created` |
| `POST` | `/api/students/{studentId}/courses/{courseId}` | Registrerar en student på en kurs | `204 No Content` |

#### Skapa student

```http
POST /api/students
Content-Type: application/json

{
  "firstName": "Anna",
  "lastName": "Andersson",
  "email": "anna.andersson@example.se"
}
```

Svar:

```json
{
  "id": 6
}
```

Valideringsregler:

- Förnamn: obligatoriskt, högst 100 tecken
- Efternamn: obligatoriskt, högst 100 tecken
- E-post: obligatorisk, giltig e-postadress, högst 200 tecken
- E-postadressen måste vara unik i databasen

### Kurser

| Metod | Endpoint | Beskrivning | Success |
|---|---|---|---|
| `GET` | `/api/courses` | Hämtar alla kurser och antal registrerade studenter | `200 OK` |
| `POST` | `/api/courses` | Skapar en ny kurs | `201 Created` |

#### Skapa kurs

```http
POST /api/courses
Content-Type: application/json

{
  "code": "NET505",
  "title": "Advanced .NET Architecture",
  "credits": 20
}
```

Svar:

```json
{
  "id": 5
}
```

Valideringsregler:

- Kurskod: obligatorisk, högst 20 tecken
- Kurskod normaliseras till versaler
- Kurskod måste vara unik i databasen
- Titel: obligatorisk, högst 200 tecken
- Poäng: ett heltal mellan 1 och 60

### Kursregistrering

```http
POST /api/students/1/courses/1
```

Registreringen returnerar `204 No Content` om båda entiteterna hittas. Om studenten eller kursen saknas returneras `404 Not Found`. Samma student–kurs-par läggs inte till två gånger.

## Databas

### Teknologi

- Microsoft SQL Server
- Entity Framework Core 10
- EF Core-migrationer
- Automatisk migrering i utvecklingsmiljö
- Async seeding via `UseAsyncSeeding()`

Standardanslutningen i `appsettings.json` är:

```json
{
  "ConnectionStrings": {
    "SchoolDbConnectionString": "Server=.;Database=SchoolDb;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

För lokalt SQL Server på Windows använder standardkonfigurationen Windows-autentisering och databasen `SchoolDb`.

### Modeller

#### Students

| Kolumn | Typ | Regel |
|---|---|---|
| `Id` | `int` | Primärnyckel, identity |
| `FirstName` | `nvarchar(100)` | Obligatorisk |
| `LastName` | `nvarchar(100)` | Obligatorisk |
| `Email` | `nvarchar(200)` | Obligatorisk, unik |
| `EnrolledAtUtc` | `datetime2` | Registreringstid i UTC |

#### Courses

| Kolumn | Typ | Regel |
|---|---|---|
| `Id` | `int` | Primärnyckel, identity |
| `Code` | `nvarchar(20)` | Obligatorisk, unik |
| `Title` | `nvarchar(200)` | Obligatorisk |
| `Credits` | `int` | Obligatorisk |

#### StudentCourses

Relationen mellan `Student` och `Course` är många-till-många. EF Core skapar automatiskt kopplingstabellen `StudentCourses` med referenser till båda entiteterna.

```text
Students ──< StudentCourses >── Courses
```

### Seeddata

Seedningen körs i anslutning till databaskonfigurationen och lägger bara till data om både kurs- och studenttabellen är tomma.

Exempeldata innehåller:

- 4 kurser
- 5 studenter
- Flera student–kurs-registreringar

## Köra applikationen

### Förutsättningar

- .NET 10 SDK
- Tillgänglig SQL Server-instans
- SQL Server Access via Windows Authentication eller en uppdaterad anslutningssträng

### Starta

```bash
dotnet restore
cd src/VerticalSliceApi
dotnet run
```

HTTP och HTTPS-profile från `launchSettings.json`:

- HTTP: `http://localhost:5136`
- HTTPS: `https://localhost:7059`

Vid första start i `Development`:

1. Applikationen skapar en DbContext-instans.
2. Migrations körs med `Database.MigrateAsync()`.
3. Saknad startdata seedas.
4. API-endpoints mappas.

### API-dokumentation

I utvecklingsmiljö exponeras:

- OpenAPI-schema: `/openapi/v1.json`
- Scalar UI: `/scalar/v1`

## Konfigurationsmiljöer

`appsettings.json` innehåller databaskonfigurationen för applikationen. `appsettings.Development.json` innehåller utvecklingsspecifik loggningskonfiguration.

För andra miljöer kan anslutningssträngen sättas genom miljövariabel:

```bash
ConnectionStrings__SchoolDbConnectionString="Server=...;Database=SchoolDb;..."
```

## Översikt över dataskiktet

- `SchoolDbContext` är enda EF Core DbContext i projektet.
- Feature-handlers använder direkt `SchoolDbContext` för denna demonstrationsapplikation.
- Domänklasserna kapslar in viktiga beteenden, exempelvis `Student.EnrollInCourse()`.
- Läsoperationer använder `AsNoTracking()` där spårning inte behövs.
- Skrivoperationer sparar ändringar genom `SaveChangesAsync()`.

## Projektstruktur

```text
.
├── README.md
├── VerticalSliceApi.slnx
└── src
    └── VerticalSliceApi
        ├── Domain
        ├── Features
        ├── Infrastructure
        ├── Migrations
        ├── Properties
        ├── Program.cs
        ├── appsettings.json
        └── appsettings.Development.json
```
