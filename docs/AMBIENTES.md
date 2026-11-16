# Ambientes: dev, qa, preprod, prod

Se construye **una vez** y el mismo artefacto se promueve de un ambiente al siguiente.
Lo único que cambia entre ambientes es la configuración, nunca el código.

```
push a main ──► CI ──► build (artefacto) ──► dev ──► qa ──► preprod ──► prod
                                            auto    auto   aprobación  aprobación
```

## Nombres (respetar mayúsculas: en Linux importan)

| GitHub Environment | `ASPNETCORE_ENVIRONMENT` | `appsettings` | Web `config.json` | Infra local |
|---|---|---|---|---|
| `dev` | `Development` | `appsettings.Development.json` | `web/public/config.json` | `infra/.env.dev` |
| `qa` | `QA` | `appsettings.QA.json` | `web/config/qa/config.json` | `infra/.env.qa` |
| `preprod` | `Preprod` | `appsettings.Preprod.json` | `web/config/preprod/config.json` | `infra/.env.preprod` |
| `prod` | `Production` | `appsettings.Production.json` | `web/config/prod/config.json` | `infra/.env.prod` |

`Development` y `Production` son los nombres estándar de ASP.NET Core: así
`IsDevelopment()` / `IsProduction()` siguen funcionando (OpenAPI solo se publica en
`Development`).

## Backend (Catalog.Api)

- `appsettings.json` es la base; `appsettings.<Ambiente>.json` la sobrescribe.
- dev y qa traen su cadena de conexión al Postgres local de su stack.
- **preprod y prod no traen secretos en el repo** (es público). La cadena de conexión y
  los orígenes CORS llegan por variables de entorno:
  ```bash
  ConnectionStrings__CatalogDb="Host=...;Password=..."
  Cors__AllowedOrigins__0="https://app.ejemplo.com"
  ```
  Si falta la cadena, la API no arranca ("Falta la cadena de conexión 'CatalogDb'").
- Correr la API local en un ambiente: `dotnet run --launch-profile qa` (perfiles `http`
  = dev, `qa`, `preprod`, `production` en `Properties/launchSettings.json`).

## Frontend (Angular)

- La app lee `config.json` **al arrancar** (`src/app/core/config/app-config.ts`, cargado
  en `main.ts`) y lo expone con el token `APP_CONFIG`.
- Por eso el bundle JS es idéntico en todos los ambientes: el pipeline solo copia
  `web/config/<amb>/config.json` encima del `config.json` del build.
- Local contra QA: `npm run start:qa` (sirve en `http://localhost:4201`, que es el origen
  CORS permitido por `appsettings.QA.json`).
- Los dominios de preprod/prod (`*.aeroapp.example`) son marcadores: cámbialos cuando
  existan dominios reales.

## Infraestructura local (`infra/`)

```bash
cd infra
cp .env.qa.example .env.qa     # una vez por ambiente
./env.sh qa up | down | ps | logs | config
```

- Cada ambiente es un stack de compose independiente (`aeroapp`, `aeroapp-qa`, ...)
  con contenedores, volúmenes y puertos propios: pueden correr a la vez.
- preprod y prod añaden `docker-compose.prod.yml`: secretos obligatorios (sin valores
  por defecto), Postgres/Redis solo en `127.0.0.1`, Redis con contraseña y Keycloak en
  modo producción. `env.sh` se niega a arrancarlos si quedan `CHANGE_ME`.
- `env.sh ... down` no borra volúmenes.

## GitHub (Settings → Environments)

- `dev`, `qa`: sin restricciones.
- `preprod`, `prod`: requieren aprobación del dueño del repo y solo aceptan
  despliegues desde `main`.
- Cada uno tiene la variable `ASPNETCORE_ENVIRONMENT`. Los secretos (cadenas de
  conexión, etc.) se agregan como *Environment secrets* cuando haya servidores.

## Workflows

- `.github/workflows/ci.yml`: tests de backend (con Postgres) y frontend en cada push
  a ramas que no son `main` y en cada PR.
- `.github/workflows/deploy.yml`: en cada push a `main` corre CI, construye el
  artefacto y lo promueve por los 4 ambientes.
- `.github/workflows/_deploy-env.yml`: despliegue de un ambiente. **Hoy es un
  placeholder** (no hay servidores todavía): prepara el artefacto y la config, pero no
  despliega en ningún lado. Cuando haya un destino real, solo cambia este archivo.
