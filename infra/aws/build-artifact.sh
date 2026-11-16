#!/usr/bin/env bash
# Construye localmente el mismo artefacto que el job "build" de deploy.yml.
#
#   ./build-artifact.sh [carpeta-salida]     (por defecto: out/ en la raíz del repo)
#
# Requiere web/node_modules instalado en esta máquina (cd web && npm ci).
set -euo pipefail

root="$(cd "$(dirname "$0")/../.." && pwd)"
out="${1:-$root/out}"

rm -rf "$out"
mkdir -p "$out"

dotnet publish "$root/services/catalog/src/Catalog.Api" -c Release -o "$out/catalog-api"

(cd "$root/web" && npx ng build)
cp -r "$root/web/dist/aeroapp-web/browser" "$out/web"
cp -r "$root/web/config" "$out/web-config"

echo "Artefacto listo en $out"
