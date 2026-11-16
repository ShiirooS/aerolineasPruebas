#!/usr/bin/env bash
# Levanta la infraestructura local (Postgres, Redis, Keycloak) de un ambiente.
#
#   ./env.sh <dev|qa|preprod|prod> <up|down|ps|logs|config>
#
# Cada ambiente es un stack de compose independiente (contenedores, volúmenes
# y puertos propios), así que pueden correr varios a la vez:
#   dev      aeroapp-*          5432 / 6379 / 8080
#   qa       aeroapp-qa-*      15432 / 16379 / 18080
#   preprod  aeroapp-preprod-* 25432 / 26379 / 28080  (+ docker-compose.prod.yml)
#   prod     aeroapp-prod-*    35432 / 36379 / 38080  (+ docker-compose.prod.yml)
#
# Lee infra/.env.<ambiente> (créalo desde .env.<ambiente>.example).
# `down` nunca borra volúmenes: para eso, docker compose ... down -v a mano.
set -euo pipefail

cd "$(dirname "$0")"

usage() {
  echo "Uso: $0 <dev|qa|preprod|prod> <up|down|ps|logs|config>" >&2
  exit 2
}

[[ $# -eq 2 ]] || usage
env_name=$1
action=$2

case "$env_name" in
  dev|qa) files=(-f docker-compose.yml) ;;
  preprod|prod) files=(-f docker-compose.yml -f docker-compose.prod.yml) ;;
  *) usage ;;
esac

env_file=".env.$env_name"
if [[ ! -f $env_file ]]; then
  echo "Falta infra/$env_file. Créalo con: cp $env_file.example $env_file" >&2
  exit 1
fi

if [[ $env_name == preprod || $env_name == prod ]] && grep -q 'CHANGE_ME' "$env_file"; then
  echo "infra/$env_file todavía tiene valores CHANGE_ME. Reemplázalos antes de levantar $env_name." >&2
  exit 1
fi

compose=(docker compose --env-file "$env_file" "${files[@]}")

case "$action" in
  up) "${compose[@]}" up -d ;;
  down) "${compose[@]}" down ;;
  ps) "${compose[@]}" ps ;;
  logs) "${compose[@]}" logs -f --tail=100 ;;
  config) "${compose[@]}" config ;;
  *) usage ;;
esac
