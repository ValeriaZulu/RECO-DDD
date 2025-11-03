# RECO Architecture

Layers (Clean Architecture + DDD):

- Domain: `src/RECO.Domain` — Aggregates, Entities, ValueObjects, Domain interfaces
- Application: `src/RECO.Application` — Use-cases, handlers, DTOs
- Infrastructure: `src/RECO.Infrastructure` — EF Core, TMDb adapter, Worker
- API: `src/RECO.API` — Controllers, Razor views, static assets

Data flow: Browser -> API -> Application -> Domain -> Infrastructure -> PostgreSQL

This document exists to make the separation explicit and to match the constitution requirements.
