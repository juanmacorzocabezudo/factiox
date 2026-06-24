# 📦 Publicación para Producción - FactioX

**Fecha de publicación:** 10 de marzo de 2026  
**Versión:** Release Build  
**Tamaño:** ~95 MB  
**Archivos:** 44 archivos

---

## ✅ Estado de la Publicación

**La aplicación está lista para subir a producción** con los siguientes componentes:

- ✅ Aplicación compilada en modo Release
- ✅ Todas las dependencias incluidas
- ✅ Archivos estáticos optimizados
- ✅ Configuración de producción preparada
- ✅ Documentación de despliegue completa
- ✅ Scripts de ayuda incluidos

---

## 📁 Archivos Generados

### 1. Carpeta `./publish/`
Contiene la aplicación compilada lista para ejecutar:
- `FactioX` (ejecutable)
- `FactioX.dll` (biblioteca principal)
- Todas las DLL de dependencias
- Carpeta `wwwroot/` con archivos estáticos
- Archivos de configuración

### 2. Documentación de Despliegue

| Archivo | Descripción |
|---------|-------------|
| **DESPLIEGUE_PRODUCCION.md** | Guía completa paso a paso para desplegar |
| **CHECKLIST_PRODUCCION.md** | Lista de verificación para el despliegue |
| **empaquetar_produccion.sh** | Script para crear paquete ZIP con todo |

---

## 🚀 Opciones de Despliegue

### Opción 1: Despliegue Manual Rápido

1. **Copiar la carpeta `publish/` al servidor:**
   ```bash
   scp -r ./publish usuario@servidor:/opt/factiox/
   ```

2. **Configurar en el servidor:**
   ```bash
   export ASPNETCORE_ENVIRONMENT=Production
   cd /opt/factiox
   ./FactioX --urls="http://0.0.0.0:5000"
   ```

3. **Configurar la base de datos:**
   - Crear base de datos MySQL
   - Aplicar migraciones o importar backup
   - Actualizar `appsettings.Production.json`

### Opción 2: Usar Script de Empaquetado (Recomendado)

1. **Ejecutar el script:**
   ```bash
   ./empaquetar_produccion.sh
   ```

2. **El script creará:**
   - Archivo ZIP con todo lo necesario
   - Backup de la base de datos (opcional)
   - Listo para transferir al servidor

3. **En el servidor:**
   ```bash
   unzip FactioX_Produccion_*.zip
   # Seguir instrucciones en DESPLIEGUE_PRODUCCION.md
   ```

---

## ⚙️ Configuración Requerida

### Antes de Subir a Producción

**🔴 CRÍTICO:** Actualiza `appsettings.Production.json` con:

1. **Conexión a Base de Datos:**
   ```json
   "Server=TU_SERVIDOR;Database=FactioX;User=TU_USUARIO;Password=TU_PASSWORD"
   ```

2. **Configuración de Email:**
   ```json
   "SmtpUser": "email-real@tudominio.com",
   "SmtpPassword": "contraseña-app-real"
   ```

3. **URL de Ollama (si aplica):**
   ```json
   "Url": "http://servidor-ollama:11434"
   ```

---

## 📋 Requisitos del Servidor

### Software Necesario
- ✅ Sistema operativo: Linux (Ubuntu/Debian recomendado) o Windows Server
- ✅ .NET 8.0 Runtime (no se necesita el SDK)
- ✅ MySQL Server 8.0 o superior
- ✅ Nginx o Apache (opcional para proxy reverso)

### Recursos Recomendados
- **CPU:** 2 cores mínimo
- **RAM:** 2 GB mínimo (4 GB recomendado)
- **Disco:** 500 MB para la aplicación + espacio para BD
- **Puertos:** 5000 (aplicación), 3306 (MySQL)

---

## 📚 Pasos Rápidos de Despliegue

### 1. Preparación (Local - Ya completado ✅)
```bash
dotnet publish -c Release -o ./publish  # ✅ HECHO
chmod +x empaquetar_produccion.sh       # ✅ HECHO
```

### 2. Empaquetar (Local)
```bash
./empaquetar_produccion.sh              # Crear ZIP
```

