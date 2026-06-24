# 📄 Extracción de Facturas con OCR - FactioX

## 🎯 Descripción

Esta funcionalidad permite extraer automáticamente los datos de facturas escaneadas o fotografiadas mediante OCR (Reconocimiento Óptico de Caracteres) utilizando IA local con Ollama y el modelo de visión LLaVA.

## ✨ Características

- ✅ Extracción automática de datos de proveedores
- ✅ Identificación de número de factura y fechas
- ✅ Detección de líneas de productos/servicios
- ✅ Cálculo automático de totales e IVA
- ✅ Creación automática de proveedores nuevos
- ✅ Edición manual de datos extraídos antes de guardar
- ✅ Soporte para imágenes JPG, PNG y PDF
- ✅ 100% local y privado (sin enviar datos a la nube)

## 📋 Requisitos Previos

### 1. Ollama instalado

Si no lo tienes, instala Ollama desde [ollama.ai](https://ollama.ai):

**macOS/Linux:**
```bash
curl https://ollama.ai/install.sh | sh
```

**Windows:**
Descarga el instalador desde [ollama.ai/download](https://ollama.ai/download)

### 2. Instalar el modelo LLaVA

Una vez instalado Ollama, ejecuta:

```bash
ollama pull llava
```

Este modelo tiene aproximadamente **4.7 GB**. La descarga puede tardar varios minutos dependiendo de tu conexión.

### 3. Verificar que Ollama está funcionando

```bash
ollama list
```

Deberías ver `llava` en la lista de modelos instalados.

## 🚀 Uso

### Acceso a la funcionalidad

1. Inicia sesión en FactioX
2. Ve al menú **Finanzas** → **Extraer Factura (OCR)**

### Proceso de extracción

#### Paso 1: Subir la imagen
- Haz clic en el botón de selección de archivo
- Elige una imagen (JPG, PNG) o PDF de la factura
- Tamaño máximo: 10 MB
- Verás un preview de la imagen cargada

#### Paso 2: Extraer datos
- Haz clic en el botón **"Extraer Datos"**
- El proceso puede tardar entre 10-60 segundos dependiendo de:
  - Complejidad de la factura
  - Calidad de la imagen
  - Potencia de tu equipo

#### Paso 3: Revisar y editar
- El sistema mostrará todos los datos extraídos:
  - **Datos del proveedor** (nombre, NIF, dirección, etc.)
  - **Información de la factura** (número, fechas)
  - **Líneas de productos/servicios**
  - **Totales** (base imponible, IVA, total)
  
- **Nivel de confianza**: Indica qué tan precisa fue la extracción (0-100%)
  - 🟢 Verde (70-100%): Alta confianza
  - 🟡 Amarillo (40-69%): Confianza media - revisa los datos
  - 🔴 Rojo (0-39%): Baja confianza - verifica todos los campos

- **Advertencias**: Aparecerán si faltan datos importantes

#### Paso 4: Ajustar datos (si es necesario)
- Puedes editar cualquier campo extraído
- Agregar o eliminar líneas de factura
- Corregir importes o cantidades

#### Paso 5: Crear factura
- Haz clic en **"Crear Factura de Compra"**
- El sistema:
  1. Buscará si el proveedor ya existe (por NIF o nombre similar)
  2. Si no existe, creará uno nuevo automáticamente
  3. Creará la factura de compra con todos los datos
  4. Te redirigirá a la lista de facturas de compra

## 💡 Consejos para mejores resultados

### Calidad de la imagen
- ✅ Usa imágenes claras y bien iluminadas
- ✅ Asegúrate de que el texto sea legible
- ✅ Evita imágenes borrosas o con sombras
- ✅ Fotografía la factura de frente (no en ángulo)

### Formato del documento
- ✅ Facturas con estructura clara y ordenada funcionan mejor
- ✅ PDFs nativos (no escaneados) dan mejores resultados
- ⚠️ Facturas manuscritas tienen menor precisión

### Después de la extracción
- ✅ **SIEMPRE revisa** los datos extraídos antes de guardar
- ✅ Presta especial atención a:
  - Importes totales
  - Número de factura
  - NIF del proveedor
  - Fechas

## 🔧 Configuración

La configuración está en `appsettings.json`:

```json
{
  "Ollama": {
    "Url": "http://localhost:11434",
    "Model": "llama3.2:1b",
    "VisionModel": "llava"
  }
}
```

### Cambiar el modelo de visión

Si quieres usar otro modelo de visión:

1. Instala el modelo alternativo:
   ```bash
   ollama pull llava:13b  # Modelo más grande y preciso
   ```

2. Actualiza `appsettings.json`:
   ```json
   "VisionModel": "llava:13b"
   ```

## 🐛 Solución de problemas

### ❌ "Modelo de visión no disponible"

**Causa**: El modelo LLaVA no está instalado.

**Solución**:
```bash
ollama pull llava
```

### ❌ "Error al procesar la imagen: Connection refused"

**Causa**: Ollama no está en ejecución.

**Solución**:
```bash
ollama serve
```

O simplemente abre la aplicación Ollama en tu sistema.

### ❌ La extracción es muy lenta

**Causas posibles**:
- Tu equipo no tiene GPU
- El modelo es muy grande para tu RAM
- Imagen de muy alta resolución

**Soluciones**:
- Usa el modelo `llava:7b` en lugar de modelos más grandes
- Reduce el tamaño de la imagen antes de subirla
- Cierra otras aplicaciones que usen mucha RAM

### ❌ Los datos extraídos son incorrectos

**Soluciones**:
- Mejora la calidad de la imagen
- Asegúrate de que la factura esté completa en la foto
- Edita manualmente los datos antes de guardar
- Prueba con una imagen más clara

### ❌ El porcentaje de confianza es muy bajo

**Normal en casos de**:
- Facturas con formato inusual
- Imágenes de baja calidad
- Facturas con muchos datos
- Textos pequeños o poco legibles

**Solución**: Revisa manualmente todos los campos y corrígelos según sea necesario.

## 📊 Datos extraídos

El sistema intenta extraer:

### Proveedor
- Nombre
- NIF/CIF
- Dirección completa
- Código postal
- Ciudad
- Provincia
- Teléfono
- Email

### Factura
- Número de factura
- Fecha de emisión
- Fecha de vencimiento
- Forma de pago
- Observaciones

### Líneas
Para cada producto/servicio:
- Descripción
- Cantidad
- Precio unitario
- Descuento (%)
- IVA (%)
- Total

### Totales
- Base imponible
- Porcentaje de IVA
- Importe del IVA
- Total de la factura

## 🔐 Privacidad y Seguridad

- ✅ **100% local**: Ningún dato se envía a servicios externos
- ✅ **Sin internet**: Funciona completamente offline (una vez descargado el modelo)
- ✅ **Privacidad total**: Tus facturas no salen de tu equipo
- ✅ **Sin costes**: No hay tarifas por uso ni límites de procesamiento

## 📝 Notas adicionales

- El modelo LLaVA puede consumir entre 4-8 GB de RAM durante el procesamiento
- El primer procesamiento puede ser más lento mientras el modelo se carga en memoria
- Los modelos más grandes (llava:13b, llava:34b) son más precisos pero requieren más recursos

## 🆘 Soporte

Si tienes problemas:

1. Verifica que Ollama está funcionando: `ollama list`
2. Comprueba los logs en la consola de la aplicación
3. Prueba con una imagen más simple primero
4. Consulta la documentación de Ollama: [docs.ollama.ai](https://docs.ollama.ai)

---

**Desarrollado para FactioX** - Sistema de gestión de facturas con IA local
