# Feature Specification: Personalized Recommendations

**Feature Branch**: `1-personalized-recommendations`
**Created**: 2025-11-02
**Status**: Draft
**Input**: User description: "An ASP.NET Core (C#) web app that recommends movies and TV shows personalized per user. Content must be ingested automatically from TMDb into PostgreSQL via a background worker. UI is demo Razor pages; architecture (Domain, Application, Infrastructure, API) and tests are primary."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Register & Preferences (Priority: P1)
As a new user, I can register and after registration fill a short preferences form (favorite genres, favorite actors, favorite directors) so the system can personalize recommendations.

Why this priority: Personalization is core to the product; onboarding with preferences is the primary driver for relevant recommendations.

Independent Test:
- Create a new user, submit the preferences form, and assert that a Recommendation profile is created for that user containing the selected preference attributes.

Acceptance Scenarios:
1. Given a visitor on the registration page, when they complete registration with valid data, then they are prompted to fill a short preferences form.
2. Given a completed preferences form, when the user submits it, then the system stores preferences tied to the user and schedules personalization to be applied.

---

### User Story 2 - Home & Navigation (Priority: P1)
As a user, I can log in and see a home page with a hero "RECO - Discover movies and series made for you..." and a main navigation with: Movies, Series, Recommendations, My Lists. Header also contains: Movies, Series, Recommendations, My Lists, My Profile, Sign Out.

Independent Test:
- Login as a seeded test user, navigate to the home page, and assert the presence of the hero heading and the listed navigation items.

Acceptance Scenarios:
1. Given an authenticated user, when they visit the site root, then they see the hero text and navigation options described above.

---

### User Story 3 - Browse, Search, Details (Priority: P2)
As a user, I can view Movies or Series pages showing an initial set of recommended items (poster + title), search and filter by genre, and click a title to view a details page with synopsis, trailer (YouTube embed if available), ratings (IMDb, Rotten Tomatoes if available, plus app average), and user reviews.

Independent Test:
- From Movies page, search for a genre filter and assert that results only contain that genre. Open a details page and assert presence of synopsis, trailer embed (if available), rating fields, and reviews section.

Acceptance Scenarios:
1. Given the Movies page, when the page loads, then an initial set of recommended items appear (poster + title).
2. Given a genre filter applied, when results load, then each shown item includes the selected genre.
3. Given a title selected, when viewing the details page, then synopsis, rating information, trailer embed (if any), and reviews are displayed.

---

### User Story 4 - Personal Lists (Priority: P2)
As a user, I can create personal lists (e.g., "Watch this weekend", "Favorites") and add/remove items.

Independent Test:
- Create a list, add an item, verify the item appears in the list, remove it, verify it no longer appears.

Acceptance Scenarios:
1. Given an authenticated user, when they create a new list (title provided), then the list is persisted and visible in "My Lists".
2. Given an item on a list, when the user removes it, then the item is removed from the list.

---

### User Story 5 - Automated Content Ingestion (Priority: P0 - systemic)
As a system, content must be loaded automatically from TMDb API into PostgreSQL via a scheduled/background worker; no manual admin interface to add titles.

Independent Test:
- Run the TMDb ingestion worker in an integration test (with test/stub TMDb responses) and assert titles are upserted into the Postgres test DB.

Acceptance Scenarios:
1. Given the worker runs on schedule or via a manual trigger, when it queries TMDb for catalog changes, then new/updated titles are upserted into the DB.
2. Given ingestion encounters duplicate titles, when upserting, then existing records are updated rather than duplicated.

---

### Edge Cases
- TMDb API rate limiting or transient failures (retry/backoff and circuit-breaker behavior required at integration level).
- Partial metadata (no trailer, no external ratings) — details page should gracefully fall back to available data only.
- User preferences empty — system falls back to general popular recommendations.

## Requirements *(mandatory)*