### 3. Transferir al Servidor
```bash
scp FactioX_Produccion_*.zip usuario@servidor:/tmp/
```

### 4. Desplegar en Servidor
```bash
# Conectar al servidor
ssh usuario@servidor

# Descomprimir
cd /opt
sudo mkdir factiox
cd factiox
sudo unzip /tmp/FactioX_Produccion_*.zip

# Configurar
sudo nano appsettings.Production.json  # Actualizar credenciales

# Configurar base de datos
mysql -u root -p
CREATE DATABASE FactioX;
exit;
mysql -u root -p FactioX < factiox_backup_*.sql

# Configurar como servicio
sudo nano /etc/systemd/system/factiox.service
# (Ver DESPLIEGUE_PRODUCCION.md para contenido)

sudo systemctl enable factiox
sudo systemctl start factiox
sudo systemctl status factiox
```

### 5. Verificar
```bash
curl http://localhost:5000
# Debería devolver la página de login
```

---

## 🔐 Seguridad

### Antes de Hacer Público

- [ ] ✅ Cambiar contraseñas de usuarios de prueba
- [ ] ✅ Configurar firewall
- [ ] ✅ Habilitar HTTPS con certificado SSL
- [ ] ✅ Cerrar puertos innecesarios
- [ ] ✅ Configurar backups automáticos
- [ ] ✅ Revisar permisos de archivos

---

## 📊 Verificación Post-Despliegue

Después de desplegar, verifica:

1. ✅ **Aplicación accesible:** http://tudominio.com
2. ✅ **Login funciona:** Puedes iniciar sesión
3. ✅ **Dashboard carga:** Ves el inicio correctamente
4. ✅ **Base de datos conectada:** Aparecen tus datos
5. ✅ **PDFs se generan:** Crear factura y descargar PDF
6. ✅ **Emails se envían:** Probar envío de factura por email
7. ✅ **HTTPS activo:** Candado verde en navegador

---

## 🆘 Solución de Problemas

### Error: "Failed to connect to database"
➡️ Verifica `appsettings.Production.json` tiene credenciales correctas

### Error: "Port 5000 already in use"
➡️ Mata proceso: `sudo lsof -ti:5000 | xargs kill -9`

### La aplicación no inicia
➡️ Revisa logs: `sudo journalctl -u factiox -f`

### Error 500 en producción
➡️ Verifica que `ASPNETCORE_ENVIRONMENT=Production` esté configurado

---

## 📞 Documentación Adicional

Para más detalles, consulta:

- **[DESPLIEGUE_PRODUCCION.md](DESPLIEGUE_PRODUCCION.md)** - Guía detallada paso a paso
- **[CHECKLIST_PRODUCCION.md](CHECKLIST_PRODUCCION.md)** - Lista de verificación completa
- **[Database/FactioX_MySQL_Schema.sql](Database/FactioX_MySQL_Schema.sql)** - Esquema de BD
- **Logs del servidor:** `sudo journalctl -u factiox -f`

---

## 📝 Comandos Útiles

```bash
# Ver estado del servicio
sudo systemctl status factiox

# Reiniciar aplicación
sudo systemctl restart factiox

# Ver logs en tiempo real
sudo journalctl -u factiox -f

# Backup de base de datos
mysqldump -u root -p FactioX > backup_$(date +%Y%m%d).sql

# Verificar puerto en uso
sudo lsof -i :5000

# Espacio en disco
df -h

# Uso de memoria
free -h
```

---

## ✅ Resumen Ejecutivo

**La aplicación FactioX está lista para producción:**

1. ✅ **Compilada** en modo Release optimizado
2. ✅ **Publicada** en `./publish/` (95 MB)
3. ✅ **Documentada** con guías de despliegue completas
4. ✅ **Lista para empaquetar** con script automatizado
5. ✅ **Preparada para desplegar** en servidor Linux/Windows

**Siguiente acción recomendada:**
```bash
./empaquetar_produccion.sh
```

---

**Fecha:** 10 de marzo de 2026  
**Versión .NET:** 8.0  
**Estado:** ✅ LISTA PARA PRODUCCIÓN
