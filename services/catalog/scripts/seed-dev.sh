#!/usr/bin/env bash
# Siembra datos de ejemplo en Catalog usando solo la API pública. Idempotente.
# Uso: scripts/seed-dev.sh [api-base-url]   (por defecto http://localhost:5095)
set -euo pipefail

BASE_URL="${1:-http://localhost:5095}"
BASE_URL="${BASE_URL%/}"
DAYS=14  # hoy + los próximos 14 días (fecha local de la máquina)

created=0
existing=0
BODY=""
HTTP_CODE=""

die() {
  echo "ERROR: $*" >&2
  exit 1
}

# api METHOD PATH [JSON] -> deja la respuesta en BODY y el estado en HTTP_CODE.
api() {
  local method="$1" path="$2" data="${3:-}" out
  local args=(-sS -X "$method" -w $'\n%{http_code}' "$BASE_URL$path")
  if [[ -n "$data" ]]; then
    args+=(-H 'Content-Type: application/json' -d "$data")
  fi
  out="$(curl "${args[@]}")" || die "no se pudo contactar $BASE_URL$path"
  HTTP_CODE="${out##*$'\n'}"
  BODY="${out%$'\n'*}"
}

# json_get <campo> lee un campo de nivel raíz de BODY.
json_get() {
  printf '%s' "$BODY" | python3 -c 'import json,sys; print(json.load(sys.stdin)[sys.argv[1]])' "$1"
}

# post_or_exists PATH JSON ETIQUETA: 201 = creado, 409 = ya existía, otro = error.
post_or_exists() {
  api POST "$1" "$2"
  case "$HTTP_CODE" in
    201) created=$((created + 1)) ;;
    409) existing=$((existing + 1)) ;;
    *) die "POST $1 ($3) devolvió HTTP $HTTP_CODE: $BODY" ;;
  esac
}

# --- Aeropuertos: código|nombre|ciudad|país|zona horaria
airports=(
  "LIM|Aeropuerto Internacional Jorge Chávez|Lima|Perú|America/Lima"
  "BOG|Aeropuerto Internacional El Dorado|Bogotá|Colombia|America/Bogota"
  "MEX|Aeropuerto Internacional Benito Juárez|Ciudad de México|México|America/Mexico_City"
  "SCL|Aeropuerto Internacional Arturo Merino Benítez|Santiago|Chile|America/Santiago"
  "MAD|Aeropuerto Adolfo Suárez Madrid-Barajas|Madrid|España|Europe/Madrid"
)

echo "== Aeropuertos"
for row in "${airports[@]}"; do
  IFS='|' read -r code name city country tz <<<"$row"
  payload="$(python3 -c 'import json,sys; k=["code","name","city","country","timeZoneId"]; print(json.dumps(dict(zip(k,sys.argv[1:]))))' \
    "$code" "$name" "$city" "$country" "$tz")"
  post_or_exists /api/airports "$payload" "$code"
done

# --- Aerolíneas ficticias
airlines=(
  "AX|Andes Express"
  "SC|Sol Cruzado"
)

echo "== Aerolíneas"
for row in "${airlines[@]}"; do
  IFS='|' read -r code name <<<"$row"
  payload="$(python3 -c 'import json,sys; print(json.dumps({"code":sys.argv[1],"name":sys.argv[2]}))' "$code" "$name")"
  post_or_exists /api/airlines "$payload" "$code"
done

# --- Rutas: aerolínea|origen|destino|nº ida|nº vuelta|hora de salida UTC ida|hora UTC vuelta|duración (min)
# Las salidas son a mediodía/tarde UTC para que el día local del origen coincida
# con el día UTC en todas las zonas (así la búsqueda por fecha es predecible).
routes=(
  "AX|LIM|BOG|101|102|14|18|200"
  "AX|BOG|MEX|201|202|13|19|270"
  "SC|SCL|LIM|301|302|12|17|230"
  "SC|MEX|MAD|401|402|15|21|690"
  "AX|LIM|SCL|501|502|16|20|235"
)

AIRCRAFT_TYPE_ID=""

# Reutiliza el tipo de aeronave de un vuelo ya sembrado; si no hay, lo crea (una vez).
ensure_aircraft_type() {
  [[ -n "$AIRCRAFT_TYPE_ID" ]] && return 0
  local payload
  payload='{"model":"Seed A320-200","seatMap":{"Economy":150,"PremiumEconomy":12,"Business":18}}'
  api POST /api/aircraft-types "$payload"
  [[ "$HTTP_CODE" == "201" ]] || die "POST /api/aircraft-types devolvió HTTP $HTTP_CODE: $BODY"
  AIRCRAFT_TYPE_ID="$(json_get aircraftTypeId)"
  created=$((created + 1))
}

