# PRD — AeroApp: Plataforma Web y Móvil de Aerolínea
### Documento de Requerimientos de Producto (Product Requirements Document)
**Versión:** 1.0 · **Fecha:** Agosto 2026

---

## 0. Cómo vamos a trabajar juntos

Dos frentes en paralelo:

1. **Mentor del curso de Angular**: cuando avances en el curso, traer el concepto (signals, standalone components, RxJS, forms, etc.) para explicarlo con la lógica de "por qué existe" y "cuándo se usa mal", no solo la sintaxis.
2. **Arquitecto del proyecto personal (AeroApp)**: este PRD es el mapa, construido pieza por pieza, con Claude Code como "pair programmer" para scaffolding, revisiones y debugging.

**Regla de oro**: en una arquitectura real, el 80% del valor no está en "qué framework usé" sino en los **límites claros entre capas y servicios**. Se aplica desde el día 1.

---

## 1. Visión y objetivos

**AeroApp** es una plataforma de reservas de vuelos (tipo mini-aerolínea) con:
- Sitio web público y panel de reservas (Angular).
- App móvil nativa-performante para Android (framework a definir en §4).
- Backend de microservicios en C#/.NET.
- Una única fuente de verdad de datos (PostgreSQL), compartida por ambos clientes a través de la misma capa de API.

**Objetivos de aprendizaje** (proyecto didáctico, no solo un producto):
- Practicar Angular moderno en un caso de uso real y no trivial.
- Entender arquitectura de microservicios: comunicación síncrona (REST/gRPC), autenticación centralizada (Keycloak), caché (Redis).
- Aprender infraestructura real: Docker, Kubernetes, CI/CD con GitHub Actions.
- Entender cómo modelan sus datos los sistemas de reservas reales (PNR, GDS) y aplicar ese conocimiento a escala reducida.

**Objetivo de producto**: buscar vuelos, reservar, pagar (simulado), gestionar el "Passenger Name Record" (PNR), ver el estado de la reserva, todo desde web y Android con la misma cuenta.

---

## 2. Alcance por fases (MVP incremental)

| Fase | Contenido | Por qué en ese orden |
|---|---|---|
| **Fase 0** | Entorno local: Docker Compose con Postgres, Redis, Keycloak levantados y hablando entre sí | Sin esto no hay nada que construir encima |
| **Fase 1** | Servicio `Catalog` (vuelos, rutas, aeropuertos, tarifas) + Angular mostrando búsqueda de vuelos | Es el corazón del dominio, sin estado complejo de reservas todavía |
| **Fase 2** | Autenticación con Keycloak (login/registro) integrado en Angular y en el backend | La reserva necesita usuario identificado |
| **Fase 3** | Servicio `Booking` (PNR, pasajeros, asientos) + flujo de reserva end-to-end en Angular | Es la parte más compleja del dominio |
| **Fase 4** | Servicio `Payments` (simulado) + `Notifications` (email/eventos) | Ya con el dominio maduro, se agregan servicios de soporte |
| **Fase 5** | Redis como caché de búsquedas de vuelos + gRPC entre `Booking` y `Catalog` | Optimización, una vez que el flujo REST ya funciona |
| **Fase 6** | App Android (Flutter) reutilizando la misma API | Cliente adicional sobre una API ya estable |
| **Fase 7** | Kubernetes (local con kind/minikube) + CI/CD con GitHub Actions | Productizar el despliegue |
| **Fase 8** | Despliegue real en un proveedor gratuito | Opcional, cuando todo funcione localmente |

**Por qué este orden importa**: un error clásico es empezar por Kubernetes o por microservicios "bien separados" antes de tener un dominio claro. Primero el dominio (vuelos, reservas), después la infraestructura.

---

## 3. Arquitectura general

```
                         ┌─────────────────────┐
                         │   Angular Web App    │
                         │  (SPA, standalone,   │
                         │   signals, SSR opc.) │
                         └──────────┬───────────┘
                                    │ REST (HTTPS/JSON)
                         ┌──────────▼───────────┐        ┌──────────────────┐
                         │   API Gateway / BFF   │◄──────►│  Flutter Android  │
                         │  (ASP.NET Core /      │  REST  │      App          │
                         │   YARP reverse proxy) │        └──────────────────┘
                         └──────────┬────────────┘
                                    │ gRPC (interno)
        ┌───────────────┬──────────┼───────────────┬────────────────┐
        ▼               ▼          ▼               ▼                ▼
  ┌───────────┐   ┌───────────┐ ┌───────────┐ ┌───────────┐  ┌──────────────┐
  │  Catalog  │   │  Booking  │ │ Payments  │ │Notification│  │  Identity     │
  │  Service  │   │  Service  │ │ Service   │ │  Service   │  │ (Keycloak)    │
  └─────┬─────┘   └─────┬─────┘ └─────┬─────┘ └─────┬─────┘  └──────────────┘
        │               │             │             │
        └───────┬───────┴──────┬──────┴─────────────┘
                 ▼              ▼
           ┌───────────┐  ┌───────────┐
           │ PostgreSQL│  │   Redis   │
           │ (por      │  │  (caché)  │
           │ servicio) │  │           │
           └───────────┘  └───────────┘
```

