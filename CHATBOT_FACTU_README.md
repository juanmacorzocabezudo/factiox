# Chatbot Factu - Configuración de IA Local

## ¿Qué es Factu?

**Factu** es tu asistente virtual de inteligencia artificial integrado en FactioX. Usa modelos de IA locales (sin necesidad de conexión a servicios externos) para ayudarte con:

- Consultas sobre facturas, clientes y productos
- Explicaciones de funcionalidades de la aplicación
- Consejos sobre facturación y gestión empresarial
- Respuestas rápidas a preguntas generales

## Instalación de Ollama

Factu funciona con **Ollama**, un motor de IA local y gratuito.

### En macOS:

```bash
# Descargar e instalar Ollama
brew install ollama

# O descarga desde https://ollama.ai
```

### En Linux:

```bash
curl -fsSL https://ollama.ai/install.sh | sh
```

### En Windows:

Descarga el instalador desde: https://ollama.ai/download

## Configuración

### 1. Iniciar Ollama

```bash
# Iniciar el servicio Ollama
ollama serve
```

Esto iniciará Ollama en `http://localhost:11434`

### 2. Descargar el modelo

El modelo por defecto es **llama3.2:1b** (pequeño y rápido, ~1.3GB):

```bash
ollama pull llama3.2:1b
```

#### Modelos alternativos:

- **llama3.2:3b** - Más preciso pero más pesado (~2GB)
  ```bash
  ollama pull llama3.2:3b
  ```

- **mistral:7b** - Muy bueno para español (~4GB)
  ```bash
  ollama pull mistral:7b
  ```

### 3. Cambiar el modelo en FactioX

Edita `appsettings.json`:

```json
{
  "Ollama": {
    "Url": "http://localhost:11434",
    "Model": "llama3.2:1b"  // Cambia aquí el modelo
  }
}
```

## Uso

1. Asegúrate de que Ollama esté ejecutándose:
   ```bash
   ollama serve
   ```

2. Abre FactioX en tu navegador

3. Haz clic en el botón flotante verde con el icono de robot en la esquina inferior derecha

4. ¡Comienza a chatear con Factu!

## Configuración en Windows con IIS

### Problema: "¿Está Ollama ejecutándose?"

En servidores Windows con IIS, el Application Pool no puede acceder a servicios locales de otros usuarios. Sigue estos pasos:

#### 1. Configurar Ollama para aceptar conexiones de red

Crea el archivo de configuración de Ollama:

**Ubicación**: `C:\Users\TuUsuario\.ollama\config.json`

```json
{
  "origins": ["*"],
  "models": "C:\\Users\\TuUsuario\\.ollama\\models"
}
```

#### 2. Configurar variables de entorno de Ollama

En PowerShell como administrador:

```powershell
# Permitir acceso desde cualquier IP
[Environment]::SetEnvironmentVariable("OLLAMA_HOST", "0.0.0.0:11434", "Machine")

# Reiniciar el servicio de Ollama (si está instalado como servicio)
Restart-Service ollama
```

O si ejecutas Ollama manualmente:

```powershell
$env:OLLAMA_HOST="0.0.0.0:11434"
ollama serve
```

#### 3. Verificar que Ollama está accesible

Desde otra terminal:

```powershell
curl http://127.0.0.1:11434/api/tags
```

Debería devolver la lista de modelos instalados.

#### 4. Configurar FactioX

En el servidor, edita `appsettings.Production.json` (en la carpeta publicada):

```json
{
  "Ollama": {
    "Url": "http://127.0.0.1:11434",
    "Model": "llama3.2:1b"
  }
}
```

**Importante**: Usa `127.0.0.1` en lugar de `localhost` para evitar problemas de resolución DNS.

#### 5. Configurar firewall (si es necesario)

Si Ollama está en otra máquina:

```powershell
# Permitir conexiones al puerto 11434
New-NetFirewallRule -DisplayName "Ollama" -Direction Inbound -Protocol TCP -LocalPort 11434 -Action Allow
```

#### 6. Instalar Ollama como servicio de Windows

Para que Ollama se inicie automáticamente:

1. Descarga NSSM (Non-Sucking Service Manager): https://nssm.cc/download
2. Instala el servicio:

```powershell
# Ejecutar como administrador
nssm install Ollama "C:\Users\TuUsuario\AppData\Local\Programs\Ollama\ollama.exe" "serve"
nssm set Ollama AppEnvironmentExtra OLLAMA_HOST=0.0.0.0:11434
nssm start Ollama
```

### Verificación final

Desde el navegador del servidor, abre:
- http://127.0.0.1:11434

Deberías ver: `Ollama is running`

## Comandos útiles de Ollama

```bash
# Ver modelos instalados
ollama list

# Eliminar un modelo
ollama rm llama3.2:1b

# Actualizar Ollama
brew upgrade ollama  # macOS

# Ver logs de Ollama
journalctl -u ollama -f  # Linux
```

## Solución de problemas

### Error: "No pude procesar tu solicitud"

- Verifica que Ollama esté ejecutándose: `ps aux | grep ollama`
- Comprueba que el puerto 11434 esté disponible: `lsof -i:11434`
- Reinicia Ollama: `killall ollama && ollama serve`

### El chatbot es muy lento

- Usa un modelo más pequeño: `llama3.2:1b`
- Reduce `num_predict` en IAService.cs (línea 85)
- Considera aumentar la RAM disponible

### Respuestas en inglés

- Usa el modelo `mistral:7b` que es mejor con español
- El prompt del sistema ya está configurado en español

## Privacidad

✅ **Todo se ejecuta localmente** - Tus datos nunca salen de tu máquina  
✅ **Sin costos de API** - 100% gratuito  
✅ **Sin límites de uso** - Úsalo tanto como quieras  
✅ **Offline** - Funciona sin conexión a internet (después de descargar el modelo)

## Desinstalar

```bash
# macOS/Linux
brew uninstall ollama  # o usa el desinstalador de tu sistema

# Eliminar modelos
rm -rf ~/.ollama
```

---

**Nota**: La primera respuesta puede tardar unos segundos mientras Ollama carga el modelo en memoria. Las siguientes respuestas serán más rápidas.
