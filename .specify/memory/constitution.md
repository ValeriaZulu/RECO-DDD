<!--
Sync Impact Report

Version change: none -> 1.0.0

Modified principles:
- SOLID (explicitly enumerated and made testable)
- CleanArchitecture+DDD (folder layout and enforcement language added)
- Patterns (list expanded with usage notes)

Added sections:
- Governance metadata (version, ratification, last amended)
- CI expectations and coverage gating
- First-day developer setup checklist

Removed sections:
- none

Templates requiring updates:
- .specify/templates/plan-template.md : ⚠ pending (update "Constitution Check" to reference RECO constitution gates and enforce DDD structure + EF migrations)
- .specify/templates/spec-template.md : ⚠ pending (ensure success criteria and testing sections reference coverage and architecture proof)
- .specify/templates/tasks-template.md : ⚠ pending (ensure foundational tasks include EF Core migrations, TMDb worker, and DDD folder creation)
- .specify/templates/checklist-template.md : ⚠ pending (add first-day developer checklist variant for RECO)
- .specify/templates/agent-file-template.md : ⚠ pending (regenerate to include new architecture rules)

Follow-up TODOs:
- TODO(CONSTITUTION_RATIFICATION): confirm ratification stakeholders and commit to versioning cadence if governance requires a different semantic bump policy.
-->

# RECO Project Constitution

*(Used by Spec-Kit — format: human-readable Markdown with YAML-like sections for clarity)*

