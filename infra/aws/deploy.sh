#!/usr/bin/env bash
# Despliega un artefacto de AeroApp en la "cuenta AWS" (AWS Emulator) de un ambiente.
#
#   ./deploy.sh <dev|qa|preprod|prod> <carpeta-artefacto>
#
# La carpeta del artefacto es la que produce el pipeline (deploy.yml) o
# build-artifact.sh:
#   catalog-api/   salida de dotnet publish
#   web/           build de Angular
#   web-config/    config.json por ambiente (web/config/)
#
# Pasos:
#   1. CloudFormation: crea/actualiza el stack `aeroapp` (buckets, ECR, SSM, secreto).
#   2. Web: sube el build a s3://aeroapp-web-<amb> con el config.json del ambiente.
#   3. API: sube catalog-api/<versión>.zip a s3://aeroapp-releases-<amb> y marca
#      esa versión como la actual en SSM (/aeroapp/catalog-api/release).
#   4. Verifica lo desplegado.
#
# Solo despliega contra el emulador: si la AWS CLI no apunta a 127.0.0.1 con la
# cuenta 000000000000, se detiene. Nunca toca AWS real.
set -euo pipefail

here="$(cd "$(dirname "$0")" && pwd)"
# shellcheck source-path=SCRIPTDIR
source "$here/lib.sh"

[[ $# -eq 2 ]] || { echo "Uso: $0 <dev|qa|preprod|prod> <carpeta-artefacto>" >&2; exit 2; }
env_name=$1
artifact="$(cd "$2" && pwd)"
port=$(aws_env_port "$env_name")
aspnetcore=$(aws_env_aspnetcore "$env_name")
version=${RELEASE_VERSION:-$(git -C "$here" rev-parse --short HEAD 2>/dev/null || date +%Y%m%d%H%M%S)}
stack=aeroapp

for dir in catalog-api web web-config; do
  [[ -d $artifact/$dir ]] || { echo "Al artefacto le falta $dir/ ($artifact)" >&2; exit 1; }
done

# --- Conectar con el emulador del ambiente y comprobar que NO es AWS real ---
eval "$(aws_emulator_env_exports "$port")"
aws_emulator_healthy "$port" || {
  echo "El emulador de $env_name no está corriendo. Arráncalo con: infra/aws/emulator.sh $env_name start" >&2
  exit 1
}
account=$(aws sts get-caller-identity --query Account --output text)
if [[ $AWS_ENDPOINT_URL != https://127.0.0.1:* || $account != 000000000000 ]]; then
  echo "ABORTADO: la AWS CLI no apunta al emulador (endpoint=$AWS_ENDPOINT_URL, cuenta=$account)." >&2
  exit 1
fi
echo "==> $env_name: cuenta emulada $account en $AWS_ENDPOINT_URL (versión $version)"

# --- Config del frontend para este ambiente ---
if [[ $env_name == dev ]]; then
  web_config="$artifact/web/config.json" # el build ya trae el de dev
else
  web_config="$artifact/web-config/$env_name/config.json"
fi
api_base_url=$(python3 -c 'import json,sys; print(json.load(open(sys.argv[1]))["apiBaseUrl"])' "$web_config")

# --- 1. Infraestructura ---
echo "==> CloudFormation: stack $stack"
aws cloudformation deploy \
  --stack-name "$stack" \
  --template-file "$here/cloudformation/aeroapp.yaml" \
  --parameter-overrides "EnvName=$env_name" "AspNetCoreEnvironment=$aspnetcore" "ApiBaseUrl=$api_base_url" \
  --tags "Environment=$env_name" "Release=$version" \
  --no-fail-on-empty-changeset

output() {
  aws cloudformation describe-stacks --stack-name "$stack" \
    --query "Stacks[0].Outputs[?OutputKey=='$1'].OutputValue" --output text
}
web_bucket=$(output WebBucketName)
releases_bucket=$(output ReleasesBucketName)

# --- 2. Web ---
echo "==> Web -> s3://$web_bucket"
staging=$(mktemp -d)
trap 'rm -rf "$staging"' EXIT
cp -r "$artifact/web/." "$staging/"
cp "$web_config" "$staging/config.json"
aws s3 sync "$staging" "s3://$web_bucket" --delete --only-show-errors
# config.json e index.html no se cachean: cambian en cada despliegue.
for f in config.json index.html; do
  aws s3 cp "$staging/$f" "s3://$web_bucket/$f" --cache-control "no-cache" --only-show-errors
done

# --- 3. API ---
echo "==> Catalog.Api -> s3://$releases_bucket/catalog-api/$version.zip"
(cd "$artifact/catalog-api" && python3 -m zipfile -c "$staging/catalog-api.zip" .)
aws s3 cp "$staging/catalog-api.zip" "s3://$releases_bucket/catalog-api/$version.zip" --only-show-errors
aws ssm put-parameter --name /aeroapp/catalog-api/release --type String --value "$version" --overwrite >/dev/null

# --- 4. Verificación ---
echo "==> Verificando"
deployed_env=$(aws s3 cp "s3://$web_bucket/config.json" - | python3 -c 'import json,sys; print(json.load(sys.stdin)["environment"])')
[[ $deployed_env == "$env_name" ]] || { echo "config.json desplegado dice '$deployed_env', se esperaba '$env_name'" >&2; exit 1; }
aws s3api head-object --bucket "$releases_bucket" --key "catalog-api/$version.zip" >/dev/null
[[ $(aws ssm get-parameter --name /aeroapp/catalog-api/aspnetcore-environment --query Parameter.Value --output text) == "$aspnetcore" ]]
[[ $(aws ssm get-parameter --name /aeroapp/catalog-api/release --query Parameter.Value --output text) == "$version" ]]
aws secretsmanager describe-secret --secret-id aeroapp/catalog-api/db --query Name --output text >/dev/null

cat <<EOF
==> Desplegado en $env_name
    web:      s3://$web_bucket ($(aws s3 ls "s3://$web_bucket" --recursive | wc -l) archivos, config.json environment=$deployed_env)
    api:      s3://$releases_bucket/catalog-api/$version.zip (release actual: $version)
    config:   SSM /aeroapp/catalog-api/aspnetcore-environment = $aspnetcore
    secreto:  Secrets Manager aeroapp/catalog-api/db
EOF
