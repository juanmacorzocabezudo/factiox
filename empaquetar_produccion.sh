#!/bin/bash
# Script de empaquetado para producción - FactioX
# Fecha: 10 de marzo de 2026

echo "=========================================="
echo "  📦 EMPAQUETADO PARA PRODUCCIÓN"
echo "  FactioX - $(date '+%Y-%m-%d %H:%M:%S')"
echo "=========================================="
echo ""

# Variables
FECHA=$(date +%Y%m%d_%H%M%S)
CARPETA_PUBLISH="./publish"
ARCHIVO_ZIP="FactioX_Produccion_${FECHA}.zip"
BACKUP_BD="factiox_backup_${FECHA}.sql"

# 1. Verificar que existe la carpeta publish
if [ ! -d "$CARPETA_PUBLISH" ]; then
    echo "❌ Error: No existe la carpeta ./publish"
    echo "   Ejecuta primero: dotnet publish -c Release -o ./publish"
    exit 1
fi

echo "✅ Carpeta de publicación encontrada"
echo ""

# 2. Preguntar si hacer backup de BD
echo "¿Deseas crear un backup de la base de datos actual? (s/n)"
read -r RESPUESTA_BD

if [[ $RESPUESTA_BD == "s" || $RESPUESTA_BD == "S" ]]; then
    echo ""
    echo "📊 Creando backup de base de datos..."
    echo "   Ingresa la contraseña de MySQL cuando se solicite"
    
    # Preguntar credenciales
    read -p "Usuario de MySQL [root]: " MYSQL_USER
    MYSQL_USER=${MYSQL_USER:-root}
    
    read -p "Nombre de la base de datos [FactioX]: " MYSQL_DB
    MYSQL_DB=${MYSQL_DB:-FactioX}
    
    # Crear backup
    mysqldump -u $MYSQL_USER -p $MYSQL_DB > $BACKUP_BD
    
    if [ $? -eq 0 ]; then
        echo "✅ Backup creado: $BACKUP_BD"
        echo "   Tamaño: $(du -h $BACKUP_BD | cut -f1)"
    else
        echo "⚠️  No se pudo crear el backup (continuando sin él)"
        BACKUP_BD=""
    fi
else
    echo "⏭️  Saltando backup de base de datos"
    BACKUP_BD=""
fi

echo ""
echo "📦 Empaquetando archivos..."

# 3. Crear archivo ZIP con los archivos necesarios
cd "$(dirname "$0")"

# Crear lista de archivos a incluir
ARCHIVOS_A_INCLUIR=(
    "$CARPETA_PUBLISH/*"
    "appsettings.Production.json"
    "DESPLIEGUE_PRODUCCION.md"
    "CHECKLIST_PRODUCCION.md"
    "Database/FactioX_MySQL_Schema.sql"
)

# Si hay backup, añadirlo
if [ -n "$BACKUP_BD" ]; then
    ARCHIVOS_A_INCLUIR+=("$BACKUP_BD")
fi

# Crear el ZIP
zip -r "$ARCHIVO_ZIP" ${ARCHIVOS_A_INCLUIR[@]} -x "*.DS_Store" -x "*/.git/*" > /dev/null 2>&1

if [ $? -eq 0 ]; then
    echo "✅ Archivo creado: $ARCHIVO_ZIP"
    echo "   Tamaño: $(du -h $ARCHIVO_ZIP | cut -f1)"
else
    echo "❌ Error al crear el archivo ZIP"
    exit 1
fi

echo ""
echo "=========================================="
echo "  ✅ EMPAQUETADO COMPLETADO"
echo "=========================================="
echo ""
echo "📋 Resumen:"
echo "   • Archivo: $ARCHIVO_ZIP"
echo "   • Ubicación: $(pwd)/$ARCHIVO_ZIP"
if [ -n "$BACKUP_BD" ]; then
    echo "   • Backup BD incluido: Sí"
else
    echo "   • Backup BD incluido: No"
fi
echo ""
echo "📤 Próximos pasos:"
echo "   1. Sube este archivo a tu servidor de producción"
echo "   2. Descomprime: unzip $ARCHIVO_ZIP"
echo "   3. Sigue las instrucciones en DESPLIEGUE_PRODUCCION.md"
echo "   4. Usa CHECKLIST_PRODUCCION.md para verificar"
echo ""
echo "⚠️  IMPORTANTE:"
echo "   No olvides configurar appsettings.Production.json"
echo "   con las credenciales reales del servidor"
echo ""
echo "=========================================="
