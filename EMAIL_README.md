# Configuración de Email - FactioX

## Funcionalidad

FactioX puede enviar facturas por email automáticamente a tus clientes en formato PDF. El email se envía al correo configurado en la ficha del cliente, con copia (CC) al email secundario si existe.

## Configuración

### 1. Editar appsettings.json

Abre el archivo `appsettings.json` y configura la sección `Email`:

```json
{
  "Email": {
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": "587",
    "SmtpUser": "tu-email@gmail.com",
    "SmtpPassword": "tu-contraseña-de-aplicacion",
    "FromEmail": "tu-email@gmail.com",
    "FromName": "FactioX",
    "EnableSsl": "true"
  }
}
```

### 2. Configuración para Gmail

Si usas **Gmail**, necesitas crear una **contraseña de aplicación**:

1. Ve a tu cuenta de Google: https://myaccount.google.com/
2. Navega a **Seguridad** → **Verificación en dos pasos** (actívala si no está activada)
3. Busca **Contraseñas de aplicaciones**
4. Selecciona **Correo** y **Otro (Nombre personalizado)**
5. Escribe "FactioX" y haz clic en **Generar**
6. Copia la contraseña generada (16 caracteres sin espacios)
7. Pega esa contraseña en `SmtpPassword` en tu `appsettings.json`

**Configuración típica para Gmail:**
```json
{
  "Email": {
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": "587",
    "SmtpUser": "tu-email@gmail.com",
    "SmtpPassword": "abcd efgh ijkl mnop",
    "FromEmail": "tu-email@gmail.com",
    "FromName": "Tu Empresa",
    "EnableSsl": "true"
  }
}
```

### 3. Configuración para Outlook/Hotmail

**Configuración para Outlook:**
```json
{
  "Email": {
    "SmtpHost": "smtp-mail.outlook.com",
    "SmtpPort": "587",
    "SmtpUser": "tu-email@outlook.com",
    "SmtpPassword": "tu-contraseña",
    "FromEmail": "tu-email@outlook.com",
    "FromName": "Tu Empresa",
    "EnableSsl": "true"
  }
}
```

### 4. Otros proveedores SMTP

- **Office 365**: `smtp.office365.com`, puerto `587`
- **Yahoo**: `smtp.mail.yahoo.com`, puerto `587`
- **SendGrid**: `smtp.sendgrid.net`, puerto `587`
- **Mailgun**: `smtp.mailgun.org`, puerto `587`

## Uso

### Desde el listado de facturas

1. Ve a **Facturas de Venta**
2. Haz clic en el botón **editar** de una factura
3. En el modal, haz clic en **Enviar Email**
4. El sistema enviará el PDF de la factura al email del cliente

### Desde la edición de factura

1. Abre una factura para editarla
2. Haz clic en el botón **Enviar Email** (icono de sobre)
3. La factura se enviará automáticamente

## ¿Qué contiene el email?

El email incluye:

- **Asunto**: Factura [número] - [Nombre Empresa]
- **Cuerpo del email**:
  - Saludo personalizado con nombre del cliente
  - Número de factura y fecha
  - Importe total
  - Datos de contacto de tu empresa (email y teléfono)
- **Archivo adjunto**: PDF de la factura con todos los datos

## Destinatarios

- **Para (To)**: Email principal del cliente (campo `Email` en la ficha del cliente)
- **CC (Copia)**: Email secundario del cliente si existe (campo `EmailSecundario`)

## Requisitos previos

1. El cliente debe tener un **email configurado** en su ficha
2. El servidor SMTP debe estar **correctamente configurado** en `appsettings.json`
3. La configuración de tu empresa debe estar completa (nombre, email, teléfono)

## Solución de problemas

### Error: "El cliente no tiene un email configurado"

- Ve a **Clientes** → Edita el cliente → Completa el campo **Email**

### Error: "La configuración de email no está completa"

- Verifica que todos los campos de `Email` en `appsettings.json` estén completos
- Asegúrate de que no haya comillas adicionales o espacios

### Error: "Autenticación fallida" (Gmail)

- Verifica que hayas activado la **verificación en dos pasos**
- Usa una **contraseña de aplicación**, no tu contraseña normal de Gmail
- Revisa que el email y contraseña en `appsettings.json` sean correctos

### Error: "No se pudo enviar el email"

- Comprueba tu conexión a internet
- Verifica que el puerto no esté bloqueado por firewall
- Prueba cambiar `SmtpPort` a `25` o `465` (con SSL)

### El email no llega

- Revisa la **carpeta de spam** del destinatario
- Espera unos minutos (a veces hay retraso)
- Verifica que el email del cliente sea correcto

## Seguridad

⚠️ **Importante**: 
- No compartas tu `appsettings.json` públicamente ya que contiene tu contraseña SMTP
- Usa contraseñas de aplicación en lugar de tu contraseña principal
- En producción, usa variables de entorno o Azure Key Vault para las credenciales

## Formato del PDF

El PDF adjunto incluye:

- Logo de la empresa (si está configurado)
- Datos completos de la empresa y cliente
- Todas las líneas de productos/servicios
- Desglose de importes (base, IVA, descuentos, etc.)
- Información legal y de protección de datos
- Número de página

---

**Nota**: Esta funcionalidad requiere que tengas configurado un servidor SMTP válido. Si no tienes uno, puedes usar Gmail con una contraseña de aplicación (es gratuito).