# find_flight ORIGEN DESTINO FECHA NUMERO -> imprime el id del vuelo si ya existe.
find_flight() {
  api GET "/api/flights?origin=$1&destination=$2&departureDate=$3"
  [[ "$HTTP_CODE" == "200" ]] || die "GET búsqueda $1-$2 $3 devolvió HTTP $HTTP_CODE: $BODY"
  printf '%s' "$BODY" | python3 -c '
import json, sys
num = sys.argv[1]
for f in json.load(sys.stdin):
    digits = "".join(c for c in f["flightNumber"] if c.isdigit())
    if digits.lstrip("0") == num:
        print(f["flightId"])
        break
' "$4"
}

# Reutiliza el tipo de aeronave de cualquier vuelo existente (evita duplicarlo al re-ejecutar).
adopt_existing_aircraft_type() {
  local first_date="$1" row airline origin dest n1 rest id
  for row in "${routes[@]}"; do
    IFS='|' read -r airline origin dest n1 rest <<<"$row"
    id="$(find_flight "$origin" "$dest" "$first_date" "$n1")"
    if [[ -n "$id" ]]; then
      api GET "/api/flights/$id"
      [[ "$HTTP_CODE" == "200" ]] || die "GET /api/flights/$id devolvió HTTP $HTTP_CODE: $BODY"
      AIRCRAFT_TYPE_ID="$(json_get aircraftTypeId)"
      return 0
    fi
  done
}

# seed_flight AEROLINEA ORIGEN DESTINO NUMERO FECHA HORA_UTC DURACION_MIN
seed_flight() {
  local airline="$1" origin="$2" dest="$3" number="$4" day="$5" hour="$6" minutes="$7"
  local found dep arr payload flight_id
  found="$(find_flight "$origin" "$dest" "$day" "$number")"
  if [[ -n "$found" ]]; then
    existing=$((existing + 1))
    return 0
  fi
  ensure_aircraft_type
  dep="$(printf '%sT%02d:00:00Z' "$day" "$hour")"
  arr="$(date -u -d "$dep + $minutes minutes" +%Y-%m-%dT%H:%M:%SZ)"
  payload="$(python3 -c '
import json, sys
a = sys.argv[1:]
print(json.dumps({"airlineCode": a[0], "flightNumber": int(a[1]), "origin": a[2], "destination": a[3],
                  "aircraftTypeId": a[4], "departureAt": a[5], "arrivalAt": a[6]}))
' "$airline" "$number" "$origin" "$dest" "$AIRCRAFT_TYPE_ID" "$dep" "$arr")"
  api POST /api/flights "$payload"
  case "$HTTP_CODE" in
    201) ;;
    409)
      existing=$((existing + 1))
      return 0
      ;;
    *) die "POST /api/flights ($airline$number $day) devolvió HTTP $HTTP_CODE: $BODY" ;;
  esac
  flight_id="$(json_get flightId)"
  created=$((created + 1))

  # Tarifas: precio base según la duración, con 3 cabinas.
  local base=$((minutes / 2 + 40)) cabin price seats
  for spec in "Economy|1|120" "PremiumEconomy|2|8" "Business|4|12"; do
    IFS='|' read -r cabin mult seats <<<"$spec"
    price=$((base * mult))
    api POST "/api/flights/$flight_id/fare-classes" \
      "{\"cabin\":\"$cabin\",\"price\":$price,\"currency\":\"USD\",\"totalSeats\":$seats}"
    [[ "$HTTP_CODE" == "201" ]] || die "POST fare-classes ($airline$number $cabin) devolvió HTTP $HTTP_CODE: $BODY"
  done
}

echo "== Vuelos ($DAYS días, ${#routes[@]} rutas ida y vuelta)"
adopt_existing_aircraft_type "$(date -d tomorrow +%F)"

for ((d = 0; d <= DAYS; d++)); do
  day="$(date -d "+$d day" +%F)"
  for row in "${routes[@]}"; do
    IFS='|' read -r airline origin dest n_out n_back h_out h_back minutes <<<"$row"
    seed_flight "$airline" "$origin" "$dest" "$n_out" "$day" "$h_out" "$minutes"
    seed_flight "$airline" "$dest" "$origin" "$n_back" "$day" "$h_back" "$minutes"
  done
done

echo "== Resumen: $created creados, $existing ya existentes (API: $BASE_URL)"
