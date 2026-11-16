#!/usr/bin/env bash
# Arranca/para el AWS Emulator ("cuenta AWS") de un ambiente.
#
#   ./emulator.sh <dev|qa|preprod|prod> <start|stop|status|env>
#
#   start   arranca el emulador del ambiente y espera a que esté sano
#   stop    lo para (se pierde todo lo desplegado: vive en memoria)
#   status  dice si está sano
#   env     imprime las variables para usar la AWS CLI contra él:
#             eval "$(./emulator.sh dev env)"; aws s3 ls
#
# Si el comando `aws-emulator` está instalado (paquete .deb/.rpm, como en CI),
# corre nativo; si no (p. ej. Arch), corre en Docker con el Dockerfile oficial
# de emulator/. La telemetría del emulador va apagada.
set -euo pipefail

here="$(cd "$(dirname "$0")" && pwd)"
# shellcheck source-path=SCRIPTDIR
source "$here/lib.sh"

[[ $# -eq 2 ]] || { echo "Uso: $0 <dev|qa|preprod|prod> <start|stop|status|env>" >&2; exit 2; }
env_name=$1
action=$2
port=$(aws_env_port "$env_name")
container="aws-emulator-$env_name"
pidfile="$HOME/.aws/emulator/$env_name.pid"

wait_healthy() {
  for _ in $(seq 60); do
    aws_emulator_healthy "$port" && return 0
    sleep 1
  done
  echo "El emulador de $env_name no respondió en el puerto $port." >&2
  return 1
}

start() {
  if aws_emulator_healthy "$port"; then
    echo "El emulador de $env_name ya está corriendo en https://127.0.0.1:$port"
    return
  fi
  mkdir -p "$HOME/.aws/emulator"

  if command -v aws-emulator >/dev/null 2>&1; then
    nohup aws-emulator -p "$port" --telemetry off >"$HOME/.aws/emulator/$env_name.log" 2>&1 &
    echo $! >"$pidfile"
  else
    if ! docker image inspect aws-emulator >/dev/null 2>&1; then
      docker build -t aws-emulator "$here/emulator"
    fi
    docker run -d --rm --name "$container" \
      --memory=2g --memory-swap=2g \
      --user "$(id -u):$(id -g)" \
      -p "127.0.0.1:$port:$port" \
      -e HOME="$HOME" -e EMULATOR_TELEMETRY=off \
      -v "$HOME/.aws/emulator:$HOME/.aws/emulator" \
      aws-emulator -p "$port" --telemetry off >/dev/null
  fi

  wait_healthy
  echo "Emulador de $env_name listo en https://127.0.0.1:$port"
}

stop() {
  if docker container inspect "$container" >/dev/null 2>&1; then
    docker stop "$container" >/dev/null
  elif [[ -f $pidfile ]]; then
    kill "$(cat "$pidfile")" 2>/dev/null || true
    rm -f "$pidfile"
  else
    echo "El emulador de $env_name no está corriendo."
    return
  fi
  echo "Emulador de $env_name detenido (sus recursos se borraron)."
}

case "$action" in
  start) start ;;
  stop) stop ;;
  status)
    if aws_emulator_healthy "$port"; then echo "$env_name: sano en https://127.0.0.1:$port"
    else echo "$env_name: detenido"; exit 1; fi ;;
  env) aws_emulator_env_exports "$port" ;;
  *) echo "Acción desconocida: $action" >&2; exit 2 ;;
esac
