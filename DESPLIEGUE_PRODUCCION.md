# Guía de Despliegue en Producción - FactioX

**Fecha de publicación:** 10 de marzo de 2026

## 📦 Archivos Publicados

La aplicación se ha publicado correctamente en la carpeta `./publish/` con la configuración de Release optimizada para producción.

## 🔧 Pasos para Desplegar

### 1. Preparar el Servidor de Producción

Asegúrate de que el servidor tenga instalado:
- **.NET 8.0 Runtime** (no se necesita el SDK completo)
- **MySQL Server** (compatible con el servidor de producción)
- **Acceso a puertos** necesarios (por defecto 5000 y 5001)

### 2. Copiar Archivos al Servidor

Transfiere todo el contenido de la carpeta `./publish/` al servidor de producción:

```bash
# Desde tu equipo local (MacOS)
scp -r ./publish/* usuario@servidor:/ruta/destino/factiox/
```

O usa FTP/SFTP según tu configuración.

### 3. Configurar Base de Datos de Producción

⚠️ **IMPORTANTE:** Antes de iniciar la aplicación, actualiza `appsettings.Production.json` con los datos del servidor:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=TU_SERVIDOR_MYSQL;Database=FactioX;User=TU_USUARIO;Password=TU_PASSWORD;Port=3306;"
  },
  "Email": {
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": "587",
    "SmtpUser": "email-produccion@tudominio.com",
    "SmtpPassword": "contraseña-app-gmail",
    "FromEmail": "email-produccion@tudominio.com",
    "FromName": "FactioX",
    "EnableSsl": "true"
  },
  "Ollama": {
    "Url": "http://127.0.0.1:11434",
    "Model": "llama3.2:3b"
  }
}
```

### 4. Aplicar Migraciones de Base de Datos

Si la base de datos de producción no tiene todas las migraciones aplicadas:

**Opción A - Usando dotnet ef (requiere SDK):**
```bash
cd /ruta/destino/factiox
dotnet ef database update
```

**Opción B - Importar desde backup:**
```bash
mysql -u usuario -p FactioX < factiox_backup_produccion_20260304_133112.sql
```

### 5. Configurar Variables de Entorno

En el servidor, establece el entorno de producción:

```bash
export ASPNETCORE_ENVIRONMENT=Production
```

O en Windows:
```cmd
set ASPNETCORE_ENVIRONMENT=Production
```

### 6. Ejecutar la Aplicación

#### Ejecución Manual (para pruebas):
```bash
cd /ruta/destino/factiox
./FactioX --urls="http://0.0.0.0:5000"
```

#### Ejecución como Servicio (Linux - systemd):

Crea el archivo `/etc/systemd/system/factiox.service`:

```ini
[Unit]
Description=FactioX Application
After=network.target

[Service]
WorkingDirectory=/ruta/destino/factiox
ExecStart=/ruta/destino/factiox/FactioX --urls="http://0.0.0.0:5000"
Restart=always
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=factiox
User=www-data
Environment=ASPNETCORE_ENVIRONMENT=Production

[Install]
WantedBy=multi-user.target
```

Luego:
```bash
sudo systemctl daemon-reload
sudo systemctl enable factiox
sudo systemctl start factiox
sudo systemctl status factiox
```

### 7. Configurar Nginx como Proxy Reverso (Recomendado)

Crea `/etc/nginx/sites-available/factiox`:

```nginx
server {
    listen 80;
    server_name tudominio.com www.tudominio.com;

    location / {
        proxy_pass http://localhost:5000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
```

Activa el sitio:
```bash
sudo ln -s /etc/nginx/sites-available/factiox /etc/nginx/sites-enabled/
sudo nginx -t
sudo systemctl restart nginx
```

### 8. Configurar HTTPS con Let's Encrypt (Opcional pero recomendado)

```bash
sudo apt install certbot python3-certbot-nginx
sudo certbot --nginx -d tudominio.com -d www.tudominio.com
```

## ✅ Verificación Post-Despliegue

1. **Verificar conectividad a base de datos:**
   - Intenta acceder al login
   - Verifica que aparezcan las empresas configuradas

2. **Probar funcionalidades críticas:**
   - Login de usuarios
   - Creación de facturas
   - Envío de emails
   - Generación de PDFs

3. **Revisar logs:**
```bash
sudo journalctl -u factiox -f
```

## 🔒 Seguridad en Producción

- ✅ Cambia las contraseñas por defecto en la base de datos
- ✅ Usa contraseñas fuertes para todos los usuarios
- ✅ Configura el firewall para solo permitir puertos necesarios
- ✅ Mantén actualizado el sistema operativo y .NET Runtime
- ✅ Configura backups automáticos de la base de datos
- ✅ Usa HTTPS en producción (certificado SSL)

## 📊 Monitoreo

Revisa regularmente:
- Logs de la aplicación
- Uso de CPU y memoria
- Espacio en disco
- Rendimiento de la base de datos

## 🆘 Solución de Problemas

### La aplicación no inicia
```bash
# Verifica logs
sudo journalctl -u factiox -n 50

# Verifica puerto en uso
sudo lsof -i :5000
```

### Error de conexión a base de datos
- Verifica las credenciales en `appsettings.Production.json`
- Asegúrate de que MySQL esté ejecutándose
- Verifica que el firewall permita conexiones al puerto 3306

### Error 500 en producción
- Revisa los logs detallados
- Verifica que todas las migraciones estén aplicadas
- Comprueba los permisos de archivos y carpetas

## 📝 Notas Importantes

1. **Backups:** Realiza backup de la base de datos antes de cualquier despliegue
2. **Variables de Entorno:** Asegúrate de que `ASPNETCORE_ENVIRONMENT=Production`
3. **Permisos:** El usuario que ejecuta la aplicación debe tener permisos de lectura en los archivos
4. **Certificados:** Si usas certificados electrónicos, copia el archivo .pfx al servidor

## 📞 Contacto y Soporte

Si encuentras problemas durante el despliegue, revisa:
- Logs de la aplicación
- Documentación de .NET: https://docs.microsoft.com/aspnet/core/
- Documentación de MySQL

---

**Última actualización:** 10 de marzo de 2026
**Versión publicada:** Release Build
**Framework:** .NET 8.0
