# 🚀 Configuración PWA para Producción

## Checklist de Despliegue PWA

Antes de desplegar FactioX como PWA en producción, verifica lo siguiente:

### ✅ Requisitos Obligatorios

- [ ] **HTTPS habilitado** - Las PWA solo funcionan con HTTPS (o localhost para desarrollo)
- [ ] **Certificado SSL válido** - Sin errores de certificado
- [ ] **Manifest.json accesible** - Debe estar en la raíz de wwwroot
- [ ] **Service Worker registrado** - Verificar en DevTools
- [ ] **Iconos creados** - Al menos icon-192.png y icon-512.png

### 📝 Configuración del Servidor Web

#### Apache (.htaccess)
```apache
# Habilitar HTTPS
RewriteEngine On
RewriteCond %{HTTPS} off
RewriteRule ^(.*)$ https://%{HTTP_HOST}%{REQUEST_URI} [L,R=301]

# Cache-Control para Service Worker (actualizar frecuentemente)
<Files "service-worker.js">
    Header set Cache-Control "no-cache, no-store, must-revalidate"
    Header set Pragma "no-cache"
    Header set Expires 0
</Files>

# Cache-Control para manifest.json
<Files "manifest.json">
    Header set Cache-Control "public, max-age=86400"
</Files>

# MIME types correctos
AddType application/manifest+json .webmanifest
AddType application/manifest+json .json
AddType application/javascript .js
```

#### Nginx (nginx.conf)
```nginx
server {
    listen 80;
    server_name tu-dominio.com;
    return 301 https://$server_name$request_uri;
}

server {
    listen 443 ssl http2;
    server_name tu-dominio.com;

    ssl_certificate /ruta/al/certificado.crt;
    ssl_certificate_key /ruta/a/la/llave.key;

    # Service Worker - no cachear
    location = /service-worker.js {
        add_header Cache-Control "no-cache, no-store, must-revalidate";
        add_header Pragma "no-cache";
        add_header Expires 0;
        try_files $uri =404;
    }

    # Manifest - cachear 1 día
    location = /manifest.json {
        add_header Cache-Control "public, max-age=86400";
        try_files $uri =404;
    }

    # PWA debe servir index.html para todas las rutas
    location / {
        try_files $uri $uri/ /index.html;
    }
}
```

#### IIS (web.config)
```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <system.webServer>
    <!-- Redirigir HTTP a HTTPS -->
    <rewrite>
      <rules>
        <rule name="HTTPS Redirect" stopProcessing="true">
          <match url="(.*)" />
          <conditions>
            <add input="{HTTPS}" pattern="off" />
          </conditions>
          <action type="Redirect" url="https://{HTTP_HOST}/{R:1}" redirectType="Permanent" />
        </rule>
      </rules>
    </rewrite>

    <!-- Cache-Control headers -->
    <staticContent>
      <clientCache cacheControlMode="UseMaxAge" cacheControlMaxAge="0.00:01:00" />
      <remove fileExtension=".json" />
      <mimeMap fileExtension=".json" mimeType="application/json" />
      <remove fileExtension=".webmanifest" />
      <mimeMap fileExtension=".webmanifest" mimeType="application/manifest+json" />
    </staticContent>

    <outboundRules>
      <rule name="Add Service Worker no-cache header">
        <match serverVariable="RESPONSE_Cache_Control" pattern=".*" />
        <conditions>
          <add input="{REQUEST_URI}" pattern="service-worker\.js$" />
        </conditions>
        <action type="Rewrite" value="no-cache, no-store, must-revalidate" />
      </rule>
    </outboundRules>
  </system.webServer>
</configuration>
```

### 🔧 Configuración de appsettings.json

Añade esta sección para configurar la PWA:

```json
{
  "PWA": {
    "Enabled": true,
    "ServiceWorkerPath": "/service-worker.js",
    "ManifestPath": "/manifest.json",
    "CacheVersion": "v1.0",
    "EnableNotifications": false,
    "EnableBackgroundSync": false
  }
}
```

### 📊 Verificación Post-Despliegue

#### 1. Lighthouse Audit
```bash
# Instalar Lighthouse CLI
npm install -g lighthouse

# Ejecutar audit
lighthouse https://tu-dominio.com --view --preset=desktop
```

Verifica que obtienes al menos:
- PWA: 90+
- Performance: 80+
- Accessibility: 90+
- Best Practices: 90+