**Decisiones de arquitectura clave**:

- **API Gateway/BFF en REST**: los clientes (Angular, Flutter) nunca hablan gRPC directo por simplicidad y compatibilidad con navegadores (gRPC-Web añade complejidad que no aporta en este contexto). El gateway expone REST hacia afuera y traduce a gRPC hacia adentro.
- **gRPC entre microservicios**: contratos fuertemente tipados (Protobuf), más rápido que REST/JSON para tráfico interno de alta frecuencia (ej. `Booking` pidiendo disponibilidad a `Catalog`).
- **Un Postgres lógico por servicio** (mismo clúster físico al inicio, esquemas separados: `catalog`, `booking`, `payments`) — respeta "database per service" sin pagar el costo operativo de 4 servidores de base de datos distintos mientras se aprende. Separar físicamente después es un cambio de configuración, no de diseño.
- **Keycloak como Identity Provider**: ni Angular ni los microservicios manejan contraseñas. Todo pasa por OAuth2/OIDC.
- **Redis**: caché de resultados de búsqueda de vuelos y almacén de sesiones/rate-limiting si hace falta.

---

## 4. Stack tecnológico (con versiones vigentes a agosto 2026)

### 4.1 Frontend Web
| Tecnología | Versión objetivo | Notas |
|---|---|---|
| Angular | **v22** (stable, jun 2026) | Signals estables, Signal Forms, zoneless por defecto, OnPush por defecto |
| TypeScript | 5.x | Acorde a lo que exija Angular 22 |
| Gestión de estado | Signals + servicios (evitar NgRx al inicio) | Suficiente y más fácil de entender antes de meter una librería de estado global |
| Estilos | Tailwind o Angular Material (a decidir cuando lleguemos ahí) | — |
| Testing | Vitest (integrado por Angular 21+) | Reemplaza Karma/Jasmine en proyectos nuevos |

> Nota de versiones: Angular 21 (nov 2025) LTS hasta mayo 2027, Angular 22 (jun 2026) es la última estable. Empezamos en 22 por ser proyecto nuevo.

### 4.2 Backend
| Tecnología | Versión objetivo | Notas |
|---|---|---|
| .NET | **.NET 10 (LTS)** | Soporte hasta noviembre 2028 |
| C# | 14 | Viene con .NET 10 |
| ASP.NET Core | 10 | Web API + gRPC host |
| Entity Framework Core | 10 | ORM hacia PostgreSQL |
| Arquitectura por servicio | Clean Architecture (Domain / Application / Infrastructure / API) | Ver §7 |

### 4.3 Base de datos
- **PostgreSQL** como base primaria transaccional (relacional).
- **Redis** como caché (capa de aceleración, no fuente de verdad).
- No se necesita NoSQL para el MVP: el dominio de reservas de vuelos es intrínsecamente relacional, con integridad referencial crítica (un asiento no puede estar doblemente ocupado). Un motor de búsqueda tipo Elasticsearch/OpenSearch solo se evaluaría si el catálogo requiere búsqueda facetada compleja — no antes de necesitarlo.

### 4.4 Comunicación entre servicios
- **gRPC** (Protobuf) para llamadas internas síncronas entre microservicios.
- **REST** para todo lo que toca el cliente (Angular/Flutter ↔ Gateway).
- (Fase futura, opcional) **eventos asíncronos** con un bus tipo RabbitMQ para desacoplar (ej. "reserva confirmada → enviar notificación").

### 4.5 Identidad y seguridad
- **Keycloak** como servidor de autenticación (OIDC/OAuth2), en Docker localmente.
- Angular usa Authorization Code + PKCE (`angular-oauth2-oidc` o `oidc-client-ts`).
- Los microservicios validan el JWT emitido por Keycloak.

### 4.6 Infraestructura
- **Docker** para contenerizar cada servicio.
- **Docker Compose** para el entorno local completo (fase 0-6).
- **Kubernetes** (local: `kind` o `minikube`) para fase 7 en adelante.
- **GitHub Actions** para CI/CD: build, test, imágenes Docker, push a GHCR, y deploy.

### 4.7 App móvil Android — Flutter

