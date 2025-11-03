# Implementation Plan: Personalized Recommendations (RECO)

**Branch**: `1-personalized-recommendations` | **Date**: 2025-11-02 | **Spec**: specs/1-personalized-recommendations/spec.md

## Summary

This plan describes a concrete implementation approach for RECO using the requested stack:
- .NET (C#) with ASP.NET Core
- EF Core (migrations)
- PostgreSQL (canonical store)
- xUnit for tests
- Docker Compose for local environment
- GitHub Actions for CI
- TMDb API as external data source via an adapter and a background worker

Primary outcome: a demo-ready Razor UI showing Movies, Series, Recommendations, My Lists, and Title Details while the repository clearly demonstrates DDD layering, automated TMDb ingestion, tests, and CI gates.

## Technical Context

Language/Version: .NET 8+ (C# 12+ recommended)
Primary Dependencies: ASP.NET Core, Entity Framework Core, Npgsql, xUnit, FluentAssertions, Moq/Mocking library, OpenTelemetry (optional)
Storage: PostgreSQL (docker-compose local + CI integration job)
Testing: xUnit (unit), integration tests using Docker Compose (Postgres), optional Playwright or Selenium for E2E smoke
Target: Server-side web application with background worker

### Constitution Check (gates)
- DDD folder structure MUST be present in repo (Domain, Application, Infrastructure, API) and used by code and tests.
- EF Core Migrations MUST be created for any schema changes and migration verification included in CI.
- TMDb ingestion MUST be implemented as a background worker (IHostedService) with integration tests that demonstrate upserts to Postgres; no admin UI to add titles.
- Coverage gate: Domain + Application layers MUST achieve >= 80% coverage (CI job enforces this).

If any gate is violated the plan MUST include a justification and a migration path to compliance.

## High-level architecture (layers & data flow)

Architecture diagram (textual):

User Browser (Razor Views) → API / Controllers (Presentation)
  ↓
Application Layer (Use-cases: Commands/Queries, DTOs, Handlers)
  ↓(depends on abstractions)
Domain Layer (Entities, Aggregates, ValueObjects, Domain Services, Repository Interfaces)
  ↓(interface implementations)
Infrastructure Layer (EF Core Repositories, TMDbAdapter, Caching, External APIs)
  ↕
PostgreSQL (Canonical store)

Background worker (IHostedService) lives in Infrastructure/Worker project and calls TMDbAdapter → Application/Domain repositories to upsert Titles/Availability.

Observability: Logs & metrics emitted from API and Worker (OpenTelemetry-compatible). Traces propagate via correlation IDs.

## Concrete DDD folder -> code project mapping

Recommended solution layout (solution-level):

- src/
  - RECO.sln
  - src/RECO.Domain/
    - RECO.Domain.csproj
    - Aggregates/
    - Entities/
    - ValueObjects/
    - Events/
    - Interfaces/   # repository interfaces, external adapter interfaces
    - Services/     # domain services
  - src/RECO.Application/
    - RECO.Application.csproj
    - Commands/
    - Queries/
    - DTOs/
    - Handlers/
    - Services/   # application services implementing use-cases
    - Mapping/
  - src/RECO.Infrastructure/
    - RECO.Infrastructure.csproj
    - Persistence/
      - EF/ (DbContext, Migrations)
      - Repositories/ (EF implementations)
    - TMDbClient/ (TMDbAdapter, DTOs, rate-limit handling)
    - Worker/ (IHostedService implementation)
    - Configuration/
    - Observability/ (OpenTelemetry wiring)
  - src/RECO.API/
    - RECO.API.csproj
    - Controllers/
    - Views/ (.cshtml)
    - wwwroot/
    - Middleware/
  - tests/
    - RECO.Domain.Tests/
    - RECO.Application.Tests/
    - RECO.Infrastructure.IntegrationTests/
    - RECO.Api.E2E (optional smoke tests)
  - tools/
    - Seed/ (seed runner to trigger TMDb sync)

Notes:
- Keep Domain project framework-agnostic: no EF Core or ASP.NET references.
- Application depends on Domain only (and defines interfaces for persistence where needed).
- Infrastructure depends on Application and Domain and wires concrete implementations.

## Database schema essentials (entities & important fields)

This is a minimal schema mapping and recommended EF entities. Use EF Core code-first with migrations.

- Users
  - Id (GUID)
  - Email (string, unique)
  - PasswordHash (string)
  - DisplayName (string)
  - CreatedAt (timestamp)

- Profiles
  - Id (GUID)
  - UserId (FK Users)
  - Preferences summary (optional JSON or normalized tables)
  - CreatedAt

- Preferences (normalized approach)
  - PreferenceId
  - ProfileId
  - Type (enum: Genre|Actor|Director)
  - Value (string or foreign key to lookup)  // alternatively use join tables per preference type

- Genres (lookup)
  - Id (int TMDb id)
  - Name

- Titles
  - Id (GUID internal) or TMDbId (int) as canonical external id
  - TMDbId (int) — unique constraint
  - Type (enum: Movie|Series)
  - Title
  - OriginalTitle
  - Synopsis (text)
  - PosterUrl
  - BackdropUrl
  - ReleaseDate
  - Runtime
  - MetadataJson (optional additional TMDb fields)
  - CreatedAt, UpdatedAt

- TitleGenres (many-to-many)
  - TitleId
  - GenreId

- Availability (platform vendors)
  - Id
  - TitleId (FK)
  - ProviderName
  - CountryCode
  - Url
  - AvailabilityType (stream/rent/buy)

- Ratings
  - Id
  - TitleId
  - Source (IMDb, RottenTomatoes, TMDb, App)
  - Value (float)
  - UpdatedAt

- Reviews
  - Id
  - TitleId
  - UserId
  - Rating (int)
  - Text
  - CreatedAt

- UserLists
  - Id
  - UserId
  - Name
  - CreatedAt

- ListItems
  - Id
  - ListId
  - TitleId
  - AddedAt

- TMDbSyncRecords (audit)
  - Id
  - TMDbId
  - LastSyncedAt
  - Status (Success/Failed)
  - ErrorMessage

Indexes & Constraints
- Unique index on Titles(TMDbId)
- FK indexes for UserId, TitleId lookups
- Partitioning not required for initial demo

## Background ingestion worker design

Project: src/RECO.Infrastructure/Worker

Responsibilities
- Periodically (configurable schedule) fetch TMDb changes (popular, updated, details endpoints)
- Map TMDb DTOs to Domain Entities via TMDbAdapter (mapping service)
- Upsert Titles, Genres, Availability, external ratings into PostgreSQL via repositories
- Track TMDbSyncRecords for processed items and checkpointing
- Respect TMDb rate limits with a token-bucket or leaky-bucket strategy and exponential backoff retry policy
- Expose metrics for sync success/failure, last run duration, items processed

Implementation details
- Implement IHostedService (BackgroundService) that schedules sync runs.
- Use a TMDbAdapter in Infrastructure that wraps HTTP client calls and exposes typed DTOs.
- TMDbAdapter should implement:
  - GetPopularMovies(page)
  - GetPopularSeries(page)
  - GetMovieDetails(tmdbId)
  - GetTvDetails(tmdbId)
  - GetExternalIds(tmdbId)
- Rate limiting helper: central TMDbHttpClient that enforces concurrency and delay; use a singleton IHttpClientFactory configured for TMDb base address and keep-alive.
- Persist changes in transactions using EF Core; upsert by searching by TMDbId and updating fields.
- For heavy sync tasks consider batching and background worker partitioning by ranges to avoid long-running DB transactions.
- Provide a CLI/tool `tools/Seed/SeedRunner` to perform an initial full sync for first-day setup.

Error handling & retries
- Use Polly policies (retry with jitter and circuit-breaker) around HTTP calls and DB upserts.
- On persistent failure, record TMDbSyncRecords with error message and continue (do not block entire run).

## Repositories & Adapter patterns

Repository approach
- Define repository interfaces in Domain or Application (e.g., ITitleRepository, IUserRepository). These are used by Application layer use-cases.
- Provide EF Core implementations in Infrastructure.Persistences.Repositories.
- Repositories should expose transaction-friendly methods and unit-of-work if needed (or rely on DbContext per request with explicit transactions where necessary).

TMDb Adapter
- TMDbAdapter in Infrastructure/TMDbClient implements an interface (ITMDbClient) declared in Application/Interfaces or Domain.Interfaces depending on use.
- Adapter responsibilities: HTTP calls, DTO deserialization, rate-limit handling, basic normalization (e.g., unify date formats).
- Mapping layer converts DTOs → Domain Entities (Factories used here for complex aggregate creation).

Factory Method & Singleton
- Use Factory Method to create platform Availability objects when mapping TMDb provider responses into domain Availability aggregates.
- Use a single configured HttpClient instance per external API (TMDb) via IHttpClientFactory and DI (effectively singleton-managed by DI container) to avoid socket exhaustion. Provide a configuration provider singleton for app-wide config where appropriate (avoid global mutable state).

Observer & Strategy
- Observer: implement Domain Events (e.g., TitleAddedEvent, RecommendationUpdatedEvent) and subscribe handlers in Infrastructure (e.g., replay to analytics). Use a lightweight mediator (e.g., MediatR) or a hand-rolled DomainEventDispatcher within the Application layer.
- Strategy: recommendation algorithms are pluggable strategies implementing IRecommendationStrategy. Provide at least two strategies:
  - PreferenceBasedStrategy (baseline): matches user preferences (genres/actors/directors) with popularity boosts
  - CollaborativeStubStrategy: a placeholder strategy that can be replaced with collaborative filtering later
- The RecommendationService composes strategies with a StrategySelector (or combine results by weights).

## Authentication & Security

- For demo: ASP.NET Identity is recommended for user management (password hashing, user stores) — integrates well with EF Core. Alternatively, implement a minimal JWT authentication with secure password hashing (ASP.NET Core IDataProtection, BCrypt/Argon2 library) if Identity is too heavy for demo.
- Secure password hashing: use the Identity default (PBKDF2) or a modern library for Argon2.
- Secrets handling: read TMDB_API_KEY and DATABASE_URL from environment variables; configure GitHub Actions to provide secrets via GitHub Secrets.
- Ensure migration-generated connection strings are not committed and that local `.env` files are gitignored.

## Testing strategy

Unit tests
- RECO.Domain.Tests: pure domain logic, value object invariants, aggregate behavior
- RECO.Application.Tests: handlers, command/query logic, orchestration, using mocks for repositories and adapters

Integration tests
- RECO.Infrastructure.IntegrationTests: run against a Docker Compose Postgres instance (use Testcontainers or spin up Postgres via docker compose in CI). Tests for TMDb ingestion upserts, EF Core migrations, and repository implementations.
- Use stubbed TMDb HTTP responses (wiremock or in-memory HTTP message handler) for deterministic behavior in CI.

E2E / Smoke tests
- Minimal E2E tests that start API and worker (or use Docker Compose) and assert key flows: user registration + preferences, TMDb sync upsert, home page renders with recommendations.

Coverage
- Use coverlet to collect coverage in CI, enforce >= 80% on Domain + Application via a coverage job.

Local dev & debugging
- Use Docker Compose to run API + Postgres + optional worker or run projects separately using `dotnet run`.

## DevOps & CI

Docker Compose
- docker-compose.yml for local development: services: postgres, reco-api, reco-worker (optional). Mount local source for iterative development or run via builds.

EF Core Migrations
- Migrations stored in RECO.Infrastructure.Persistence/EF/Migrations. Developers generate migrations with `dotnet ef migrations add` and apply with `dotnet ef database update` or via `dotnet run` startup migration command.
- CI workflow includes a migrations verification step: build and run `dotnet ef migrations script` or run a migrations-check tool to ensure migrations compile and apply.

GitHub Actions (suggested jobs)
- name: CI
  runs-on: ubuntu-latest
  jobs:
    - build: restore & build
    - unit-tests: run unit tests and publish coverage
    - integration-tests: (runs with docker-compose) run integration tests against Postgres and the worker; run migrations verification
    - coverage-gate: fail if Domain+Application coverage < 80%

Deployment
- Provide a simple Dockerfile for API and Worker. For demonstration, use Docker Compose.

## Local seeding / first day setup

- Provide `tools/Seed/SeedRunner` that triggers a limited TMDb sync (configurable pages) to seed initial Titles.
- Add `scripts/first-run.ps1` or `first-run.sh` to:
  - copy `.env.example` to `.env`
  - `docker compose up -d postgres`
  - run migrations
  - run `dotnet run --project tools/Seed/SeedRunner --sync-initial`

## Milestones & Task groups (step-by-step)

Milestone 0 — Foundation (blocking)
- T0.1 Create solution + projects: Domain, Application, Infrastructure, API, Worker, tools/Seed
- T0.2 Setup Docker Compose and Postgres dev environment
- T0.3 Configure DI container, logging, configuration, and OpenTelemetry basics
- T0.4 Add EF Core DbContext and initial migrations scaffold
- T0.5 Add basic GitHub Actions skeleton

Milestone 1 — Domain & Application core
- T1.1 Implement Domain entities: User, Profile, Title, List, Review
- T1.2 Implement repository interfaces (ITitleRepository, IUserRepository, IListRepository)
- T1.3 Implement Application handlers for registration, preferences submission, list management, browsing queries
- T1.4 Unit tests for Domain and Application (aim to approach coverage target)

Milestone 2 — Infrastructure & Persistence
- T2.1 Implement EF Core persistence (DbContext, entity mappings)
- T2.2 Implement EF-based repository implementations
- T2.3 Implement TMDbAdapter (HTTP client wrapper) and DTO mappings
- T2.4 Implement Worker (IHostedService) wiring to adapter + repositories
- T2.5 Integration tests for ingestion and upserts

Milestone 3 — UI & Presentation
- T3.1 Implement Razor Views: Home hero, Movies, Series, Recommendations, My Lists, Title Details
- T3.2 Implement Controllers and view models; wire Application handlers
- T3.3 Implement basic client-side assets in wwwroot for styling
- T3.4 E2E smoke tests for main flows

Milestone 4 — Quality & CI enforcement
- T4.1 Configure coverage reporting and coverage gate
- T4.2 Finalize migrations verification in CI
- T4.3 Add security checks (secret scanning, dependency scanning)
- T4.4 Add observability dashboards and log sampling

Milestone 5 — Polish & demo
- T5.1 Seed sample data via SeedRunner and document Quickstart
- T5.2 Prepare demo script and verify that the DDD layering and tests are evident
- T5.3 Final code cleanup and documentation (docs/ARCHITECTURE.md)

## Risks & Mitigations

- Rate limits & TMDb API availability: mitigate with robust retry, backoff, and record-level checkpointing and partial syncs.
- Large volume of titles: implement batching and incremental sync; keep sync windows short.
- Tests flakiness: use deterministic stubbed TMDb responses and isolated Postgres test instances in CI.

## Deliverables (from plan)

- RECO.sln with projects for Domain, Application, Infrastructure, API, Worker, tests, and tools/Seed
- Docker Compose and sample `.env.example`
- EF Core migrations and migration verification in CI
- TMDbAdapter and Worker with integration tests demonstrating upserts
- Razor demo UI with the required pages
- GitHub Actions workflows enforcing tests, migrations, and coverage gates
- docs/ARCHITECTURE.md and Quickstart with first-day checklist

---

**Plan ready for Phase 0 research & Phase 1 design**

Location: specs/1-personalized-recommendations/plan.md

Constitution gates: ensure DDD folders exist and repo organization matches mapping above before Phase 0 research starts.
