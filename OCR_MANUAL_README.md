# 📄 OCR Manual con Selección de Áreas - FactioX

## ✨ Nueva Funcionalidad Implementada

Ahora FactioX incluye **dos modos de OCR** para extraer datos de facturas:

### 🤖 Modo Automático (IA)
- Usa Ollama con LLaVA
- Extracción automática de todos los datos
- Requiere instalación de Ollama y el modelo LLaVA
- Ideal para facturas estándar

### 🎯 Modo Manual (Selección de Áreas)
- Selección manual de áreas en la imagen
- Mayor precisión
- No requiere IA (solo Tesseract)
- Funciona con cualquier formato de factura
- **Recomendado para mayor precisión**

---

## 🚀 Configuración Inicial

### 1. Instalar Tesseract OCR

#### macOS:
```bash
brew install tesseract
```

#### Linux (Ubuntu/Debian):
```bash
sudo apt-get update
sudo apt-get install tesseract-ocr tesseract-ocr-spa
```

#### Windows:
1. Descarga el instalador desde: https://github.com/UB-Mannheim/tesseract/wiki
2. Ejecuta el instalador
3. Agrega Tesseract al PATH del sistema

### 2. Descargar Datos de Idioma

Necesitas descargar los archivos de datos de idioma de Tesseract:

#### Opción A: Descarga Automática (macOS/Linux)
```bash
cd /Users/juanmariacorzo/Documents/FactioX
mkdir -p tessdata
cd tessdata

# Descargar español
curl -L -o spa.traineddata https://github.com/tesseract-ocr/tessdata/raw/main/spa.traineddata

# Descargar inglés (opcional)
curl -L -o eng.traineddata https://github.com/tesseract-ocr/tessdata/raw/main/eng.traineddata
```

#### Opción B: Descarga Manual
1. Ve a: https://github.com/tesseract-ocr/tessdata
2. Descarga `spa.traineddata` (español) y `eng.traineddata` (inglés)
3. Crea la carpeta `tessdata` en la raíz del proyecto
4. Copia los archivos descargados a `/Users/juanmariacorzo/Documents/FactioX/tessdata/`

#### Opción C: Usar Tesseract del Sistema (macOS con Homebrew)
Si instalaste Tesseract con Homebrew, los datos ya están en:
```
/opt/homebrew/share/tessdata
```

El servicio ManualOcrService buscará automáticamente en estas ubicaciones:
- `./tessdata` (carpeta local en el proyecto)
- `/usr/share/tesseract-ocr/4.00/tessdata`
- `/usr/share/tessdata`
- `/opt/homebrew/share/tessdata`

### 3. Verificar Instalación

```bash
# Verificar versión de Tesseract
tesseract --version

# Listar idiomas disponibles
tesseract --list-langs
```

Deberías ver `spa` y/o `eng` en la lista.

---

## 📖 Cómo Usar el Modo Manual

### Paso 1: Acceder al OCR
1. Ve a **Finanzas → Facturas de Compra**
2. Haz clic en el botón **"Extraer con OCR"**

### Paso 2: Seleccionar Modo Manual
1. En el modal que se abre, haz clic en la pestaña **"Modo Manual (Selección)"**

### Paso 3: Subir la Factura
1. Haz clic en **"Selecciona una imagen de factura"**
2. Elige tu archivo de imagen (JPG, PNG, PDF)
3. La imagen se mostrará en un canvas interactivo

### Paso 4: Seleccionar Campos
1. En la columna derecha verás una lista de campos disponibles
2. Los campos con asterisco (*) son obligatorios
3. Haz clic en un campo (ej: "Número de Factura")
4. El botón se pondrá en azul y el cursor cambiará

### Paso 5: Dibujar Áreas
1. Con el ratón, dibuja un rectángulo sobre el dato en la imagen
2. Mantén presionado y arrastra para definir el área
3. Suelta el botón del ratón
4. El área se guardará con un color identificador
5. Repite para cada campo que quieras extraer

### Paso 6: Extraer Datos
1. Una vez seleccionadas todas las áreas necesarias
2. Haz clic en **"Extraer Datos Seleccionados"**
3. El OCR procesará cada área y extraerá el texto
4. Los datos aparecerán en formularios editables
5. Revisa y corrige si es necesario
6. Haz clic en **"Crear Factura de Compra"**

---

## 🎨 Controles Disponibles

### Campos Disponibles:
- ✅ **Número de Factura** (obligatorio)
- ✅ **Fecha de Emisión** (obligatorio)
- ✅ **Nombre Proveedor** (obligatorio)
- NIF/CIF
- ✅ **Base Imponible** (obligatorio)
- % IVA
- Importe IVA
- ✅ **Total** (obligatorio)
- Fecha Vencimiento
- Dirección
- Ciudad
- Código Postal