Recomendación: **Flutter**, por rendimiento en gama media-baja, un solo código Android/iOS, curva de aprendizaje suave viniendo de TypeScript/C#, y consumo de la misma API REST con el mismo modelo mental de DTOs que en Angular.

---

## 5. Microservicios propuestos

| Servicio | Responsabilidad | Base de datos |
|---|---|---|
| `Catalog.Service` | Aeropuertos, aerolíneas, rutas, vuelos, horarios, tarifas, disponibilidad de asientos por clase | `catalog` |
| `Booking.Service` | PNR, pasajeros, itinerarios, asignación de asientos, estado de la reserva | `booking` |
| `Payments.Service` | Procesamiento de pago simulado, estado de transacción | `payments` |
| `Notifications.Service` | Envío de confirmaciones (email/push), plantillas | `notifications` |
| `Identity` | Delegado 100% a Keycloak | — |
| `Gateway/BFF` | Punto único de entrada REST, agregación de llamadas, traducción REST↔gRPC | sin base de datos propia |

---

## 6. Modelo de datos

Patrón real de la industria (Amadeus/Sabre/Travelport): el PNR (Passenger Name Record) contiene el itinerario completo de un pasajero o grupo, separado en entidades lógicas: Reservation, Segment, Passenger, Ticket.

**Esquema `catalog`:**
```
airports (code PK, name, city, country, timezone)
airlines (code PK, name)
aircraft_types (id PK, model, seat_map_json)
flights (id PK, airline_code FK, flight_number, origin_code FK, destination_code FK,
         departure_at, arrival_at, aircraft_type_id FK)
fare_classes (id PK, flight_id FK, cabin, base_price, available_seats)
```

**Esquema `booking`:**
```
reservations (id PK, locator_code UNIQUE, user_id, status, created_at)
   -- locator_code = el "PNR" visible al usuario, ej. "AB7X2K"
segments (id PK, reservation_id FK, flight_id, fare_class_id, seat_number)
passengers (id PK, reservation_id FK, full_name, document_id, date_of_birth)
```

**Esquema `payments`:**
```
transactions (id PK, reservation_id, amount, currency, status, provider_ref, created_at)
```

**¿SQL o NoSQL?** SQL (PostgreSQL) para todo el dominio transaccional: la integridad referencial es exactamente la garantía que las bases relacionales dan gratis. Se deja la puerta abierta a Elasticsearch solo si el catálogo crece lo suficiente para necesitar búsqueda facetada compleja.

---

## 7. Buenas prácticas de arquitectura y código

### 7.1 Backend — Clean Architecture por servicio
```
Service.Domain          → Entidades, value objects, reglas de negocio puras (sin dependencias externas)
Service.Application     → Casos de uso (CQRS con MediatR), interfaces de repositorio
Service.Infrastructure  → Implementación de EF Core, acceso a Redis, clientes gRPC externos
Service.Api             → Controladores REST y/o servicios gRPC, DTOs, mapeo
```

### 7.2 Principios exigidos en cada PR
- Nombres explícitos (`GetAvailableFlightsQuery`, no `GetData`).
- Funciones pequeñas, una responsabilidad.
- Sin lógica de negocio en los controladores.
- Tests en Application layer (éxito + fallo por caso de uso importante).
- Conventional Commits (`feat:`, `fix:`, `refactor:`).

### 7.3 Frontend — buenas prácticas Angular
- Standalone components.
- Signals para estado local; servicios inyectables con signals para estado compartido simple.
- Smart/Dumb components.
- DTOs del backend como `interface` de TypeScript en `core/models`, nunca `any`.

---

## 8. Entorno local (Fase 0-6)

`docker-compose.yml` levanta: `postgres`, `redis`, `keycloak` (realm `aeroapp`), `catalog-api`, `booking-api`, `payments-api`, `notifications-api`, `gateway`, `web`.

### 8.1 Kubernetes (Fase 7)
- `kind` o `minikube`.
- `Deployment` + `Service` por microservicio, `ConfigMap`/`Secret` según corresponda.
- `Ingress` (NGINX Ingress Controller).
- Manifiestos con Kustomize (overlays `local` / `prod`).

### 8.2 CI/CD con GitHub Actions
1. `on: pull_request` → build + tests unitarios + linter.
2. `on: push a main` → build de imagen Docker → push a GHCR → (opcional) deploy automático.

### 8.3 Opciones gratuitas para despliegue real (Fase 8, opcional)
Oracle Cloud (VM Always Free), Fly.io, Railway, o free tier de Azure/GCP para estudiantes — a decidir en su momento.

---

## 9. Rol de Claude Code en este proyecto

- Genera el scaffolding inicial de cada microservicio (Clean Architecture) a partir de instrucciones del usuario.
- Ayuda a depurar errores de compilación o de configuración de Docker/Kubernetes.
- Revisa PRs contra las buenas prácticas del §7.

