# 🧪 Guía de Pruebas - PWA FactioX

## 🚀 Probar la PWA en Desarrollo Local

### 1. Iniciar la Aplicación

```bash
cd /Users/juanmariacorzo/Documents/FactioX
dotnet run
```

La aplicación debería estar disponible en: `http://localhost:5007`

### 2. Abrir Chrome DevTools

1. Abre Chrome o Edge
2. Navega a `http://localhost:5007`
3. Presiona **F12** para abrir DevTools
4. Ve a la pestaña **"Application"**

### 3. Verificar Manifest

En la pestaña "Application":
- Haz clic en **"Manifest"** en el menú lateral
- Deberías ver:
  - ✅ Name: "FactioX - Gestión Empresarial"
  - ✅ Short name: "FactioX"
  - ✅ Theme color: #1a3a1a
  - ✅ Display: standalone
  - ✅ Iconos: 5 iconos listados
  - ✅ Shortcuts: 3 accesos directos

### 4. Verificar Service Worker

En la pestaña "Application":
- Haz clic en **"Service Workers"** en el menú lateral
- Deberías ver:
  - ✅ Source: service-worker.js
  - ✅ Status: **"activated and running"** (círculo verde)
  - ✅ Scope: http://localhost:5007/

**Acciones disponibles:**
- **Update**: Fuerza actualización del SW
- **Unregister**: Desregistra el SW (para testing)
- **Bypass for network**: Desactiva el SW temporalmente

### 5. Verificar Cache Storage

En la pestaña "Application":
- Haz clic en **"Cache Storage"** en el menú lateral
- Deberías ver dos cachés:
  - `factiox-static-v1.0` (archivos estáticos)
  - `factiox-dynamic-v1.0` (páginas visitadas)

Expande cada caché para ver los archivos almacenados.

### 6. Banner de Instalación

Después de cargar la página:
- Debería aparecer un **banner verde** en la esquina inferior derecha
- Texto: "Instalar FactioX"
- Botón: "Instalar"

**Si no aparece el banner:**
- El navegador podría haber bloqueado el evento `beforeinstallprompt`
- Busca el icono de instalación (➕) en la barra de direcciones de Chrome

### 7. Instalar la Aplicación

#### Opción A: Usar el Banner
1. Haz clic en el botón **"Instalar"** del banner
2. Confirma la instalación en el diálogo del navegador
3. La aplicación se instalará

#### Opción B: Usar el Icono del Navegador
1. Busca el icono ➕ en la barra de direcciones
2. Haz clic en **"Instalar FactioX"**
3. Confirma la instalación

#### Opción C: Menú del Navegador
1. Chrome: Menú (⋮) → **"Instalar FactioX"**
2. Edge: Menú (···) → **"Aplicaciones"** → **"Instalar FactioX"**

### 8. Verificar Instalación Exitosa

Después de instalar:
- ✅ Se abre una ventana nueva con la aplicación
- ✅ No hay barra de direcciones (modo standalone)
- ✅ El icono aparece en:
  - **Windows**: Menú Inicio
  - **macOS**: Launchpad / Dock
  - **Linux**: Menú de aplicaciones

### 9. Probar Funcionamiento Offline

1. **Con la app instalada abierta**, ve a DevTools (F12)
2. Pestaña **"Network"**
3. Marca la casilla **"Offline"** (simula sin conexión)
4. Recarga la página (F5 o Cmd+R)
5. ✅ La página debería cargar desde caché
6. Navega a páginas que hayas visitado antes
7. ✅ Deberían funcionar sin conexión

**Nota:** Las páginas nuevas (no visitadas) no funcionarán sin conexión.

### 10. Probar Actualización del Service Worker

Para simular una actualización:

1. Abre `wwwroot/service-worker.js`
2. Cambia la versión del caché:
   ```javascript
   // De:
   const CACHE_NAME = 'factiox-v1.0';
   // A:
   const CACHE_NAME = 'factiox-v1.1';
   ```
3. Guarda el archivo
4. Recarga la aplicación
5. Debería aparecer un diálogo: **"Nueva versión disponible. ¿Desea actualizar?"**
6. Haz clic en "Aceptar"
7. ✅ La aplicación se recarga con la nueva versión

### 11. Lighthouse Audit

Para obtener un score de PWA:

1. DevTools → Pestaña **"Lighthouse"**
2. Selecciona:
   - ✅ Progressive Web App
   - ✅ Performance
   - ✅ Accessibility
   - ✅ Best Practices
3. Haz clic en **"Analyze page load"**
4. Espera los resultados

**Scores esperados:**
- PWA: 90-100 ✅
- Performance: 70-90 ⚠️ (puede variar en local)
- Accessibility: 85-95
- Best Practices: 90-100

### 12. Probar en Móvil (Opcional)

#### Android via USB Debugging:

