# 📘 Manual de Usuario - FactioX

**Versión Beta 24062026**

## 🎯 ¿Qué es FactioX?

FactioX es una **solución completa de gestión empresarial** diseñada para autónomos, pequeñas y medianas empresas. Integra facturación, gestión de clientes, control de inventario, presupuestos y análisis financiero en una única plataforma web accesible desde cualquier dispositivo.

---

## 👥 Roles de Usuario

FactioX ofrece **4 niveles de acceso** adaptados a diferentes necesidades:

### 1. **SuperAdministrador** 🔧
- Control total del sistema
- Gestión de múltiples empresas
- Administración de usuarios y permisos
- Acceso a todas las funcionalidades sin restricciones
- Configuración global de la plataforma

### 2. **Administrador** 👔
- Gestión completa de su empresa
- Creación y edición de facturas, presupuestos y documentos
- Administración de clientes, proveedores y productos
- Configuración de la empresa y formas de pago personalizadas
- Acceso a estadísticas y reportes completos

### 3. **Usuario Estándar** 👤
- Consulta de información de su empresa
- Creación de facturas y presupuestos
- Gestión básica de clientes y productos
- Acceso limitado a configuración sensible

### 4. **Asesoría** 📊
- **Perfil especial** diseñado para asesorías fiscales y contables
- **Acceso de solo lectura** a las empresas asociadas
- Consulta de facturas de compra y venta de múltiples clientes
- Exportación de datos para análisis externo
- **Filtrado por empresa** para supervisión individual
- Vista consolidada de todas las empresas bajo supervisión
- No puede modificar ni crear documentos

> **💡 Ventaja para Asesorías:** Un único usuario puede gestionar múltiples empresas cliente, facilitando la supervisión fiscal y contable sin necesidad de múltiples accesos.

---

## 🚀 Funcionalidades Principales

### 📊 **Dashboard Inteligente**
- **Vista unificada** de tu negocio al iniciar sesión
- Estadísticas en tiempo real (ventas, compras, pendientes)
- Gráficos interactivos de ingresos y gastos mensuales
- **Mapa geográfico** de distribución de clientes
- Indicadores clave de rendimiento (KPIs)
- Accesos rápidos a funciones más utilizadas

### 👥 **Gestión de Clientes**
- Ficha completa con datos fiscales (NIF, dirección, CP, ciudad)
- Emails principal y secundario para comunicaciones
- Configuración **SEPA** para domiciliación bancaria (IBAN, BIC)
- **Facturación masiva** opcional por cliente
- Histórico de facturas y presupuestos asociados
- **Geolocalización** en mapa por código postal
- Exportación a Excel de listados completos
- Búsqueda y filtrado avanzado

### 🏢 **Gestión de Proveedores**
- Registro completo de datos fiscales
- Formas de pago personalizables
- Histórico de facturas de compra
- Control de pagos pendientes
- Exportación a Excel

### 📦 **Gestión de Productos/Servicios**
- Catálogo ilimitado de productos y servicios
- Descripción, referencia y precio unitario
- Tipo de IVA configurable (21%, 10%, 4%, 0%)
- Stock y control de inventario
- Búsqueda rápida por nombre o referencia
- Importación/Exportación masiva
- Imágenes de producto (opcional)

### 📝 **Presupuestos**
- Creación rápida desde plantilla
- Múltiples líneas de detalle (productos/servicios)
- Cálculo automático de subtotales, IVA y total
- **Estados**: Pendiente, Enviado, Aceptado, Rechazado
- **Conversión directa a factura** con un clic
- Envío por email automático
- Generación de PDF profesional con logo de empresa
- Validez del presupuesto configurable
- Notas y observaciones personalizables

### 🧾 **Facturación de Venta**
- Numeración automática y secuencial
- Múltiples líneas de factura con productos/servicios
- Descuentos por línea y globales
- Cálculo automático de IVA y retenciones
- **Formas de pago**: Transferencia, Efectivo, Tarjeta, Pagaré, SEPA, Personalizadas
- Fechas de emisión y vencimiento
- **Generación de PDF** profesional con logo corporativo
- **Envío automático por email** al cliente
- Estados: Pendiente, Pagada, Vencida
- **Facturación masiva** para clientes recurrentes

