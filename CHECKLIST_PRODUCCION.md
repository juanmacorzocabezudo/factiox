# ✅ Checklist de Despliegue a Producción - FactioX

**Fecha:** 10 de marzo de 2026

## Pre-Despliegue

- [x] ✅ Aplicación compilada sin errores
- [x] ✅ Migraciones de base de datos aplicadas en desarrollo
- [x] ✅ Archivos publicados en `./publish/`
- [ ] ⚠️  Configurar `appsettings.Production.json` con credenciales reales
- [ ] ⚠️  Backup de base de datos de producción (si existe)
- [ ] ⚠️  Crear base de datos en servidor de producción
- [ ] ⚠️  Aplicar migraciones en servidor de producción

## Archivos a Verificar Antes de Subir

### 1. appsettings.Production.json
```bash
# Debe contener:
- Cadena de conexión a MySQL de producción
- Credenciales de email reales (no de prueba)
- URL de Ollama si está en servidor diferente
```

### 2. Archivos Incluidos en `./publish/`
- [x] FactioX.dll (archivo principal)
- [x] FactioX (ejecutable)
- [x] FactioX.deps.json
- [x] FactioX.runtimeconfig.json
- [x] Todas las DLL de dependencias
- [x] Carpeta wwwroot/
- [x] Archivos de configuración

## Durante el Despliegue

### Base de Datos
- [ ] ⚠️  MySQL Server instalado y ejecutándose
- [ ] ⚠️  Base de datos FactioX creada
- [ ] ⚠️  Usuario de MySQL con permisos correctos
- [ ] ⚠️  Importar backup O aplicar migraciones
- [ ] ⚠️  Verificar que exista al menos un usuario SuperAdministrador

### Servidor
- [ ] ⚠️  .NET 8.0 Runtime instalado
- [ ] ⚠️  Archivos copiados a `/opt/factiox/` o similar
- [ ] ⚠️  Variable de entorno `ASPNETCORE_ENVIRONMENT=Production`
- [ ] ⚠️  Permisos de archivos correctos
- [ ] ⚠️  Puerto 5000 disponible (o configurar otro)

### Configuración de Servicio
- [ ] ⚠️  Crear archivo systemd service (Linux)
- [ ] ⚠️  Habilitar servicio para inicio automático
- [ ] ⚠️  Iniciar servicio
- [ ] ⚠️  Verificar que el servicio está corriendo

### Nginx/Apache (si aplica)
- [ ] ⚠️  Configurar proxy reverso
- [ ] ⚠️  Configurar dominio
- [ ] ⚠️  Reiniciar servidor web
- [ ] ⚠️  Certificado SSL configurado

## Post-Despliegue

### Pruebas Funcionales
- [ ] ⚠️  Acceso a la URL de producción funciona
- [ ] ⚠️  Login con usuario existente funciona
- [ ] ⚠️  Creación de factura funciona
- [ ] ⚠️  Generación de PDF funciona
- [ ] ⚠️  Envío de email funciona
- [ ] ⚠️  Certificado electrónico funciona (si aplica)
- [ ] ⚠️  Chatbot responde (si Ollama está configurado)

### Seguridad
- [ ] ⚠️  Contraseñas de usuarios de prueba cambiadas
- [ ] ⚠️  Usuario SuperAdministrador con contraseña segura
- [ ] ⚠️  Firewall configurado
- [ ] ⚠️  Solo puertos necesarios abiertos (80, 443, 3306 si es necesario)
- [ ] ⚠️  HTTPS habilitado
- [ ] ⚠️  Certificado SSL válido

### Configuración de Backups
- [ ] ⚠️  Script de backup de MySQL configurado
- [ ] ⚠️  Cron job para backups automáticos
- [ ] ⚠️  Verificar que los backups se están creando
- [ ] ⚠️  Probar restauración desde backup

### Monitoreo
- [ ] ⚠️  Logs de aplicación accesibles
- [ ] ⚠️  Comando para ver logs: `sudo journalctl -u factiox -f`
- [ ] ⚠️  Monitoreo de recursos (CPU, RAM, disco)
- [ ] ⚠️  Alertas configuradas (opcional)

## Comandos Útiles de Producción

```bash
# Ver estado del servicio
sudo systemctl status factiox

# Ver logs en tiempo real
sudo journalctl -u factiox -f

# Reiniciar servicio
sudo systemctl restart factiox

# Ver puertos en uso
sudo netstat -tulpn | grep :5000

# Backup de base de datos
mysqldump -u usuario -p FactioX > backup_$(date +%Y%m%d_%H%M%S).sql

# Verificar uso de recursos
htop
df -h
```

## URLs de Verificación

Después del despliegue, verifica estas URLs:

- [ ] ⚠️  `http://tudominio.com` → Debe redirigir a HTTPS
- [ ] ⚠️  `https://tudominio.com` → Página de login
- [ ] ⚠️  `https://tudominio.com/login` → Login funcional
- [ ] ⚠️  `https://tudominio.com/home` → Dashboard (requiere login)

## Contactos de Emergencia

**Problemas con:**
- Servidor: Contactar a proveedor de hosting
- Base de datos: Verificar logs de MySQL
- SSL: Renovar con certbot
- Aplicación: Revisar logs con journalctl

## Rollback (En caso de problemas)

1. Detener servicio: `sudo systemctl stop factiox`
2. Restaurar archivos de versión anterior
3. Restaurar backup de base de datos si es necesario
4. Iniciar servicio: `sudo systemctl start factiox`

---

## ✅ Checklist Rápido de 5 Minutos

Una vez desplegado, verifica rápidamente:

1. ✅ La aplicación responde en el navegador
2. ✅ Login funciona
3. ✅ Puedes ver el dashboard
4. ✅ HTTPS está activo (candado verde)
5. ✅ Logs no muestran errores críticos

Si todo está ✅, ¡El despliegue fue exitoso!

---

**Última actualización:** 10 de marzo de 2026