```yaml
# RECO - Spec Kit Constitution
# Purpose: project governing principles, code & architecture standards for RECO (movie/series recommender)

metadata:
  constitution_version: 1.0.0
  ratification_date: 2025-11-02
  last_amended_date: 2025-11-02

project:
  name: RECO
  description: >
    Academic web application that recommends movies & series personalized to users.
    Automatically syncs catalog from TMDb into a PostgreSQL database and serves content via ASP.NET Core (C#).
  license: MIT

principles:
  - name: SOLID
    details:
      - Single Responsibility (SRP): "Each class or module MUST have one, and only one, reason to change. Keep domain models focused on domain behavior. Tests MUST assert single responsibility where practical."
      - Open/Closed (OCP): "Modules MUST be open for extension via abstractions (interfaces / protected extension points) and closed for modification. Prefer extension through new behavior rather than changing existing tested logic."
      - Liskov Substitution (LSP): "Derived types MUST be substitutable for their base types. Interfaces and base classes must preserve contracts and not strengthen preconditions nor weaken postconditions. Unit tests SHOULD include substitution checks where polymorphism is used."
      - Interface Segregation (ISP): "Prefer multiple focused interfaces over large, multipurpose ones. Interfaces in Application and Domain layers MUST be small and intent-revealing."
      - Dependency Inversion (DIP): "High-level modules (Domain/Application) MUST NOT depend on low-level modules (Infrastructure). Depend on abstractions and inject concrete implementations in Infrastructure."

  - name: CleanArchitecture+DDD
    details:
      - "The repository MUST reflect clear logical layering: Domain, Application, Infrastructure, API (presentation). Physical layout is required and MUST be followed by all features and tests."
      - "Domain folder contains: Aggregates, Entities, ValueObjects, DomainEvents, DomainServices, Exceptions, and repository interfaces. Domain code MUST be framework-agnostic and free of EF Core / ASP.NET Core references."
      - "Application folder contains: Use-cases (Commands/Queries), DTOs, Handlers, ApplicationServices, and mapping logic. Application layer orchestrates domain operations and depends only on Domain abstractions."
      - "Infrastructure contains concrete implementations: persistence (EF Core repositories), external API clients (TMDb client adapter), caches, files, and cross-cutting platform glue. Infrastructure may depend on Application and Domain."
      - "API contains: Controllers, minimal Presentation adapters, Middleware, Views (optional demo .cshtml), and wwwroot (static assets). UI is secondary: proving DDD layering and testability is priority."
      - "Tests MUST map to layers: Domain tests exercise domain rules, Application tests exercise use-cases and orchestration. Coverage gating applies to Domain and Application layers (see Quality)."

  - name: Patterns
    details:
      - Structural: Repository (interface in Domain, implementation in Infrastructure), Adapter (external API adapters for TMDb), Facade (aggregate operations for complex subsystems)
      - Creational: Factory Method for complex aggregate creation; Singleton only where process-wide singletons are necessary (e.g., telemetry singleton), otherwise avoid global state
      - Behavioral: Observer (Domain events / event handlers), Strategy (pluggable algorithms like recommendation strategies)
      - "Pattern usage MUST be justified in PR descriptions; prefer small, well-tested implementations over premature abstraction."

  - name: Quality
    details:
      - "Code coverage: Domain + Application layers MUST achieve >= 80% line coverage. Coverage tools (coverlet, etc.) MUST be run in CI and reported."
      - "Unit tests: fast, deterministic unit tests for Domain and Application. Use in-memory or mocked dependencies; avoid DB in unit tests."
      - "Integration tests: TMDb ingestion, EF Core persistence, and API endpoints MUST have integration tests. These may run in CI on an integration job (using Docker Compose)."
      - "PRs: All PRs MUST pass lint, build, tests, and coverage gates before merging. A single approving reviewer is required."

  - name: Security & Privacy
    details:
      - "Secrets and credentials MUST come from environment variables or a secrets manager; do NOT commit secrets. Examples: TMDB_API_KEY, DATABASE_URL."
      - "Use secure credential storage on CI (GitHub Secrets) and in deployment. Local developers may use a .env file for convenience but MUST NOT commit it."
      - "Passwords and secrets in persistent storage MUST be encrypted or hashed using industry-standard algorithms (PBKDF2/Argon2)."
      - "Follow minimal data collection principles; do not store PII unless required and explicitly documented."

  - name: Data & Sync
    details:
      - "Canonical store: a single PostgreSQL database MUST be the source of truth for catalog and user data. Use one DATABASE_URL configuration for connection strings."
      - "TMDb ingestion: an automated background worker (IHostedService / Worker) MUST poll TMDb and upsert titles into the database. There MUST NOT be an admin UI to add titles; the worker is the authoritative ingestion path."
      - "Schema changes: use EF Core Migrations for all schema changes. Developers MUST create migrations and include migration verification in CI (e.g., dotnet ef migrations script / migrations check)."

  - name: Observability
    details:
      - "Application MUST expose structured logs, basic metrics (requests, errors, worker sync stats), and trace IDs for error correlation. Prefer OpenTelemetry-compatible libraries."

  - name: CI/CD & Deployment
    details:
      - "CI: GitHub Actions MUST run on PRs and merges to main: steps include restore/build, lint, unit tests, coverage report, integration tests (on a separate matrix job), and migrations verification."
      - "Coverage gating: a dedicated CI job MUST fail the PR if Domain+Application coverage < 80%."
      - "Provide Docker Compose for local development (app + postgres + optional test services). Provide a one-command quickstart to run local environment, apply migrations, and run the TMDb sync worker."

development-guidelines:
  folder-structure:
    - Domain/
      - Aggregates/
      - Entities/
      - Events/
      - Exceptions/
      - Interfaces/
      - Services/
      - ValueObjects/
    - Application/
      - Commands/
      - DTOs/
      - Handlers/
      - Queries/
      - Services/
    - Infrastructure/
      - Cache/
      - Configuration/
      - Persistence/
      - Repositories/
      - TMDbClient/        # adapter implementation
    - API/
      - Controllers/
      - Middleware/
      - Views/              # optional demo Razor pages
      - wwwroot/
        - css/
        - js/
        - images/

  testing:
    - "Unit tests: Domain + Application layers (fast, mocked dependencies). Coverage threshold applies (>=80%)."
    - "Integration tests: TMDb sync, DB upserts, and API endpoints. Use Docker Compose to spin up Postgres in CI/integration runs."

  code-review:
    - "All changes must have a PR with at least one approving reviewer and passing CI that enforces the constitution gates (lint/build/tests/migrations/coverage)."

  documentation:
    - "Keep architecture and DDD maps in docs/ARCHITECTURE.md and include a simple diagram showing the four-layer separation. Ensure README highlights that UI is secondary and the repo demonstrates DDD layering and tests."

ops:
  database: PostgreSQL (canonical store)
  external-apis:
    - TMDb: "Catalog & metadata for movies and TV shows; ingested via background worker"
  tech-stack:
    - backend: ".NET 8 (or latest stable) with ASP.NET Core"
    - orm: "Entity Framework Core (use migrations for schema changes)"
    - db: "PostgreSQL (Docker Compose for local)"
    - auth: "ASP.NET Identity or minimal identity solution for demo"
    - background-worker: "IHostedService / Worker service for TMDb sync"
    - tests: "xUnit / FluentAssertions / Moq"
    - ci: "GitHub Actions"
  credentials:
    - "TMDB_API_KEY in env var TMDB_API_KEY"
    - "DATABASE_URL in env var DATABASE_URL"

```

