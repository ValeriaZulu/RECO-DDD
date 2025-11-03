# RECO — academic movie/series recommender

Quickstart:

1. Copy `.env.example` to `.env` and set `TMDB_API_KEY` and `DATABASE_URL`.
2. Start Postgres for local development:

```powershell
docker compose up -d postgres
```

3. Build and run projects via `dotnet run` or open in VS/VSCode.

Notes: This repository demonstrates DDD layering per the project constitution. UI is a demo Razor pages app; the TMDb ingestion worker is authoritative for titles.
