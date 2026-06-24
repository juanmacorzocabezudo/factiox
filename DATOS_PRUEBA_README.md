# 📊 Generar Datos de Prueba para Empresas

## 🎯 Funcionalidad Implementada

Se ha agregado la capacidad de **generar datos de prueba automáticamente** al crear una nueva empresa en FactioX.

## ✨ Características

### Al Crear una Nueva Empresa

1. **Opción visual en el formulario**
   - Checkbox destacado: "Generar Datos de Prueba"
   - Panel informativo que muestra qué datos se generarán
   - Solo visible al crear empresas nuevas (no en edición)

2. **Datos Generados Automáticamente**

Cuando activas la opción, se generan:

| Entidad | Cantidad | Descripción |
|---------|----------|-------------|
| ⚙️ **Configuración Empresa** | 1 | Datos básicos (NIF, dirección, contacto) |
| 👥 **Clientes** | 5 | Mix de particulares y empresas |
| 🏭 **Proveedores** | 3 | Con datos completos y formas de pago |
| 📦 **Productos/Servicios** | 8 | Variedad de productos y servicios |
| 💳 **Formas de Pago** | 3 | Bizum, PayPal, Stripe |
| 📋 **Presupuestos** | 3 | Con diferentes estados |
| 📄 **Facturas de Venta** | 4 | Con estados variados de pago |
| 🧾 **Facturas de Compra** | 2 | Vinculadas a proveedores |

## 🚀 Cómo Usar

### Desde la Interfaz

1. **Accede como SuperAdministrador**
   - Ir a: Configuración → Empresas
   - O directamente: `/empresas`

2. **Crear Nueva Empresa**
   - Clic en "Nueva Empresa"
   - Completar datos básicos:
     - Nombre Comercial
     - Slug (URL amigable)
     - Plan de Suscripción
     - Máximo de Usuarios

3. **Activar Generación de Datos**
   - ✅ Marcar: "Generar Datos de Prueba"
   - Se mostrará un panel informativo con el detalle de lo que se creará

4. **Guardar**
   - Clic en "Guardar"
   - El sistema creará la empresa
   - Luego generará todos los datos de prueba (puede tardar unos segundos)
   - Aparecerá un mensaje de confirmación

5. **Resultado**
   - Empresa completamente lista para usar
   - Datos de demostración en todas las secciones
   - Perfecto para formación, demos o evaluación

### Crear Empresa Vacía

Si prefieres empezar desde cero:
- Simplemente **NO marques** la opción "Generar Datos de Prueba"
- La empresa se creará sin ningún dato adicional

## 📝 Ejemplos de Datos Generados

### Clientes de Prueba
- Carlos González Martínez (Particular)
- Tecnología Avanzada S.L. (Empresa)
- María López Fernández (Particular)
- Servicios Profesionales BCN (Empresa)
- Ana Ruiz Sánchez (Particular)

### Productos de Prueba
- Servicio de Consultoría (75€/hora)
- Software de Gestión Premium (299€)
- Mantenimiento Web (120€/mes)
- Equipo Informático HP (850€)
- Curso Formación Online (199€)
- Impresora Multifunción (425€)
- Desarrollo Web Personalizado (1500€)
- Pack Material Oficina (89€)

### Proveedores de Prueba
- Suministros Oficina S.A.
- Tecnología Digital S.L.
- Logística Express S.A.

## 💡 Casos de Uso

### 1. **Formación y Capacitación**
```
Escenario: Enseñar a usar FactioX
Acción: Crear empresa con datos de prueba
Beneficio: Los usuarios ven ejemplos reales sin crear datos manualmente
```

### 2. **Demos Comerciales**
```
Escenario: Presentar el producto a clientes potenciales
Acción: Empresa de demo con todos los datos
Beneficio: Mostrar todas las funcionalidades con información visual  
```

### 3. **Testing y Evaluación**
```
Escenario: Probar nuevas funcionalidades
Acción: Crear empresa de prueba con datos
Beneficio: Datos consistentes para pruebas repetibles
```

### 4. **Onboarding de Clientes**
```
Escenario: Cliente nuevo quiere ver el sistema funcionando
Acción: Crear su empresa con datos de ejemplo
Beneficio: Exploran el sistema antes de ingresar sus datos reales
```

## ⚙️ Implementación Técnica

### Archivos Modificados

1. **Servicio de Datos de Prueba**
   - `/Services/DatosPruebaService.cs`
   - Genera todos los datos automáticamente
   - Relaciones correctamente establecidas

2. **Página de Empresas**
   - `/Components/Pages/Empresas.razor`  
   - Checkbox para activar generación
   - Integración con el servicio

3. **Registro de Servicios**
   - `/Program.cs`
   - `IDatosPruebaService` registrado

### Flujo de Ejecución

```
1. Usuario crea empresa ✓
2. [Si marcó opción] Sistema llama a DatosPruebaService
3. Servicio crea datos en orden:
   - Configuración empresa
   - Clientes
   - Proveedores
   - Productos
   - Formas de pago
   - Presupuestos (con líneas)
   - Facturas venta (con líneas y pagos)
   - Facturas compra (con líneas y pagos)
4. Todo se guarda en una transacción
5. Usuario recibe confirmación
```

## 🎯 Beneficios

✅ **Ahorro de tiempo**: No necesitas crear datos manualmente  
✅ **Consistencia**: Todos los datos de prueba son coherentes  
✅ **Realismo**: Los datos parecen casos reales de uso  
✅ **Completo**: Cubre todas las entidades del sistema  
✅ **Flexible**: Opción ON/OFF según necesidad  
✅ **Rápido**: Generación automática en segundos

## 📊 Comparativa

| Aspecto | Sin Datos de Prueba | Con Datos de Prueba |
|---------|---------------------|---------------------|
| Tiempo setup | 30-60 minutos | < 1 minuto |
| Datos creados | 0 (manual) | ~25+ registros |
| Listo para demo | ❌ No | ✅ Sí |
| Formación | Complicado | Fácil e intuitivo |
| Testing | Datos inconsistentes | Datos predecibles |

## ⚡ Próximas Mejoras Sugeridas

- [ ] Poder seleccionar qué tipo de datos generar
- [ ] Diferentes "packs" de datos (básico, completo, avanzado)
- [ ] Opción para regenerar datos en empresas existentes
- [x] Panel visual con lista de datos a generar
- [ ] Progreso visual durante la generación
- [ ] Exportar/importar sets de datos de prueba personalizados

---

✅ **Funcionalidad añadida el:** 10 de marzo de 2026  
📝 **Versión:** 1.0  
👤 **Disponible para:** SuperAdministrador únicamente  
🎯 **Estado:** En desarrollo (refinando servicio de generación)
