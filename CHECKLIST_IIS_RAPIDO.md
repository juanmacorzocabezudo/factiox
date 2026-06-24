# ✅ Checklist Rápido - Despliegue IIS

## 📋 Antes de Subir al Servidor

- [ ] Archivo ZIP generado: `factiox_iis_YYYYMMDD_HHMMSS.zip` (**61 MB**)
- [ ] Backup de la base de datos incluido en el ZIP
- [ ] Documentación de instalación revisada

## 🔧 En el Servidor Windows (IIS)

### 1. Requisitos del Servidor ✅

- [ ] Windows Server con IIS instalado
- [ ] **.NET 8.0 Hosting Bundle** instalado
  - Descargar: https://dotnet.microsoft.com/download/dotnet/8.0
  - Verificar: `dotnet --list-runtimes` (debe aparecer AspNetCore.App 8.0.x)
- [ ] **MySQL Server** instalado y funcionando
- [ ] **URL Rewrite Module** para IIS (opcional para PWA)

### 2. Preparación (5 min) 📁

```powershell
# 1. Crear carpeta
mkdir C:\inetpub\wwwroot\FactioX

# 2. Extraer el ZIP
# Descomprimir factiox_iis_XXXXXX.zip en C:\inetpub\wwwroot\FactioX

# 3. Verificar que existe web.config
dir C:\inetpub\wwwroot\FactioX\web.config
```

### 3. Configurar Base de Datos (10 min) 🗄️

```powershell
# Importar BD (ajusta la ruta del backup)
cd C:\inetpub\wwwroot\FactioX
mysql -u root -p FactioX < factiox_backup_PRODUCCION.sql

# O si prefieres usar MySQL Workbench:
# - Abre MySQL Workbench
# - Server → Data Import
# - Selecciona el archivo .sql
# - Import to Self-Contained File
```

### 4. Editar appsettings.Production.json (3 min) ⚙️

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=FactioX;User=root;Password=TU_PASSWORD;Port=3306;"
  },
  "Email": {
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": "587",
    "SmtpUser": "tu-email@gmail.com",
    "SmtpPassword": "tu-password-app",
    "FromEmail": "tu-email@gmail.com",
    "FromName": "FactioX",
    "EnableSsl": "true"
  }
}
```

**⚠️ IMPORTANTE:** Reemplaza los valores de ejemplo con los reales.

### 5. Configurar Permisos (2 min) 🔒

```powershell
cd C:\inetpub\wwwroot\FactioX

# Dar permisos al Application Pool (usa el nombre que vayas a crear)
icacls . /grant "IIS AppPool\FactioX":(OI)(CI)M
icacls wwwroot /grant "IIS AppPool\FactioX":(OI)(CI)M
icacls logs /grant "IIS AppPool\FactioX":(OI)(CI)M
icacls tessdata /grant "IIS AppPool\FactioX":(OI)(CI)R
```

### 6. Crear Application Pool en IIS (3 min) 🏊

1. Abre **IIS Manager** (Windows + R → `inetmgr`)
2. Expande el servidor → **Application Pools**
3. Click derecho → **Add Application Pool**
   - Name: `FactioX`
   - .NET CLR version: **No Managed Code**
   - Managed pipeline mode: **Integrated**
   - ✓ Start application pool immediately
4. Click **OK**
5. Click derecho en **FactioX** → **Advanced Settings**:
   - **Start Mode:** AlwaysRunning
   - **Idle Time-out:** 0
   - Click **OK**

### 7. Crear Sitio Web en IIS (5 min) 🌐

1. Click derecho en **Sites** → **Add Website**
2. Configuración:
   - **Site name:** FactioX
   - **Application pool:** FactioX
   - **Physical path:** C:\inetpub\wwwroot\FactioX
   - **Binding:**
     - Type: http
     - Port: 80 (o el que uses)
     - Host name: (vacío o tu dominio)
3. Click **OK**

### 8. Reiniciar IIS (1 min) 🔄

```powershell
iisreset /restart
```

### 9. Verificar que Funciona (2 min) ✅

```powershell
# Opción 1: Navegador
# Abre: http://localhost

# Opción 2: PowerShell
Invoke-WebRequest -Uri http://localhost -UseBasicParsing
```

**Deberías ver la página de login de FactioX** 🎉

## 🐛 Si algo sale mal...

### Error 500.19
```powershell
# Reinstalar .NET 8.0 Hosting Bundle
# Descargar de: https://dotnet.microsoft.com/download/dotnet/8.0
```

### Error 502.5
```powershell
# Ver logs
type C:\inetpub\wwwroot\FactioX\logs\stdout_*.log

# Revisar conexión a MySQL
# Verifica appsettings.Production.json
```

### No se ve nada / Error genérico
```powershell
# Activar logs detallados en web.config
# Cambiar stdoutLogEnabled="true"
# Revisar: C:\inetpub\wwwroot\FactioX\logs\
```

## 🔒 Configurar HTTPS (Opcional pero recomendado)

1. En IIS Manager → Tu sitio → **Bindings**
2. Click **Add**:
   - Type: **https**
   - Port: **443**
   - SSL certificate: Selecciona tu certificado
3. Click **OK**

## 📊 Verificación Final

- [ ] La aplicación carga en el navegador
- [ ] Puedes hacer login con un usuario de prueba
- [ ] Las imágenes y estilos se cargan correctamente
- [ ] La conexión a MySQL funciona (puedes ver clientes, facturas, etc.)
- [ ] Los logs no muestran errores críticos

## 📞 Documentación Adicional

Si necesitas más información, revisa estos archivos en la carpeta de publicación:

- **INSTALAR_IIS.md** - Guía detallada de instalación
- **SOLUCIONAR_ERROR_500_IIS.md** - Solución de problemas comunes
- **DIAGNOSTICO_ERROR_500.md** - Diagnóstico avanzado

---

## ⏱️ Tiempo Total Estimado: **30 minutos**

(Más tiempo si hay que instalar requisitos o solucionar problemas)

---

**Última actualización:** 11 de junio de 2026
