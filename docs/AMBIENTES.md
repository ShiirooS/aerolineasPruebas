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
- Solo dev trae su cadena de conexión (al Postgres local, `aeroapp/aeroapp`).
- **qa, preprod y prod no traen credenciales en el repo** (es público). La cadena de
  conexión y los orígenes CORS llegan por variables de entorno:
  ```bash
  ConnectionStrings__CatalogDb="Host=...;Password=..."
  Cors__AllowedOrigins__0="https://app.ejemplo.com"
  ```
  Si falta la cadena, la API no arranca ("Falta la cadena de conexión 'CatalogDb'").
- Correr la API local en un ambiente: `dotnet run --launch-profile qa` (perfiles `http`
  = dev, `qa`, `preprod`, `production` en `Properties/launchSettings.json`). Para qa
  contra su stack local (la contraseña sale de `infra/.env.qa`, que no se versiona):
  ```bash
  (source infra/.env.qa && cd services/catalog/src/Catalog.Api && \
    ConnectionStrings__CatalogDb="Host=localhost;Port=$POSTGRES_PORT;Database=$POSTGRES_DB;Username=$POSTGRES_USER;Password=$POSTGRES_PASSWORD;Search Path=catalog" \
    dotnet run --launch-profile qa)
  ```

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

## AWS emulado (`infra/aws/`)

Cada ambiente se despliega en su propia **"cuenta AWS"**, que localmente es un
[AWS Emulator](https://github.com/aws/aws-emulator) en su propio puerto. Es lo mismo que
se hace en AWS real: una cuenta por ambiente, aisladas entre sí.

| Ambiente | Puerto del emulador | Buckets |
|---|---|---|
| dev | 9297 | `aeroapp-web-dev`, `aeroapp-releases-dev` |
| qa | 9298 | `aeroapp-web-qa`, `aeroapp-releases-qa` |
| preprod | 9299 | `aeroapp-web-preprod`, `aeroapp-releases-preprod` |
| prod | 9300 | `aeroapp-web-prod`, `aeroapp-releases-prod` |

Qué crea `cloudformation/aeroapp.yaml` en cada cuenta (stack `aeroapp`):
- **S3** `aeroapp-web-<amb>`: el build de Angular con el `config.json` del ambiente.
- **S3** `aeroapp-releases-<amb>` (versionado, se conserva): `catalog-api/<versión>.zip`.
- **ECR** `aeroapp/catalog-api`: para cuando la API corra en contenedores.
- **SSM Parameter Store**: `/aeroapp/catalog-api/aspnetcore-environment`,
  `/aeroapp/web/api-base-url` y `/aeroapp/catalog-api/release` (versión desplegada).
- **Secrets Manager** `aeroapp/catalog-api/db`: usuario y contraseña generados por AWS.

### Uso local

Requisitos: Docker (en Arch no hay paquete del emulador; `emulator.sh` construye la
imagen con el Dockerfile oficial la primera vez) y la AWS CLI v2.

```bash
cd web && npm ci && cd ..                 # una vez (node_modules de ESTA máquina)
infra/aws/build-artifact.sh               # artefacto en out/ (igual que el pipeline)

infra/aws/emulator.sh qa start            # arranca la "cuenta" de qa
infra/aws/deploy.sh qa out                # despliega

eval "$(infra/aws/emulator.sh qa env)"    # apunta la AWS CLI a la cuenta de qa
aws s3 ls
aws ssm get-parameter --name /aeroapp/catalog-api/release

infra/aws/emulator.sh qa stop             # al parar se borra todo (vive en memoria)
```

### Seguridad

- `deploy.sh` **solo despliega contra el emulador**: si la AWS CLI no apunta a
  `127.0.0.1` con la cuenta `000000000000`, se detiene.
- El emulador solo escucha en `127.0.0.1` y su telemetría va apagada.

### Limitaciones (honestas)

- El emulador guarda todo **en memoria**: al pararlo se pierde lo desplegado. En CI
  cada job arranca uno nuevo, despliega, verifica y lo descarta.
- No ejecuta la API: la API queda como release (zip en S3 + versión en SSM), no como
  servicio corriendo. Tampoco sirve la web como sitio público (no hay endpoint de
  website de S3); se verifica leyendo los objetos.
- No evalúa políticas IAM: no sirve para probar seguridad.
- Pasar a AWS real = credenciales reales (p. ej. OIDC de GitHub → rol por cuenta) y
  quitar el emulador de `_deploy-env.yml`. La plantilla y `deploy.sh` (sin el candado)
  sirven igual.

## Workflows

- `.github/workflows/ci.yml`: tests de backend (con Postgres) y frontend en cada push
  a ramas que no son `main` y en cada PR, más un despliegue de prueba en un AWS
  emulado (cuenta dev) para detectar plantillas o scripts rotos antes del merge.
- `.github/workflows/deploy.yml`: en cada push a `main` corre CI, construye el
  artefacto y lo promueve por los 4 ambientes.
- `.github/workflows/_deploy-env.yml`: despliegue de un ambiente en su cuenta AWS
  emulada con `infra/aws/deploy.sh`. Comprueba además que la variable
  `ASPNETCORE_ENVIRONMENT` del GitHub Environment coincide con `infra/aws/lib.sh`.