### 🧾 **Facturación de Compra**
- Registro manual de facturas recibidas
- **OCR Inteligente con IA** (extracción automática de datos)
  - Número de factura, fecha, NIF, proveedor
  - Base imponible, IVA, total
  - Dirección, ciudad, código postal
  - **Modo automático**: IA con Ollama + LLaVA (60-80% precisión)
  - **Modo manual**: Selección visual de áreas + Tesseract (90-95% precisión)
- Control de pagos realizados
- Estados: Pendiente, Pagada, Vencida
- Exportación a Excel para contabilidad

### 💰 **Control de Pagos**
- **Registro detallado** de pagos parciales o totales
- Múltiples formas de pago por factura
- Seguimiento de saldo pendiente
- Historial completo de transacciones
- **Remesas SEPA** para domiciliaciones bancarias
- Generación de archivos XML SEPA estándar
- Control de fechas de cobro y vencimiento

### 💳 **Formas de Pago Personalizadas**
- Crear métodos de pago específicos de tu negocio
- Ejemplo: "Bizum", "PayPal", "Contra reembolso", etc.
- Disponibles en facturas de venta y compra
- Gestión completa desde interfaz dedicada

### 🏢 **Gestión Multi-Empresa**
- Cada empresa con su propia base de datos aislada
- Configuración independiente:
  - Datos fiscales (CIF, razón social, dirección)
  - Logo corporativo para documentos
  - Datos bancarios (IBAN, BIC)
  - Información de contacto
  - Plan de suscripción
- Cambio rápido entre empresas (SuperAdmin)
- **Slug único** por empresa para acceso web

### 📈 **Estadísticas y Análisis**
- Gráficos de ingresos y gastos mensuales
- Comparativas año actual vs. año anterior
- Top clientes por facturación
- Facturas pendientes de cobro
- Alertas de vencimientos próximos
- Ratios financieros básicos
- Exportación de datos para análisis externo

### 📧 **Sistema de Comunicación**
- **Envío automático de emails** con plantillas profesionales
- Adjuntar PDF de facturas/presupuestos
- Configuración SMTP personalizable (Gmail, Outlook, servidor propio)
- Historial de comunicaciones por cliente

### 🗺️ **Mapa de Clientes**
- Visualización geográfica de clientes
- Marcadores interactivos por código postal
- Información al hacer clic (nombre, ciudad, email)
- Útil para rutas comerciales y análisis territorial

### 🤖 **Asistente IA Integrado** (ChatBox)
- Consultas en lenguaje natural sobre tus datos
- Ejemplo: "¿Cuánto he facturado este mes?"
- Interpretación de estadísticas y reportes
- Recomendaciones y análisis predictivo
- Integración con Ollama (LLM local)

### 📱 **Progressive Web App (PWA)**
- **Instalable** en móvil, tablet y escritorio como aplicación nativa
- Funciona sin conexión (caché inteligente)
- Notificaciones push (opcional)
- Icono en pantalla de inicio
- Experiencia de aplicación nativa
- Accesos directos: Nueva Factura, Clientes, Presupuestos

### 🔒 **Seguridad y Privacidad**
- Autenticación segura con contraseñas encriptadas
- Sesiones por navegador
- Control de acceso por rol
- Datos aislados por empresa (multi-tenant)
- **Cumplimiento RGPD**
- Política de privacidad y cookies incluida

### 📄 **Generación de Documentos**
- **PDF profesionales** con logo de empresa
- Diseño moderno y personalizable
- Facturas, presupuestos y remesas SEPA
- Exportación a **Excel** de listados completos
  - Clientes, productos, facturas, proveedores
  - Formato compatible con Excel y LibreOffice
  - Filtros aplicados conservados en exportación

### ⚙️ **Configuración Flexible**
- Personalización de información de empresa
- Gestión de usuarios y permisos
- Formas de pago personalizadas
- Configuración de email SMTP
- Logo corporativo para documentos
- Tipos de IVA y retenciones

