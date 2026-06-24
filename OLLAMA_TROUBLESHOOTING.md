# Solución de Problemas con Ollama y LLaVA

## Error: "model runner has unexpectedly stopped"

Este error indica que Ollama se quedó sin memoria (RAM) al procesar la imagen.

### Soluciones

#### 1. Reducir tamaño de imagen (Recomendado)
- **Tamaño óptimo**: 500KB - 1MB
- **Tamaño máximo**: 3MB
- **Herramientas para redimensionar**:
  - Mac: Vista Previa → Herramientas → Ajustar tamaño
  - Online: tinypng.com, compressor.io
  - Comando: `sips -Z 1024 factura.jpg` (redimensiona a 1024px máximo)

**Ejemplo con sips (Mac)**:
```bash
# Redimensionar imagen a máximo 1024px manteniendo proporción
sips -Z 1024 factura_original.jpg --out factura_optimizada.jpg

# Comprimir calidad JPEG al 70%
sips -s format jpeg -s formatOptions 70 factura.jpg --out factura_comprimida.jpg
```

#### 2. Reiniciar Ollama
```bash
# Detener Ollama
killall ollama

# Iniciar de nuevo
ollama serve
```

#### 3. Liberar memoria del sistema
- Cierra aplicaciones que no uses
- Reinicia el Mac si es necesario
- Verifica memoria disponible: Actividad de Monitor → Memoria

#### 4. Usar modelo más pequeño (Alternativa)
Si LLaVA sigue fallando, puedes probar con un modelo de visión más pequeño:

```bash
# Descargar modelo más pequeño (no recomendado para OCR)
ollama pull moondream

# Luego actualizar appsettings.json:
"VisionModel": "moondream"
```

⚠️ **Nota**: Modelos más pequeños tienen menor precisión en OCR.

#### 5. Aumentar recursos de Ollama (Avanzado)
Edita las variables de entorno de Ollama:

```bash
# En ~/.zshrc o ~/.bashrc
export OLLAMA_NUM_PARALLEL=1
export OLLAMA_MAX_LOADED_MODELS=1

# Aplicar cambios
source ~/.zshrc
```

### Verificar estado de Ollama

```bash
# Ver modelos instalados
ollama list

# Ver logs de Ollama
tail -f ~/.ollama/logs/server.log

# Probar modelo manualmente
ollama run llava
```

### Recomendaciones para mejores resultados

1. **Calidad de imagen**:
   - Resolución mínima: 800x600px
   - Formato preferido: JPG con calidad 70-80%
   - Evita PDFs escaneados muy pesados

2. **Contenido**:
   - Imagen clara y legible
   - Buena iluminación
   - Texto nítido sin desenfoques

3. **Preparación**:
   - Recorta márgenes innecesarios
   - Endereza la imagen si está inclinada
   - Asegúrate de que todo el texto sea visible

### Recursos del sistema recomendados

- **RAM mínima**: 8GB (16GB recomendado)
- **Espacio libre**: 10GB mínimo
- **CPU**: 4 núcleos o más

### Contacto y soporte

Si el problema persiste:
1. Revisa los logs de Ollama: `~/.ollama/logs/server.log`
2. Verifica el monitor de actividad para ver uso de memoria
3. Consulta: https://github.com/ollama/ollama/issues