## Required repository rules (enforced by PRs / CI)

- The repository structure MUST reflect the DDD layers above; feature code and tests MUST be placed inside the matching folders.
- PRs MUST include the reason for chosen patterns and references to which principle they satisfy (SRP, DIP, etc.).
- All new DB schema changes MUST include EF Core Migrations files and a migration verification step in CI.
- Secrets MUST come from env vars or a cloud secret manager (GitHub secrets for CI). No secrets in code or committed .env files.
- The TMDb ingestion worker MUST run in CI smoke tests (integration job) and local Quickstart.

## First-day developer checklist (quickstart)

1. Install Docker Desktop and enable WSL2 (if on Windows).
2. Copy `.env.example` to `.env` and set values for:
   - TMDB_API_KEY (obtain from TMDb)
   - DATABASE_URL (e.g., postgres://user:pass@localhost:5432/reco)
3. Start services locally:
   - Use provided Docker Compose: `docker compose up --build -d`
4. Apply migrations:
   - `dotnet ef database update --project src/Infrastructure --startup-project src/API`
5. Seed sample catalog data (local script):
   - Run `dotnet run --project tools/Seed/SeedRunner --sync-tmdb` (or the provided script) to trigger TMDb seed once
6. Start the web app and worker (if not running via Docker Compose):
   - `dotnet run --project src/API` and `dotnet run --project src/Worker`
7. Run unit tests and coverage locally:
   - `dotnet test /p:CollectCoverage=true` (CI has exact command)

Notes: the Quickstart must demonstrate DDD layering and tests; UI is optional and secondary to proving the architecture.

## CI expectations (GitHub Actions)

- Pull Request workflow MUST include jobs:
  - restore & build
  - lint/static analysis
  - unit tests (Domain + Application) with coverage report
  - integration tests (Docker Compose with Postgres and worker) on a separate job
  - migrations verification (generate SQL and ensure migrations applied)
  - coverage gate: fail PR if Domain+Application coverage < 80%

## Governance and versioning

- Versioning for the constitution follows semantic rules:
  - MAJOR: incompatible governance or principle removals/redefinitions
  - MINOR: new principle/section added or materially expanded guidance
  - PATCH: clarifications, wording, typos
- RATIFICATION_DATE is the original adoption date. LAST_AMENDED_DATE is updated to the date of any change. Dates are ISO YYYY-MM-DD.

## Enforcement and compliance

- The repository's plan/spec/tasks templates MUST include a "Constitution Check" gate; CI or reviewers MUST ensure plans comply before Phase 0 research proceeds.
- Non-compliant PRs must include an explicit justification and a migration plan to align with the constitution.

## Appendix: sample env var names

- TMDB_API_KEY — TMDb API key used by the worker and any TMDb client
- DATABASE_URL — PostgreSQL connection string (canonical DB)
- DOTNET_ENVIRONMENT — Development/Production
- ASPNETCORE_ENVIRONMENT — ASP.NET Core environment

## Appendix: required design patterns (summary)

- Repository: domain interface, EF Core implementation in Infrastructure
- Adapter: TMDb API adapter living in Infrastructure/TMDbClient
- Facade: aggregate facade for complex composition operations
- Factory Method: for controlled aggregate creation
- Singleton: use sparingly and only for app-scoped singletons like telemetry
- Observer: domain events and event handlers
- Strategy: interchangeable recommendation algorithms

## Closing notes

This constitution is the source of truth for architectural governance for RECO. The repository MUST demonstrate the DDD folder layout and the Layered architecture in code and tests; visual UI is secondary.
# RECO Project Constitution

*(Used by Spec-Kit — format: human-readable Markdown with YAML-like sections for clarity)*

```yaml

# RECO - Spec Kit Constitution
# Purpose: project governing principles, code & architecture standards for RECO (movie/series recommender)

project:
  name: RECO
  description: >
    Academic web application that recommends movies & series personalized to users.
    Automatically syncs catalog from TMDb into a PostgreSQL database and serves content via ASP.NET Core (C#).
  license: MIT

principles:
  - name: SOLID
    details:
      - Single Responsibility: "Each class/module has one clear responsibility."
      - Open/Closed: "Components are open for extension, closed for modification."
      - Liskov Substitution: "Derived types must be replaceable for their base types."
      - Interface Segregation: "Prefer many specific interfaces over large general ones."
      - Dependency Inversion: "Depend on abstractions; keep concrete implementations in infrastructure."
  - name: CleanArchitecture+DDD
    details:
      - "Logical separation into Domain, Application, Infrastructure, API layers."
      - "Domain folder contains: Aggregates, Entities, Events, Exceptions, Interfaces, Services, ValueObjects."
      - "Infrastructure contains: Cache, Configuration, Persistence, Repositories."
      - "Application contains: Commands, DTOs, Handlers, Queries."
      - "API contains: Controllers, Middleware, Views (.cshtml optional), wwwroot (images/css/js)."
      - "Emphasize DDD folder structure and layering over UI implementation."
  - name: Patterns
    details:
      - Structural: Repository, Adapter, Facade
      - Creational: Factory Method, Singleton (use carefully)
      - Behavioral: Observer, Strategy
  - name: Quality
    details:
      - "Unit tests: target >= 80% coverage for Domain & Application layers."
      - "Integration tests for TMDb ingestion and DB persistence."
      - "Automated CI: linting, build, tests, migration checks."
  - name: Security & Privacy
    details:
      - "Store secrets (TMDb key, DB creds) in environment variables / secret manager."
      - "Use secure password hashing (ASP.NET Identity / PBKDF2 or Argon2)."
      - "Follow GDPR-like minimal data collection (only needed profile fields)."
  - name: Data & Sync
    details:
      - "Single PostgreSQL database as canonical store."
      - "Background service (hosted service / worker) that pulls TMDb catalog (movies/series) and upserts to DB; no manual admin UI to add content."
      - "Use migrations (EF Core Migrations) for schema changes."
  - name: Observability
    details:
      - "Request logging, metrics (Prometheus-compatible), and error tracing."
  - name: UX / Accessibility
    details:
      - "Ensure basic responsive design and keyboard navigability for academic demo."
  - name: CI/CD & Deployment
    details:
      - "Build and test on PRs. Provide a simple Docker Compose for local dev (app + postgres)."
      - "Provide a script to seed DB from TMDb for first run."

development-guidelines:
  folder-structure:
    - Domain/
      - Aggregates/
      - Entities/
      - Events/
      - Exceptions/
      - Interfaces/
      - Services/
      - ValueObjects/
    - Application/
      - Commands/
      - DTOs/
      - Handlers/
      - Queries/
      - Services/
    - Infrastructure/
      - Cache/
      - Configuration/
      - Persistence/
      - Repositories/
    - API/
      - Controllers/
      - Middleware/
      - Views/      # optional .cshtml Razor pages for demo UI
      - wwwroot/
        - css/
        - js/
        - images/
  testing:
    - "Unit tests: Domain + Application layers (fast, mocked dependencies)."
    - "Integration tests: TMDb sync, DB upserts, and API endpoints."
  code-review:
    - "All changes must have PR with at least one approving reviewer and passing CI."
  documentation:
    - "Keep architecture and DDD maps in docs/ARCHITECTURE.md"

ops:
  database: PostgreSQL
  external-apis:
    - TMDb: "Catalog & metadata for movies and TV shows"
  tech-stack:
    - backend: ".NET 8 (or latest stable) with ASP.NET Core"
    - orm: "Entity Framework Core"
    - db: "PostgreSQL (Docker Compose for local)"
    - auth: "ASP.NET Identity or IdentityServer minimal for demo"
    - background-worker: "IHostedService / Worker service for TMDb sync"
    - tests: "xUnit / FluentAssertions / Moq"
    - ci: "GitHub Actions"
  credentials:
    - "TMDB_API_KEY in env var TMDB_API_KEY"
    - "POSTGRES_URL in env var DATABASE_URL"