1. **Habilitar depuración USB en Android:**
   - Ajustes → Sistema → Opciones de desarrollador → Depuración USB

2. **Conectar dispositivo por USB**

3. **En Chrome Desktop:**
   - Navega a `chrome://inspect#devices`
   - Deberías ver tu dispositivo
   - Haz clic en **"Port forwarding"**
   - Agrega: `5007` → `localhost:5007`

4. **En el móvil:**
   - Abre Chrome
   - Navega a `localhost:5007`
   - Instala la PWA desde el banner

#### iOS via BrowserStack / Simulador:

Si tienes macOS con Xcode:
```bash
open -a Simulator
```
En Safari del simulador, navega a tu IP local:
```
http://192.168.1.X:5007
```

### 13. Verificar Accesos Directos

**En Android (después de instalar):**
1. Mantén presionado el icono de FactioX
2. Deberías ver 3 accesos directos:
   - Nueva Factura
   - Clientes
   - Presupuestos

**En Windows:**
1. Haz clic derecho en el icono de FactioX en el Menú Inicio
2. Haz clic en "Pin to taskbar"
3. Haz clic derecho en el icono de la barra de tareas
4. Deberías ver los accesos directos en la jump list

---

## 🧪 Tests Funcionales

### Test 1: Instalación y Desinstalación
- [ ] La app se instala correctamente
- [ ] La app aparece en el sistema operativo
- [ ] La app se abre en modo standalone
- [ ] La app se puede desinstalar correctamente

### Test 2: Service Worker
- [ ] El SW se registra sin errores
- [ ] Los archivos se cachean correctamente
- [ ] La app funciona offline (páginas visitadas)
- [ ] Las actualizaciones se detectan automáticamente

### Test 3: Manifest
- [ ] El manifest es válido (sin errores en DevTools)
- [ ] Los iconos se muestran correctamente
- [ ] El color del tema se aplica
- [ ] Los shortcuts funcionan

### Test 4: Cache Strategy
- [ ] Los assets estáticos se cachean
- [ ] Las páginas dinámicas se cachean después de visitarlas
- [ ] Las peticiones de red funcionan cuando hay conexión
- [ ] El fallback a caché funciona sin conexión

### Test 5: User Experience
- [ ] El banner de instalación aparece
- [ ] El banner se puede cerrar
- [ ] El banner no vuelve a aparecer en 24h después de cerrarse
- [ ] La instalación es fluida y rápida

### Test 6: Updates
- [ ] Al cambiar la versión del SW, se detecta
- [ ] El prompt de actualización aparece
- [ ] La actualización se aplica correctamente
- [ ] Los archivos antiguos se eliminan de caché

---

## 🐛 Errores Comunes y Soluciones

### Error: "Failed to register service worker"
**Causa:** El archivo service-worker.js no se encuentra
**Solución:**
```bash
# Verificar que existe
ls wwwroot/service-worker.js
# Verificar permisos
chmod 644 wwwroot/service-worker.js
```

### Error: "Manifest is not valid JSON"
**Causa:** Error de sintaxis en manifest.json
**Solución:**
```bash
# Validar JSON
cat wwwroot/manifest.json | python3 -m json.tool
```

### Warning: "Page does not work offline"
**Causa:** Normal en desarrollo, el SW necesita tiempo para cachear
**Solución:**
- Visita las páginas principales
- Recarga la aplicación
- El SW cacheará los recursos progresivamente

### La app no se puede instalar
**Causas posibles:**
- No hay HTTPS (ok en localhost)
- Faltan iconos requeridos
- El manifest tiene errores
- El navegador no soporta PWA

**Solución:**
- Verifica en DevTools → Application → Manifest
- Revisa errores en la consola
- Verifica que service-worker.js está registrado

---

## 📊 Métricas a Monitorear

Cuando la PWA esté en producción, monitorea:

1. **Tasa de Instalación:** % de usuarios que instalan
2. **Tasa de Retención:** % de usuarios que mantienen instalada
3. **Uso Offline:** % de tiempo que se usa sin conexión
4. **Performance:** Tiempo de carga, FCP, LCP
5. **Errores:** Fallos del Service Worker

---

## ✅ Checklist de Testing Completo

- [ ] Manifest.json accesible y válido
- [ ] Service Worker registrado correctamente
- [ ] Cache Storage contiene archivos
- [ ] Banner de instalación aparece
- [ ] Instalación funciona (Desktop)
- [ ] Instalación funciona (Android)
- [ ] Instalación funciona (iOS)
- [ ] App funciona offline
- [ ] Actualizaciones se detectan
- [ ] Accesos directos funcionan
- [ ] Lighthouse score > 90 en PWA
- [ ] No hay errores en consola
- [ ] Iconos se muestran correctamente
- [ ] Tema de color se aplica
- [ ] Desinstalación funciona

---

¡Felicidades! ✨ Si todos los tests pasan, tu PWA está lista.