#### 2. Chrome DevTools
1. Abre tu aplicación en Chrome
2. DevTools (F12) → Application tab
3. Verifica:
   - ✅ Manifest: Todo en verde
   - ✅ Service Workers: "activated and running"
   - ✅ Cache Storage: Archivos cacheados correctamente
   - ✅ Install button: Aparece en la barra de direcciones

#### 3. PWA Builder
Visita: https://www.pwabuilder.com/
1. Ingresa tu URL
2. Haz clic en "Analyze"
3. Verifica el score y las recomendaciones

### 🌍 Dominio y DNS

Asegúrate de que tu dominio tenga:
```
A     @    TU_IP_SERVIDOR
CNAME www  TU_DOMINIO.com
```

### 📱 Testing en Dispositivos Reales

#### Android
1. Chrome → Menu → "Instalar aplicación"
2. Verificar que aparece en el cajón de aplicaciones
3. Probar funcionamiento offline

#### iOS
1. Safari → Compartir → "Agregar a pantalla de inicio"
2. Verificar icono en la pantalla principal
3. Probar apertura en modo standalone

#### Desktop
1. Chrome/Edge → Botón de instalación en barra de direcciones
2. Verificar que aparece en el menú de inicio/aplicaciones
3. Probar como ventana independiente

### 🔔 Notificaciones Push (Opcional)

Para habilitar notificaciones:

1. Configurar VAPID keys:
```bash
npm install -g web-push
web-push generate-vapid-keys
```

2. Guardar las keys en appsettings.json:
```json
{
  "PushNotifications": {
    "PublicKey": "TU_PUBLIC_KEY",
    "PrivateKey": "TU_PRIVATE_KEY",
    "Subject": "mailto:contacto@tu-dominio.com"
  }
}
```

3. Implementar endpoint de subscripción en el backend

### 🐛 Troubleshooting Común

#### La PWA no se puede instalar
- Verificar HTTPS activo
- Comprobar manifest.json es válido: https://manifest-validator.appspot.com/
- Service Worker debe estar registrado sin errores
- Al menos 2 iconos (192x192 y 512x512)

#### Service Worker no se actualiza
- Incrementar número de versión en `CACHE_NAME`
- Limpiar caché en DevTools
- Hacer hard refresh (Ctrl+Shift+R / Cmd+Shift+R)

#### Iconos no se muestran
- Verificar que las rutas en manifest.json son correctas
- Iconos deben ser PNG o WebP
- Tamaños recomendados: 192x192, 512x512, 72x72, 96x96, 128x128, 144x144, 152x152, 384x384

#### No funciona offline
- Verificar que el Service Worker está activo
- Comprobar estrategia de caché en service-worker.js
- Revisar Cache Storage en DevTools

### 📈 Monitoreo y Analytics

Considera agregar:

```javascript
// En service-worker.js, agregar analytics
self.addEventListener('install', (event) => {
    // Enviar evento de instalación
    fetch('/api/analytics/pwa-install', {
        method: 'POST',
        body: JSON.stringify({ event: 'pwa_install', timestamp: Date.now() })
    });
});
```

### 🔄 Plan de Actualización

1. Modificar archivos de la aplicación
2. Incrementar versión en `CACHE_NAME` en service-worker.js
3. Desplegar nueva versión
4. Service Worker detectará cambios automáticamente
5. Usuarios verán prompt de actualización

### 📦 Checklist Final

- [ ] HTTPS configurado y funcionando
- [ ] Manifest.json accesible y válido
- [ ] Service Worker registrado correctamente
- [ ] Iconos creados y optimizados
- [ ] Cache-Control headers configurados
- [ ] Pruebas en dispositivos reales (Android, iOS, Desktop)
- [ ] Lighthouse score > 90 en PWA
- [ ] Modo offline funciona correctamente
- [ ] Banner de instalación aparece correctamente
- [ ] Documentación actualizada para usuarios

---

## 🎯 Optimizaciones Recomendadas

### Iconos
Ejecuta el script de Python para generar iconos optimizados:
```bash
python3 generar_iconos_pwa.py
```

### Service Worker
Considera implementar:
- Estrategias de caché más específicas (Cache First para assets estáticos)
- Pre-caché de rutas principales
- Background Sync para facturas sin conexión
- Periodic Background Sync para sincronización automática

### Manifest
Añade:
- Screenshots de la aplicación
- Más shortcuts personalizados
- Related applications (si tienes app nativa)

---

✅ **¡Tu PWA está lista para producción!**
