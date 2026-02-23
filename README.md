# Second API

This repository is fully containerized so it can run on **Windows, macOS, and Linux** with only Docker installed.

## Prerequisites

- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (Windows/macOS)
- Docker Engine + Compose plugin (Linux)

## Quick start

From the repository root:

```bash
docker compose up --build
```

This starts:

- `api` (`Second.API`) on `http://localhost:8080`
- `db` (SQL Server 2022) on `localhost:1433`
- `redis` on `localhost:6379`

The API runs EF Core migrations on startup, so the database schema is created automatically.

## Email in Docker: why disabled by default?

Email is **disabled by default** in Docker (`EMAIL_ENABLED=false`) because SMTP credentials are environment-specific secrets and typically not available in a freshly cloned repo.

If email were enabled by default without valid credentials, the API would fail startup due to email configuration validation.

## Enable email sending

1. Copy the template and fill your SMTP values:

```bash
cp .env.example .env
```

2. Edit `.env` and set:

```dotenv
EMAIL_ENABLED=true
EMAIL_FROM_ADDRESS=your-email@example.com
EMAIL_SMTP_HOST=smtp.gmail.com
EMAIL_SMTP_PORT=587
EMAIL_USE_SSL=true
EMAIL_USE_DEFAULT_CREDENTIALS=false
EMAIL_USERNAME=your-email@example.com
EMAIL_PASSWORD=your-app-password
```

3. Start (or restart) the stack:

```bash
docker compose up --build
```

## Stop everything

```bash
docker compose down
```

To also remove persisted database/redis data:

```bash
docker compose down -v
```

## Notes

- Configuration is injected through `docker-compose.yml` environment variables.
- `docker-compose.yml` reads email values from `.env` (or falls back to safe defaults).
## API documentation

- Complete frontend-focused API reference: `docs/api-reference.md`

