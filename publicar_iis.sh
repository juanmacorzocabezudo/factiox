#!/bin/bash

# Script de Publicación para IIS - FactioX
# Fecha: $(date +"%Y-%m-%d %H:%M:%S")

echo "=========================================="
echo "  📦 Publicando FactioX para IIS"
echo "=========================================="
echo ""

# 1. Limpiar publicaciones anteriores
echo "🧹 Limpiando publicación anterior..."
rm -rf ./publish_iis
mkdir -p ./publish_iis

# 2. Publicar aplicación en modo Release
echo ""
echo "⚙️  Compilando aplicación en modo Release..."
dotnet publish -c Release -o ./publish_iis --no-self-contained

if [ $? -ne 0 ]; then
    echo "❌ Error al compilar la aplicación"
    exit 1
fi

echo "✅ Compilación completada"

# 3. Copiar archivos adicionales necesarios
echo ""
echo "📁 Copiando archivos adicionales..."

# Copiar tessdata para OCR manual
if [ -d "./tessdata" ]; then
    echo "  - Copiando tessdata para OCR..."
    cp -r ./tessdata ./publish_iis/
fi

# Copiar documentación de producción
echo "  - Copiando documentación..."
cp -f ./DESPLIEGUE_PRODUCCION.md ./publish_iis/ 2>/dev/null || true
cp -f ./CHECKLIST_PRODUCCION.md ./publish_iis/ 2>/dev/null || true

# 4. Crear web.config para IIS
echo ""
echo "🔧 Generando web.config para IIS..."
cat > ./publish_iis/web.config << 'EOF'
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <location path="." inheritInChildApplications="false">
    <system.webServer>
      <handlers>
        <add name="aspNetCore" path="*" verb="*" modules="AspNetCoreModuleV2" resourceType="Unspecified" />
      </handlers>
      <aspNetCore processPath="dotnet" 
                  arguments=".\FactioX.dll" 
                  stdoutLogEnabled="true" 
                  stdoutLogFile=".\logs\stdout" 
                  hostingModel="inprocess">
        <environmentVariables>
          <environmentVariable name="ASPNETCORE_ENVIRONMENT" value="Production" />
        </environmentVariables>
      </aspNetCore>
      <httpProtocol>
        <customHeaders>
          <remove name="X-Powered-By" />
        </customHeaders>
      </httpProtocol>
      <staticContent>
        <remove fileExtension=".woff" />
        <remove fileExtension=".woff2" />
        <mimeMap fileExtension=".woff" mimeType="application/font-woff" />
        <mimeMap fileExtension=".woff2" mimeType="font/woff2" />
        <mimeMap fileExtension=".webmanifest" mimeType="application/manifest+json" />
        <clientCache cacheControlMode="UseMaxAge" cacheControlMaxAge="7.00:00:00" />
      </staticContent>
    </system.webServer>
  </location>
</configuration>
EOF

# 5. Crear carpeta de logs
echo ""
echo "📝 Creando carpeta de logs..."
mkdir -p ./publish_iis/logs

# 6. Crear archivo .env de ejemplo
echo ""
echo "🔐 Creando archivo de configuración de ejemplo..."
cat > ./publish_iis/.env.example << 'EOF'
# Configuración de Producción - FactioX
# Copia este archivo a .env y configura los valores correctos

# Base de Datos
DB_SERVER=tu-servidor-mysql
DB_DATABASE=FactioX
DB_USER=tu-usuario
DB_PASSWORD=tu-password
DB_PORT=3306

# Email
SMTP_HOST=smtp.gmail.com
SMTP_PORT=587
SMTP_USER=tu-email@gmail.com
SMTP_PASSWORD=tu-password-app
FROM_EMAIL=tu-email@gmail.com
FROM_NAME=FactioX

# Ollama (Opcional - para OCR con IA)
OLLAMA_URL=http://127.0.0.1:11434
OLLAMA_MODEL=llama3.2:3b
OLLAMA_VISION_MODEL=llava
EOF

# 7. Crear guía rápida de instalación en IIS
echo ""
echo "📖 Creando guía de instalación IIS..."
cat > ./publish_iis/INSTALAR_IIS.md << 'EOF'
# 🚀 Instalación en IIS - FactioX

## Requisitos Previos

1. **Windows Server** con IIS instalado
2. **.NET 8.0 Hosting Bundle** instalado
   - Descarga: https://dotnet.microsoft.com/download/dotnet/8.0
3. **MySQL Server** instalado y funcionando
4. **URL Rewrite Module** para IIS (para PWA)
   - Descarga: https://www.iis.net/downloads/microsoft/url-rewrite

## Pasos de Instalación

### 1. Verificar .NET 8.0 Runtime

Abre PowerShell como Administrador:
```powershell
dotnet --list-runtimes
```

Debe aparecer `Microsoft.AspNetCore.App 8.0.x`

Si no está instalado:
```powershell
# Descargar e instalar el Hosting Bundle
# https://dotnet.microsoft.com/download/dotnet/8.0
```

### 2. Preparar Carpeta de la Aplicación

```powershell
# Copiar todos los archivos a:
C:\inetpub\wwwroot\FactioX

# O crear una carpeta personalizada:
mkdir C:\Apps\FactioX
# Copiar archivos ahí
```

### 3. Configurar Permisos

```powershell
cd C:\inetpub\wwwroot\FactioX  # O tu ruta

# Dar permisos al Application Pool
icacls . /grant "IIS AppPool\FactioX":(OI)(CI)M
icacls wwwroot /grant "IIS AppPool\FactioX":(OI)(CI)M
icacls logs /grant "IIS AppPool\FactioX":(OI)(CI)M

# Si usas tessdata para OCR
icacls tessdata /grant "IIS AppPool\FactioX":(OI)(CI)R
```