### Functional Requirements
- FR-001: The system MUST allow user registration and authentication.
- FR-002: After registration the system MUST present a short preferences form (genres, actors, directors) and persist the results.
- FR-003: The system MUST provide a home page with the hero text and the navigation items: Movies, Series, Recommendations, My Lists, My Profile, Sign Out.
- FR-004: The system MUST present Movies and Series lists with initial recommended items (poster + title), and support search and genre-based filtering.
- FR-005: The system MUST provide a title details page including synopsis, trailer embed (if available), ratings (external when available plus app average), and user reviews.
- FR-006: The system MUST allow users to create named lists and add/remove items from those lists.
- FR-007: The system MUST automatically ingest content from TMDb into a single PostgreSQL database via a background worker; no manual UI for adding titles.
- FR-008: All schema changes MUST be managed using EF Core Migrations.
- FR-009: Secrets (TMDb API key, DB credentials) MUST be provided via environment variables or a secret manager.
- FR-010: The repository MUST show explicit layering (Domain, Application, Infrastructure, API) and tests that demonstrate layering.

### Non-Functional Requirements
- NFR-001: The repository MUST include unit tests and integration tests; Domain + Application layers MUST have >= 80% coverage.
- NFR-002: The canonical store MUST be a single PostgreSQL database (DATABASE_URL env var).
- NFR-003: The TMDb ingestion worker MUST be testable in CI and locally via Docker Compose.
- NFR-004: Observability: application MUST expose structured logs and basic metrics for requests and worker syncs.
- NFR-005: Security: secrets MUST NOT be committed to source control and MUST be supplied via env vars / secret manager (examples: TMDB_API_KEY, DATABASE_URL).
- NFR-006: The UI MAY be implemented with Razor (.cshtml) for demo purposes but UI is secondary to code architecture and tests.

## Key Entities *(include if feature involves data)*
- User: id, email, password_hash, profile (display name), preferences (genres, actors, directors)
- Title: id (TMDb id), type (movie/series), title, synopsis, poster_url, release_date, external_ratings, app_average_rating
- RecommendationProfile: user_id, computed_recommendation_seed (derived from preferences)
- List: id, user_id, name, created_at
- ListItem: list_id, title_id, added_at
- TMDbSyncJob / TMDbRecord: records used during ingestion to track processed items

## Success Criteria *(mandatory)*
- SC-001: DDD folder structure (Domain, Application, Infrastructure, API) exists in the repository and contains code and tests that map to each layer.
- SC-002: Automated TMDb ingestion is implemented and covered by integration tests which pass in CI (stubbing or test TMDb responses); worker upserts new titles to PostgreSQL.
- SC-003: The UI contains the described pages (home hero + navigation, Movies, Series, Recommendations, My Lists, title details) implemented as Razor demo pages; pages render and link as expected in an end-to-end smoke test.
- SC-004: Domain + Application test coverage is >= 80% as reported by CI coverage tooling.
- SC-005: EF Core migrations are included for any schema changes and migration verification passes in CI.
- SC-006: Secrets are not committed and required env var names are documented (`TMDB_API_KEY`, `DATABASE_URL`).

## Assumptions
- The project will use ASP.NET Core and EF Core per request (this is a stated constraint, but functional requirements and success criteria remain technology-agnostic where practical).
- TMDb provides required metadata via its public API (title data, posters, trailers, external ids for ratings).
- For external ratings (IMDb, Rotten Tomatoes) the ingestion pipeline will attempt to fetch them when TMDb exposes external IDs; where not available the app average rating will be used.
- Local developer will use Docker Compose to run Postgres during integration tests.

## Dependencies
- TMDb API availability & API key
- PostgreSQL for canonical storage
- EF Core for migrations and ORM
- Background worker hosting (IHostedService / Worker or equivalent)

## Open questions / clarifications
- None required; user supplied the stack (ASP.NET Core) and the major architectural constraints. No [NEEDS CLARIFICATION] markers remain.

## Implementation notes (for planning only)
- The TMDb ingestion worker SHOULD be implemented as an infrastructure service that depends on a TMDb adapter and repository interface; tests should mock the TMDb adapter for unit tests and use a test Postgres instance for integration.
- Recommendation algorithms (Strategy pattern) will be pluggable; a simple baseline (preference matching + popularity boost) is acceptable for the first iteration.


---

**Spec ready for planning**
