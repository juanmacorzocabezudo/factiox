# 📱 FactioX - Progressive Web App (PWA)

## ✨ Características PWA Implementadas

FactioX ahora puede instalarse como una aplicación nativa en cualquier dispositivo (Windows, macOS, Linux, Android, iOS).

### 🎯 Funcionalidades

- ✅ **Instalación como aplicación nativa**
- ✅ **Funciona sin conexión** (caché inteligente)
- ✅ **Actualizaciones automáticas**
- ✅ **Accesos directos personalizados**
- ✅ **Notificaciones push** (preparado para futuras funcionalidades)
- ✅ **Sincronización en segundo plano**
- ✅ **Banner de instalación personalizado**

---

## 🚀 Cómo Instalar FactioX

### 📱 En Dispositivos Móviles (Android/iOS)

#### Android (Chrome/Edge):
1. Abre FactioX en Chrome o Edge
2. Aparecerá un banner en la parte inferior derecha: **"Instalar FactioX"**
3. Haz clic en **"Instalar"**
4. La app se instalará en tu dispositivo
5. Accede desde el cajón de aplicaciones

Alternativa:
- Toca el menú (⋮) → **"Instalar aplicación"** o **"Agregar a pantalla de inicio"**

#### iOS (Safari):
1. Abre FactioX en Safari
2. Toca el botón de **Compartir** (📤)
3. Selecciona **"Agregar a pantalla de inicio"**
4. Confirma el nombre y toca **"Agregar"**
5. El icono de FactioX aparecerá en tu pantalla de inicio

---

### 💻 En Computadoras (Windows/macOS/Linux)

#### Windows/Linux (Chrome/Edge):
1. Abre FactioX en Chrome o Edge
2. Busca el icono de instalación (➕) en la barra de direcciones
3. Haz clic en **"Instalar FactioX"**
4. La aplicación se instalará como programa independiente
5. Búscala en el menú de inicio o aplicaciones

Alternativa:
- Menú (⋮) → **"Instalar FactioX"** o **"Instalar aplicación"**

#### macOS (Chrome/Edge/Safari):
1. Abre FactioX en Chrome, Edge o Safari
2. En Chrome/Edge: Icono de instalación en la barra de direcciones
3. En Safari: Archivo → **"Agregar al Dock"**
4. La aplicación aparecerá en el Dock o Launchpad

---

## 🔧 Archivos PWA Implementados

```
wwwroot/
├── manifest.json              # Configuración PWA
├── service-worker.js          # Service Worker para caché y offline
├── js/
│   └── pwa-install.js        # Lógica de instalación personalizada
└── iconos PWA:
    ├── icon-192.png          # Icono 192x192px
    ├── icon-512.png          # Icono 512x512px
    ├── icon-maskable-192.png # Icono adaptable 192x192px
    └── icon-maskable-512.png # Icono adaptable 512x512px
```

---

## ⚙️ Configuración del Manifest

El archivo `manifest.json` define:

- **Nombre**: FactioX - Gestión Empresarial
- **Tema**: Verde oscuro (#1a3a1a)
- **Modo de visualización**: Standalone (pantalla completa)
- **Accesos directos**: Nueva Factura, Clientes, Presupuestos
- **Iconos**: Múltiples tamaños para diferentes dispositivos

---

## 🔄 Service Worker y Caché

El Service Worker implementa:

### Estrategia de Caché:
- **Network First**: Intenta cargar desde la red primero
- **Cache Fallback**: Si no hay conexión, usa la caché
- **Caché estática**: Archivos CSS, JS, imágenes
- **Caché dinámica**: Páginas visitadas

### Archivos Cacheados Automáticamente:
- Página principal (/)
- Hojas de estilo (CSS)
- Scripts de la aplicación
- Iconos y logos
- Framework de Blazor

---

## 🎨 Personalización del Banner de Instalación

El banner de instalación personalizado:

- Aparece después de cargar la página
- Se puede cerrar (no vuelve a aparecer en 24h)
- Diseño responsive (se adapta a móviles)
- Animación de entrada suave
- Botón verde destacado

Para personalizar el banner, edita: `wwwroot/js/pwa-install.js`

---

## 📊 Accesos Directos

La PWA incluye accesos directos para:

1. **Nueva Factura** → `/facturas-venta`
2. **Clientes** → `/clientes`
3. **Presupuestos** → `/presupuestos`

En Android, mantén presionado el icono de la app para ver estos accesos.

---

## 🔔 Notificaciones Push (Preparado)

El Service Worker está configurado para soportar notificaciones push. Para activarlas:

1. Solicitar permiso: `window.pwaInstall.requestNotifications()`
2. Implementar endpoint de push en el backend
3. Enviar notificaciones desde el servidor

---

## 🧪 Probar la PWA en Desarrollo

### Chrome DevTools:
1. Abre DevTools (F12)
2. Ve a la pestaña **"Application"**
3. Verifica:
   - **Manifest**: Debe mostrar la configuración
   - **Service Workers**: Debe estar "activated and running"
   - **Cache Storage**: Verifica los archivos cacheados

### Lighthouse:
1. DevTools → Pestaña **"Lighthouse"**
2. Selecciona **"Progressive Web App"**
3. Haz clic en **"Generate report"**
4. Revisa las puntuaciones y recomendaciones

---

## 🌐 Funcionamiento Offline

Cuando no hay conexión:

1. Las páginas visitadas previamente se cargarán desde caché
2. Los assets estáticos (CSS, JS) estarán disponibles
3. Las imágenes y logos se mostrarán desde caché
4. Las peticiones de red fallarán (se puede implementar cola de sincronización)

---

## 🔄 Actualizaciones Automáticas

El Service Worker:

1. Verifica actualizaciones cada minuto
2. Si hay una nueva versión, pregunta al usuario
3. Al confirmar, recarga la aplicación
4. Los cambios se aplican inmediatamente

---

## 📱 Soporte de Navegadores

| Navegador | Instalación PWA | Service Worker | Offline |
|-----------|----------------|----------------|---------|
| Chrome    | ✅ Completo    | ✅ Completo    | ✅      |
| Edge      | ✅ Completo    | ✅ Completo    | ✅      |
| Firefox   | ⚠️ Parcial     | ✅ Completo    | ✅      |
| Safari    | ✅ iOS/macOS   | ✅ Limitado    | ⚠️      |
| Opera     | ✅ Completo    | ✅ Completo    | ✅      |

---

## 🐛 Solución de Problemas

### La app no se instala:
- Verifica que estés en HTTPS (o localhost)
- Limpia la caché del navegador
- Verifica que `manifest.json` sea accesible
- Revisa la consola de DevTools por errores

### El Service Worker no se registra:
- Verifica que `service-worker.js` esté en la raíz de wwwroot
- Comprueba que no hay errores en la consola
- Asegúrate de que el servidor sirve el archivo correctamente

### Los cambios no se reflejan:
- El Service Worker puede estar cacheando versiones antiguas
- Borra manualmente la caché en DevTools → Application → Cache Storage
- O ejecuta en la consola: `window.location.reload(true)`

### Desinstalar el Service Worker:
```javascript
navigator.serviceWorker.getRegistrations().then(function(registrations) {
    for(let registration of registrations) {
        registration.unregister();
    }
});
```

---

## 🎯 Mejoras Futuras

- [ ] Optimizar iconos para diferentes tamaños (usar el script Python)
- [ ] Implementar sincronización en segundo plano para facturas
- [ ] Agregar notificaciones push para recordatorios
- [ ] Mejorar caché con estrategias más específicas
- [ ] Agregar screenshots para la instalación
- [ ] Implementar modo offline completo con base de datos local

---

## 📚 Recursos Adicionales

- [MDN - Progressive Web Apps](https://developer.mozilla.org/es/docs/Web/Progressive_web_apps)
- [web.dev - PWA](https://web.dev/progressive-web-apps/)
- [Service Worker API](https://developer.mozilla.org/es/docs/Web/API/Service_Worker_API)
- [Web App Manifest](https://developer.mozilla.org/es/docs/Web/Manifest)

---

## 📝 Notas

- Los iconos actuales son copias del logo principal. Para mejor calidad, considera optimizar cada tamaño.
- El Service Worker cachea agresivamente. Incrementa la versión en `service-worker.js` para forzar actualizaciones.
- En producción, asegúrate de servir la aplicación con HTTPS para que la PWA funcione correctamente.

---

✨ **¡FactioX ahora es una aplicación instalable!**