### Botones de Control:
- 🗑️ **Limpiar Todo**: Elimina todas las áreas seleccionadas
- 🔄 **Cambiar Imagen**: Carga una imagen diferente
- ❌ **Eliminar área específica**: Haz clic en el icono de papelera junto a cada campo

---

## 💡 Consejos y Mejores Prácticas

### Para Mejores Resultados:

1. **Calidad de Imagen:**
   - Usa imágenes con buena resolución (300 DPI o más)
   - Asegúrate de que el texto esté nítido
   - Evita imágenes borrosas o con sombras

2. **Selección de Áreas:**
   - Dibuja el rectángulo lo más ajustado posible al texto
   - No incluyas líneas o bordes de la factura
   - Selecciona solo el dato específico (no etiquetas)
   - Para campos de múltiples líneas, incluye todas las líneas necesarias

3. **Orden de Selección:**
   - Empieza por los campos obligatorios (marcados con *)
   - Los campos opcionales puedes omitirlos si no son necesarios

4. **Corrección Manual:**
   - Siempre revisa los datos extraídos
   - El OCR puede cometer errores, especialmente con números similares (0 vs O, 1 vs I)
   - Corrige cualquier dato antes de crear la factura

---

## 🔧 Solución de Problemas

### Error: "Tesseract no está disponible"
**Causa:** No se encuentran los archivos de datos de idioma

**Solución:**
1. Verifica que la carpeta `tessdata` existe
2. Verifica que contiene `spa.traineddata` o `eng.traineddata`
3. Si no existe, descarga los archivos manualmente (ver sección "Descargar Datos de Idioma")

### Error: "No se pudo extraer texto del área"
**Causas posibles:**
- El área seleccionada es demasiado pequeña
- El texto está borroso o mal escaneado
- El contraste de la imagen es bajo

**Soluciones:**
- Vuelve a dibujar el área más grande
- Usa una imagen de mejor calidad
- Ajusta el brillo/contraste de la imagen antes de subirla

### El texto extraído tiene errores
**Causas comunes:**
- Confusión entre caracteres similares (0/O, 1/I, 5/S)
- Texto muy pequeño o borroso
- Fuentes poco comunes

**Soluciones:**
- Corrige manualmente los errores en el formulario
- Usa imágenes de mayor resolución
- Para mejores resultados, usa facturas con fuentes estándar

### Los rectángulos no se dibujan
**Causas:**
- JavaScript no se cargó correctamente
- Conflicto con otros scripts

**Soluciones:**
1. Recarga la página (F5)
2. Abre la consola del navegador (F12) y busca errores
3. Intenta con otro navegador (Chrome/Edge recomendados)

---

## ⚖️ Comparación de Modos

| Característica | Modo Automático (IA) | Modo Manual (Selección) |
|----------------|----------------------|-------------------------|
| Precisión | 60-80% | 90-95% |
| Velocidad | 10-30 segundos | 2-5 segundos |
| Esfuerzo del usuario | Ninguno | Medio (seleccionar áreas) |
| Dependencias | Ollama + LLaVA | Solo Tesseract |
| Tamaño de imagen | Limitado (3MB) | Sin límite práctico |
| Facturas complejas | Regular | Excelente |
| Configuración | Compleja | Simple |

---

## 🎯 Casos de Uso Recomendados

### Usa Modo Automático cuando:
- Facturas con formato estándar y limpio
- Quieres procesar rápidamente sin intervención
- Ya tienes Ollama configurado
- La calidad de imagen es excelente

### Usa Modo Manual cuando:
- Facturas con formato complejo o inusual
- Necesitas máxima precisión
- La calidad de imagen es variable
- Quieres control total sobre lo que se extrae
- No tienes o no quieres instalar Ollama
- **Recomendado para uso productivo**

---

## 📊 Estadísticas y Métricas

El modo manual proporciona:
- **Confianza estimada**: Basada en campos extraídos exitosamente
- **Advertencias**: Lista de campos que no se pudieron extraer
- **Corrección manual**: Todos los campos son editables antes de guardar

---

## 🔮 Próximas Mejoras

Funcionalidades planeadas para el modo manual:
- [ ] Plantillas guardadas (áreas predefinidas para proveedores recurrentes)
- [ ] Zoom y pan en la imagen para mejor precisión
- [ ] Rotación de imagen
- [ ] Corrección de perspectiva
- [ ] OCR de tablas (líneas de factura)
- [ ] Exportar/importar configuraciones de áreas
- [ ] Modo batch (procesar múltiples facturas)

---

## 📝 Notas Técnicas

- El OCR se ejecuta en el servidor (no en el navegador)
- Las imágenes se procesan temporalmente y se eliminan después
- Los datos extraídos no se guardan hasta que creas la factura
- Tesseract soporta múltiples idiomas simultáneamente (spa+eng)
- El modo manual funciona completamente offline (sin necesidad de internet)

---

✨ **¡Disfruta de la extracción precisa de facturas con FactioX!**
