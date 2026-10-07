#!/bin/bash
set -euo pipefail

PROJECT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
OUTPUT_DIR="$PROJECT_DIR/publicado"
STAGING_DIR="$(mktemp -d "${TMPDIR:-/tmp}/factiox-publicado.XXXXXX")"
trap 'rm -rf "$STAGING_DIR"' EXIT

cd "$PROJECT_DIR"

printf '%s\n' 'Reconstruyendo FactioX para IIS Windows x64...'
dotnet clean FactioX.csproj -c Release -r win-x64 '-clp:ErrorsOnly;Summary'
dotnet publish FactioX.csproj -c Release -r win-x64 --self-contained false \
    -p:UseAppHost=false -p:IsTransformWebConfigDisabled=true \
    -o "$STAGING_DIR" '-clp:ErrorsOnly;Summary'

find "$STAGING_DIR" -type f \( -iname 'web.config' -o -iname 'appsettings*.json' \) -delete
rsync -aR tessdata Database/AddAbonosCompraVenta.sql ABONOS_README.md "$STAGING_DIR/"

for required in FactioX.dll FactioX.deps.json FactioX.runtimeconfig.json \
    wwwroot/app.css wwwroot/js/pwa-install.js tessdata/spa.traineddata \
    Database/AddAbonosCompraVenta.sql; do
    if [[ ! -s "$STAGING_DIR/$required" ]]; then
        printf 'Falta un archivo necesario: %s\n' "$required" >&2
        exit 1
    fi
done

if [[ -n "$(find "$STAGING_DIR" -type f \( -iname 'web.config' -o -iname 'appsettings*.json' -o -iname '*.zip' \) -print -quit)" ]]; then
    printf '%s\n' 'La publicacion contiene configuraciones o ZIP no permitidos.' >&2
    exit 1
fi

mkdir -p "$OUTPUT_DIR"
rsync -a --delete "$STAGING_DIR/" "$OUTPUT_DIR/"
diff -qr "$STAGING_DIR" "$OUTPUT_DIR"

printf '\n%s\n' "Publicacion actualizada: $OUTPUT_DIR"
printf '%s\n' 'Sin ZIP, web.config ni appsettings. Conserve los existentes en IIS.'
printf '%s\n' 'Copie el contenido de publicado sobre la aplicacion detenida, conservando los archivos de usuarios.'