### 4. Configurar Base de Datos

Edita `appsettings.Production.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=TU_SERVIDOR;Database=FactioX;User=TU_USUARIO;Password=TU_PASSWORD;Port=3306;"
  }
}
```

Importa la base de datos:
```powershell
mysql -u root -p FactioX < factiox_backup_PRODUCCION.sql
```

### 5. Crear Application Pool en IIS

1. Abre **IIS Manager**
2. Click derecho en **Application Pools** → **Add Application Pool**
3. Configuración:
   - **Name:** FactioX
   - **.NET CLR version:** No Managed Code
   - **Managed pipeline mode:** Integrated
   - **Start application pool immediately:** ✓

4. Click derecho en el pool creado → **Advanced Settings**:
   - **Identity:** ApplicationPoolIdentity
   - **Start Mode:** AlwaysRunning
   - **Idle Time-out (minutes):** 0
   - **Regular Time Interval (minutes):** 0

### 6. Crear Sitio Web en IIS

1. Click derecho en **Sites** → **Add Website**
2. Configuración:
   - **Site name:** FactioX
   - **Application pool:** FactioX (el que acabas de crear)
   - **Physical path:** C:\inetpub\wwwroot\FactioX (o tu ruta)
   - **Binding:**
     - Type: http
     - IP address: All Unassigned
     - Port: 80
     - Host name: tudominio.com (opcional)

3. Click **OK**

### 7. Verificar web.config

El archivo `web.config` debe existir con este contenido mínimo:
```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <location path="." inheritInChildApplications="false">
    <system.webServer>
      <handlers>
        <add name="aspNetCore" path="*" verb="*" modules="AspNetCoreModuleV2" resourceType="Unspecified" />
      </handlers>
      <aspNetCore processPath="dotnet" 
                  arguments=".\FactioX.dll" 
                  stdoutLogEnabled="true" 
                  stdoutLogFile=".\logs\stdout" 
                  hostingModel="inprocess">
        <environmentVariables>
          <environmentVariable name="ASPNETCORE_ENVIRONMENT" value="Production" />
        </environmentVariables>
      </aspNetCore>
    </system.webServer>
  </location>
</configuration>
```

### 8. Reiniciar IIS

```powershell
iisreset /restart
```

### 9. Verificar que Funciona

Abre el navegador y ve a:
- `http://localhost` (si es local)
- `http://tu-servidor` (si es remoto)
- `http://tudominio.com` (si configuraste un dominio)

Deberías ver la página de login de FactioX.

## 🔒 Configurar HTTPS (Recomendado)

### Con Certificado SSL

1. En IIS Manager → Tu sitio → **Bindings**
2. Click **Add**
3. Configuración:
   - Type: https
   - Port: 443
   - SSL certificate: Selecciona tu certificado
4. Click **OK**

### Forzar HTTPS

Agrega en `web.config` dentro de `<system.webServer>`:
```xml
<rewrite>
  <rules>
    <rule name="HTTPS Redirect" stopProcessing="true">
      <match url="(.*)" />
      <conditions>
        <add input="{HTTPS}" pattern="^OFF$" />
      </conditions>
      <action type="Redirect" url="https://{HTTP_HOST}/{R:1}" redirectType="Permanent" />
    </rule>
  </rules>
</rewrite>
```

## 🐛 Solución de Problemas

### Error 500.19 - Cannot read configuration file

**Causa:** Permisos incorrectos o falta ASP.NET Core Module

**Solución:**
```powershell
# Reparar instalación de .NET Hosting Bundle
# Reinstalar desde: https://dotnet.microsoft.com/download/dotnet/8.0
```

### Error 502.5 - Process Failure

**Causa:** La aplicación no puede iniciar

**Solución:**
1. Verifica los logs en `./logs/stdout_*.log`
2. Verifica la cadena de conexión en `appsettings.Production.json`
3. Verifica que MySQL esté funcionando

### La aplicación se reinicia constantemente

**Causa:** Error de conexión a BD o configuración incorrecta

**Solución:**
1. Revisa `logs\stdout_*.log`
2. Verifica que la base de datos exista
3. Verifica credenciales de MySQL

### OCR no funciona

**Causa:** Tesseract no encuentra los archivos de idioma

**Solución:**
```powershell
# Verifica que la carpeta tessdata existe
dir tessdata

# Debe contener spa.traineddata
dir tessdata\*.traineddata
```

## 📞 Soporte

Para más información, consulta:
- `DESPLIEGUE_PRODUCCION.md`
- `SOLUCIONAR_ERROR_500_IIS.md`
- `DIAGNOSTICO_ERROR_500.md`
EOF

# 8. Mostrar resumen
echo ""
echo "=========================================="
echo "  ✅ Publicación Completada"
echo "=========================================="
echo ""
echo "📦 Carpeta de salida: ./publish_iis"
echo ""
echo "Contenido generado:"
echo "  ✓ Aplicación compilada (Release)"
echo "  ✓ web.config para IIS"
echo "  ✓ Carpeta de logs"
echo "  ✓ Datos de Tesseract (OCR)"
echo "  ✓ Documentación de instalación"
echo "  ✓ Guías de solución de problemas"
echo ""
echo "📋 Próximos pasos:"
echo ""
echo "1. Comprimir la carpeta:"
echo "   cd publish_iis && zip -r ../factiox_iis_$(date +%Y%m%d_%H%M%S).zip . && cd .."
echo ""
echo "2. Transferir al servidor Windows"
echo ""
echo "3. Seguir la guía: publish_iis/INSTALAR_IIS.md"
echo ""
echo "4. IMPORTANTE: Configurar appsettings.Production.json con los datos reales"
echo ""
echo "=========================================="
