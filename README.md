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
cp .env.dev.example .env.dev
./env.sh dev up        # equivale al antiguo `docker compose up -d`
```

Esto levanta:
- **Postgres** en `localhost:5432` (esquemas `catalog`, `booking`, `payments`, `notifications` ya creados)
- **Redis** en `localhost:6379`
- **Keycloak** en `http://localhost:8080` (realm `aeroapp` importado, admin console con las credenciales de `.env.dev`)

Verificar que los tres contenedores están sanos:

```bash
./env.sh dev ps
```

## Ambientes: dev, qa, preprod, prod

Cada ambiente tiene su configuración de API (`appsettings.<Ambiente>.json`), de web
(`config.json` leído al arrancar) e infraestructura local (`infra/.env.<amb>`), y un
GitHub Environment para el pipeline de despliegue. Detalle en
[`docs/AMBIENTES.md`](docs/AMBIENTES.md).

| Ambiente | API (`ASPNETCORE_ENVIRONMENT`) | Infra local | Despliegue |
|---|---|---|---|
| dev | `Development` | `./env.sh dev up` (puertos 5432/6379/8080) | automático |
| qa | `QA` | `./env.sh qa up` (15432/16379/18080) | automático |
| preprod | `Preprod` | `./env.sh preprod up` (25432/26379/28080) | aprobación manual, solo `main` |
| prod | `Production` | `./env.sh prod up` (35432/36379/38080) | aprobación manual, solo `main` |

## Fases del proyecto

Ver el detalle completo en [`docs/PRD.md`](docs/PRD.md#2-alcance-por-fases-mvp-incremental). Estado actual: **Fase 0 en curso**.
