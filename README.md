# AeroApp

Plataforma de reservas de vuelos (web + Android) construida como proyecto de aprendizaje de arquitectura de microservicios. PRD completo en [`docs/PRD.md`](docs/PRD.md).

## Estructura del monorepo

```
web/            Angular (Fase 1)
mobile/         Flutter para Android (Fase 6)
services/
  catalog/      Catalog.Service (Fase 1)
  booking/      Booking.Service (Fase 3)
  payments/     Payments.Service (Fase 4)
  notifications/ Notifications.Service (Fase 4)
  gateway/      Gateway/BFF (Fase 1+)
infra/          docker-compose, Kubernetes (Fase 7), config de Postgres/Keycloak
docs/           PRD y documentación
```

## Entorno local (Fase 0)

```bash
cd infra
cp .env.example .env
docker compose up -d
```

Esto levanta:
- **Postgres** en `localhost:5432` (esquemas `catalog`, `booking`, `payments`, `notifications` ya creados)
- **Redis** en `localhost:6379`
- **Keycloak** en `http://localhost:8080` (realm `aeroapp` importado, admin console con las credenciales de `.env`)

Verificar que los tres contenedores están sanos:

```bash
docker compose ps
```

## Fases del proyecto

Ver el detalle completo en [`docs/PRD.md`](docs/PRD.md#2-alcance-por-fases-mvp-incremental). Estado actual: **Fase 0 en curso**.
