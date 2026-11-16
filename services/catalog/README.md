# Catalog.Service

Aeropuertos, aerolíneas, rutas, vuelos, horarios y tarifas. Esquema Postgres: `catalog`.
Clean Architecture: `Catalog.Domain` / `Catalog.Application` / `Catalog.Infrastructure` / `Catalog.Api`.
Se crea en la Fase 1.

## Datos de ejemplo (dev)

`scripts/seed-dev.sh` carga aeropuertos (LIM, BOG, MEX, SCL, MAD), dos aerolíneas
ficticias, un tipo de aeronave y vuelos diarios (hoy + 14 días) en 5 rutas de ida y
vuelta, cada uno con 3 clases de tarifa. Usa solo la API pública (bash + curl + python3)
y es idempotente: un 409 en aeropuertos/aerolíneas cuenta como "ya existe" y los vuelos
se buscan antes de crearse. Ante cualquier otro error sale con código ≠ 0.

Desde `services/catalog`, con el Postgres de dev levantado (puerto 5432):

```bash
dotnet tool restore
dotnet ef database update --project src/Catalog.Infrastructure --startup-project src/Catalog.Api
dotnet run --project src/Catalog.Api --launch-profile http &   # API en http://localhost:5095
scripts/seed-dev.sh                       # o: scripts/seed-dev.sh http://otro-host:5095
curl -s "http://localhost:5095/api/flights?origin=LIM&destination=BOG&departureDate=$(date -d tomorrow +%F)"
```

Las salidas se fijan en horas UTC de mediodía/tarde para que la fecha local del
origen coincida con la fecha sembrada.