### 🔄 **Importación y Exportación**
- Importar productos masivamente desde Excel
- Exportar listados completos a Excel
- Backup y restauración de datos (admin)
- Migración entre empresas facilitada

---

## 🎨 Ventajas Competitivas

✅ **Todo en uno**: Facturación + CRM + Inventario + Contabilidad básica  
✅ **Acceso web**: Sin instalaciones, funciona en cualquier dispositivo  
✅ **Multi-empresa**: Gestiona varios negocios desde un único acceso  
✅ **Perfil Asesoría**: Supervisa múltiples clientes con vista consolidada  
✅ **OCR Inteligente**: Digitaliza facturas automáticamente con IA  
✅ **PWA**: Instala como app nativa en móvil o PC  
✅ **SEPA**: Genera remesas bancarias directamente  
✅ **Facturación masiva**: Automatiza facturación recurrente  
✅ **Sin permanencia**: Suscripción flexible y escalable  
✅ **Soporte en español**: Interfaz y documentación 100% en castellano  

---

## 📋 Casos de Uso

### Para Autónomos
- Factura a tus clientes en minutos
- Control de gastos y compras
- Presupuestos profesionales
- Seguimiento de cobros pendientes

### Para Pequeñas Empresas
- Gestión completa de facturación
- Control de inventario de productos
- Múltiples usuarios con permisos
- Estadísticas para toma de decisiones

### Para Asesorías Fiscales
- Supervisa múltiples empresas cliente
- Acceso de solo lectura a todas las facturas
- Exportación para tu software contable
- Vista consolidada de actividad fiscal

### Para Empresas Multi-Sede
- Gestiona varias empresas independientes
- Cambio rápido entre entidades
- Datos completamente aislados
- Configuración individual por empresa

---

## 🖥️ Requisitos Técnicos

- **Navegador web** moderno (Chrome, Firefox, Safari, Edge)
- **Conexión a internet** (funciona con conexiones lentas)
- **No requiere instalación** de software
- Compatible con Windows, macOS, Linux, iOS, Android
- **Responsive**: Se adapta a móvil, tablet y escritorio

---

## 💡 Primeros Pasos

1. **Configura tu empresa**: Datos fiscales, logo, información bancaria
2. **Crea usuarios** (si tienes equipo): Asigna roles y permisos
3. **Importa o crea clientes**: Datos fiscales y de contacto
4. **Añade productos/servicios**: Catálogo con precios e IVA
5. **Crea tu primera factura**: Selecciona cliente, añade productos, genera PDF
6. **Envía por email**: Automáticamente desde la aplicación
7. **Controla los cobros**: Registra pagos y consulta pendientes

---

## 📞 Soporte y Ayuda

- **Interfaz intuitiva**: Diseño limpio y fácil de usar
- **Tooltips contextuales**: Ayuda en cada campo
- **Documentación completa**: Guías paso a paso
- **Actualizaciones automáticas**: Sin intervención del usuario

---

## 🔄 Actualizaciones y Mejoras

FactioX está en **desarrollo continuo** con actualizaciones periódicas que incluyen:

- Nuevas funcionalidades solicitadas por usuarios
- Mejoras de rendimiento y seguridad
- Integración con servicios externos
- Optimización de la experiencia de usuario

**Versión actual**: Beta 11062026  
**Última actualización**: 11 de junio de 2026

---

## 📄 Licencia y Condiciones

FactioX es una aplicación de **suscripción** con diferentes planes adaptados a tus necesidades:

- **Plan Básico**: Para autónomos y freelancers
- **Plan Profesional**: Para pequeñas empresas (hasta 10 usuarios)
- **Plan Empresarial**: Para empresas medianas (usuarios ilimitados)
- **Plan Asesoría**: Para asesorías fiscales (múltiples empresas cliente)

Consulta planes y precios con nuestro equipo comercial.

---

**FactioX** - Tu solución integral de gestión empresarial 🚀

*Desarrollado en España 🇪🇸 | Versión Beta 24062026*
