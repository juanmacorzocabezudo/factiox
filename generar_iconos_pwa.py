#!/usr/bin/env python3
"""
Script para generar iconos PWA en diferentes tamaños desde el logo principal
Requiere: pip install pillow
"""

from PIL import Image
import os

# Configuración
LOGO_SOURCE = "wwwroot/LogoFactioxFIN.png"
OUTPUT_DIR = "wwwroot"

# Tamaños de iconos necesarios para PWA
ICON_SIZES = [
    (192, 192, "icon-192.png"),
    (512, 512, "icon-512.png"),
    (192, 192, "icon-maskable-192.png"),
    (512, 512, "icon-maskable-512.png"),
    (72, 72, "icon-72.png"),
    (96, 96, "icon-96.png"),
    (128, 128, "icon-128.png"),
    (144, 144, "icon-144.png"),
    (152, 152, "icon-152.png"),
    (384, 384, "icon-384.png"),
]

def crear_icono(source_path, output_path, width, height, add_padding=False):
    """Crea un icono redimensionado desde la imagen fuente"""
    try:
        # Abrir la imagen original
        with Image.open(source_path) as img:
            # Convertir a RGBA si no lo está
            if img.mode != 'RGBA':
                img = img.convert('RGBA')
            
            if add_padding:
                # Para iconos maskable, agregar padding del 10%
                padding = int(width * 0.1)
                new_size = width - (padding * 2)
                
                # Redimensionar la imagen
                img_resized = img.resize((new_size, new_size), Image.Resampling.LANCZOS)
                
                # Crear una imagen nueva con padding
                new_img = Image.new('RGBA', (width, height), (255, 255, 255, 0))
                
                # Pegar la imagen redimensionada en el centro
                position = (padding, padding)
                new_img.paste(img_resized, position, img_resized)
                
                img = new_img
            else:
                # Redimensionar directamente
                img = img.resize((width, height), Image.Resampling.LANCZOS)
            
            # Guardar el icono
            img.save(output_path, 'PNG', optimize=True)
            print(f"✓ Creado: {output_path} ({width}x{height})")
            return True
    except Exception as e:
        print(f"✗ Error al crear {output_path}: {e}")
        return False

def main():
    print("=" * 60)
    print("Generador de Iconos PWA para FactioX")
    print("=" * 60)
    
    # Verificar que existe el logo fuente
    if not os.path.exists(LOGO_SOURCE):
        print(f"❌ Error: No se encuentra el archivo {LOGO_SOURCE}")
        return
    
    print(f"\n📁 Imagen fuente: {LOGO_SOURCE}")
    print(f"📁 Directorio de salida: {OUTPUT_DIR}\n")
    
    # Crear los iconos
    success_count = 0
    for width, height, filename in ICON_SIZES:
        output_path = os.path.join(OUTPUT_DIR, filename)
        add_padding = "maskable" in filename
        
        if crear_icono(LOGO_SOURCE, output_path, width, height, add_padding):
            success_count += 1
    
    print(f"\n{'=' * 60}")
    print(f"✓ Proceso completado: {success_count}/{len(ICON_SIZES)} iconos creados")
    print(f"{'=' * 60}")

if __name__ == "__main__":
    main()
