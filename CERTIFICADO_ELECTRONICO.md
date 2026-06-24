# FactioX - Configuración de Certificado Electrónico para FacturaE

## Descripción

La funcionalidad de certificado electrónico permite a las empresas firmar digitalmente sus facturas electrónicas en formato FacturaE, proporcionando autenticidad y validez legal a los documentos.

## Requisitos

- Certificado digital en formato **.pfx** o **.p12**
- Contraseña del certificado digital
- Tamaño máximo del archivo: 5 MB

## Configuración

### 1. Acceder a la configuración de empresa

1. Inicie sesión en FactioX
2. Navegue a **Configuración** → **Información empresa**
3. Desplácese hasta la sección **"Certificado Electrónico para FacturaE"**

### 2. Cargar el certificado

1. Haga clic en el botón **"Examinar"** o **"Choose File"**
2. Seleccione su archivo de certificado (.pfx o .p12)
3. Introduzca la contraseña del certificado en el campo correspondiente
4. Haga clic en **"Guardar Configuración"**

### 3. Verificación

Una vez guardado, verá un mensaje de confirmación:
- ✅ **Certificado configurado:** [nombre del archivo]

## Tipos de certificados compatibles

- **Formato .pfx** (Personal Information Exchange)
- **Formato .p12** (PKCS#12)

## Seguridad

- Los certificados se almacenan en el servidor en una carpeta protegida
- La contraseña del certificado se guarda encriptada en la base de datos
- Los archivos de certificado no se sincronizan con el control de versiones (Git)

## Gestión del certificado

### Eliminar certificado

1. En la sección del certificado, haga clic en el botón **"Eliminar"**
2. Confirme la acción
3. El certificado será eliminado tanto del sistema de archivos como de la base de datos

### Actualizar certificado

Para actualizar el certificado:
1. Simplemente seleccione un nuevo archivo de certificado
2. Introduzca la nueva contraseña (si es diferente)
3. Guarde la configuración

El sistema eliminará automáticamente el certificado anterior y guardará el nuevo.

## Uso en FacturaE

Una vez configurado el certificado, este se utilizará automáticamente para:
- Firmar facturas electrónicas en formato FacturaE
- Validar la autenticidad de las facturas emitidas
- Cumplir con los requisitos legales de facturación electrónica

## Solución de problemas

### Error: "El archivo debe tener extensión .pfx o .p12"
**Solución:** Asegúrese de que el archivo seleccionado tiene la extensión correcta.

### Error: "El archivo no puede superar 5 MB"
**Solución:** El certificado es demasiado grande. Los certificados digitales normalmente no superan 1 MB. Verifique que está seleccionando el archivo correcto.

### Error al guardar el certificado
**Solución:** 
- Verifique que tiene permisos de escritura en el servidor
- Compruebe que la contraseña del certificado es correcta
- Contacte con el administrador del sistema si el problema persiste

## Renovación del certificado

Los certificados digitales tienen una fecha de caducidad. Se recomienda:
- Verificar la fecha de caducidad de su certificado regularmente
- Actualizar el certificado antes de que caduque
- Mantener una copia de seguridad del certificado en un lugar seguro

## Soporte

Para más información sobre certificados digitales o problemas técnicos, contacte con:
- Soporte técnico de FactioX
- Su proveedor de certificados digitales