## 10. Estado actual

- **2026-08-29**: Fase 0 iniciada — estructura de monorepo y `docker-compose.yml` (Postgres + Redis + Keycloak) creados en `infra/`.
- **2026-08-30**: `Catalog.Service` completo en Clean Architecture (Domain/Application/Infrastructure/Api), todavía sin correr contra Postgres real (falta instalar Docker Desktop).
  - **Domain**: 5 entidades (`Airport`, `Airline`, `AircraftType`, `Flight`, `FareClass`) con value objects (`AirportCode`, `AirlineCode`, `FlightNumber`, `Money`, `AircraftTypeId`, `SeatMap`) y `DomainRuleViolationException` propia. 25 tests.
  - **Application**: `AddFareClassToFlightCommand` (cruza los agregados `Flight`/`AircraftType`) y `SearchFlightsQuery` (read model separado del repositorio, sin cargar agregados). 11 tests.
  - **Api**: `FlightsController` (MediatR + `ISender`), manejo centralizado de excepciones con `IExceptionHandler`, `JsonStringEnumConverter`. Probado en vivo con `curl` — encontró y corrigió 2 bugs reales (mapeo de `InvalidOperationException` genérica, serialización de enums).
  - **Infrastructure**: `CatalogDbContext` + configuraciones EF Core, repositorios, `FlightSearchReadModel`, migración inicial (`InitialCreate`) generada y validada.
  - Entorno: se instaló .NET 10 SDK y se actualizó Node a 24.19 LTS (ambos necesarios para que el tooling de Angular/EF funcionara).
- **2026-08-30 (verificación contra Postgres real)**: Docker Desktop **no se pudo instalar** — el instalador de winget necesita una confirmación de UAC interactiva que ninguna sesión automatizada puede completar (no es un tema de permisos otorgados, es una restricción de diseño de Windows). Para no bloquear la verificación, se levantó un Postgres 17 portátil (binarios sin instalador de EnterpriseDB, sin admin, corriendo como proceso de usuario normal) y se corrió todo en vivo contra él:
  - Se aplicó la migración `InitialCreate` de verdad con `dotnet ef database update`.
  - Se confirmó que la igualdad de `AirportCode` y el rango de `DateTimeOffset` en `FlightSearchReadModel` **sí traducen correctamente en Npgsql** (el problema que bloqueaba los 3 tests `Skip` con SQLite era específico del proveedor SQLite, no del diseño). Esos 3 tests ahora corren de verdad en `CatalogDbContextPostgresTests.cs` contra Postgres real (por default `localhost:5432`, override con `AEROAPP_TEST_POSTGRES_CONNECTION`).
  - Se encontró y corrigió un bug real nuevo: Npgsql rechaza un `DateTimeOffset` con offset distinto de cero contra `timestamp with time zone` — `Flight.DepartureAt`/`ArrivalAt` ahora se normalizan a UTC en `FlightConfiguration` al persistir (`HasConversion(dt => dt.ToUniversalTime(), dt => dt)`). Confirma que la hora local de un vuelo debe reconstruirse combinando el instante UTC guardado con el `TimeZone` del `Airport` de origen, nunca guardando un offset fijo.
  - Se probó `GET /api/flights` y `POST /api/flights/{id}/fare-classes` en vivo con `curl` contra datos sembrados por SQL directo, incluyendo el camino de error (409 al exceder la capacidad del `SeatMap`) — todo funcionó de punta a punta.
  - **El Postgres portátil sigue corriendo** en `127.0.0.1:5433` (fuera del repo, en un directorio temporal) como respaldo utilizable mientras se resuelve la instalación de Docker Desktop a mano (requiere que el usuario apruebe el diálogo de UAC).
- **2026-08-30 (endpoints de creación)**: se completó el lado de escritura de `Catalog.Service` que faltaba — hasta ahora solo se podía sembrar datos con SQL a mano. Se agregaron `CreateAirportCommand`, `CreateAirlineCommand`, `CreateAircraftTypeCommand` y `CreateFlightCommand` (este último cruza `Airline`/`Airport`×2/`AircraftType`, todos validados con `NotFoundException` antes de crear el vuelo), con sus controllers REST (`AirportsController`, `AirlinesController`, `AircraftTypesController`, y `POST /api/flights` en `FlightsController`). 16 tests nuevos (52 en total). Probado en vivo de punta a punta por HTTP contra Postgres real: crear 2 aeropuertos, 1 aerolínea, 1 tipo de aeronave, 1 vuelo, agregarle una tarifa, y encontrarlo en la búsqueda — todo sin una sola sentencia SQL manual. También confirmó el rechazo correcto de códigos duplicados (409) y referencias inexistentes (404).
