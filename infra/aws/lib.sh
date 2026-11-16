# shellcheck shell=bash
# Funciones compartidas por emulator.sh y deploy.sh. No se ejecuta solo.
#
# Un AWS Emulator por ambiente, cada uno en su puerto: es el equivalente local
# de "una cuenta AWS por ambiente". El emulador guarda sus credenciales y su
# certificado en ~/.aws/emulator/<puerto>/.

aws_env_port() {
  case "$1" in
    dev) echo 9297 ;;
    qa) echo 9298 ;;
    preprod) echo 9299 ;;
    prod) echo 9300 ;;
    *) echo "Ambiente desconocido: $1 (usa dev, qa, preprod o prod)" >&2; return 2 ;;
  esac
}

aws_env_aspnetcore() {
  case "$1" in
    dev) echo Development ;;
    qa) echo QA ;;
    preprod) echo Preprod ;;
    prod) echo Production ;;
    *) return 2 ;;
  esac
}

# Variables para que la AWS CLI hable con el emulador del puerto dado.
# Fuente: https://docs.aws.amazon.com/emulator/latest/userguide/point-sdk-to-emulator.html
aws_emulator_env_exports() {
  local port=$1 dir="$HOME/.aws/emulator/$1"
  cat <<EOF
unset AWS_ACCESS_KEY_ID AWS_SECRET_ACCESS_KEY AWS_SESSION_TOKEN AWS_PROFILE
export AWS_ENDPOINT_URL=https://127.0.0.1:$port
export AWS_SHARED_CREDENTIALS_FILE=$dir/emulator_credentials
export AWS_CONFIG_FILE=$dir/config
export AWS_CA_BUNDLE=$dir/ca.pem
export AWS_EC2_METADATA_DISABLED=true
export AWS_DISABLE_HOST_PREFIX_INJECTION=true
export AWS_REGION=us-east-1
export AWS_DEFAULT_REGION=us-east-1
EOF
}

aws_emulator_healthy() {
  local port=$1
  curl -sf --cacert "$HOME/.aws/emulator/$port/ca.pem" "https://127.0.0.1:$port/__admin/health" >/dev/null 2>&1
}
