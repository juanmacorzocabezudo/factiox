# Formas de Pago Personalizadas

## Descripción

Esta funcionalidad permite a cada empresa gestionar sus propias formas de pago personalizadas para las facturas de venta. La forma de pago se define a nivel de factura y es heredada automáticamente por los pagos parciales registrados.

## Cambios Implementados

### 1. Nuevos Modelos

- **FormaPagoPersonalizada**: Modelo para gestionar formas de pago personalizadas
  - `Id`: Identificador único
  - `Nombre`: Nombre de la forma de pago (ej: PayPal, Stripe, Contrareembolso)
  - `Descripcion`: Descripción opcional
  - `Activo`: Indica si está activa y disponible
  - `EsPredefinido`: Las formas predefinidas no se pueden eliminar
  - `EmpresaId`: Soporte multi-tenant

### 2. Modificaciones en Factura

- **FormaPago** (enum): Ahora es **nullable** para compatibilidad con datos antiguos
- **FormaPagoPersonalizadaId**: Nueva relación con FormaPagoPersonalizada
- **FormaPagoPersonalizada**: Navegación a la entidad FormaPagoPersonalizada

### 3. Modificaciones en PagoFactura

- **FormaPago** (enum): Ahora es **nullable** para compatibilidad
- **FormaPagoPersonalizadaId**: Relación que hereda la forma de pago de la factura
- **FormaPagoPersonalizada**: Navegación a la entidad

### 4. Servicios

- **IFormaPagoPersonalizadaService / FormaPagoPersonalizadaService**: 
  - CRUD completo de formas de pago
  - Método `InicializarFormasPredefinidasAsync()` para crear formas por defecto
  - Solo usuarios de la empresa pueden gestionar sus formas de pago

### 5. Componentes

- **FormaPagoList.razor** (`/configuracion/formas-pago`):
  - Listado de formas de pago
  - Crear, editar y eliminar formas personalizadas
  - Activar/desactivar formas de pago
  - Inicializar formas predefinidas

- **FacturaForm.razor** (actualizado):
  - Selector de forma de pago personalizada en la factura
  - Enlace directo a configuración de formas de pago
  - Modal de gestión de pagos muestra la forma de pago de la factura
  - Los pagos heredan automáticamente la forma de pago de la factura

### 6. Base de Datos

- **Migración**: `20260223155622_RenombrarTiposPagoAFormasPagoPersonalizadas`
- **Script SQL**: `Database/AddFormasPagoPersonalizadas.sql`

## Formas de Pago Predefinidas

Al inicializar, se crean automáticamente las siguientes formas para cada empresa:

1. Efectivo
2. Transferencia
3. Tarjeta Crédito
4. Tarjeta Débito
5. Bizum
6. Domiciliación

## Uso

### Para Administradores

1. Acceder a **Configuración > Formas de Pago**
2. Ver formas predefinidas y personalizadas
3. Crear nuevas formas de pago (ej: PayPal, Stripe, Western Union)
4. Editar nombre, descripción y estado
5. Activar/desactivar formas según necesidad

### Al Crear/Editar Facturas

1. En el formulario de factura, seleccionar la forma de pago del desplegable
2. Solo aparecen las formas activas
3. Enlace directo a configuración si necesitas añadir una forma nueva
4. La forma seleccionada se aplicará automáticamente a todos los pagos de esta factura

### Al Registrar Pagos

1. En el formulario de factura, clic en **Gestionar Pagos**
2. La forma de pago de la factura se muestra automáticamente
3. No se puede cambiar la forma de pago en los pagos individuales
4. Todos los pagos parciales usan la misma forma de pago de la factura

## Flujo de Trabajo

```
Factura
  ├─ FormaPagoPersonalizadaId (definida al crear/editar factura)
  └─ Pagos[]
      └─ FormaPagoPersonalizadaId (heredada automáticamente de la factura)
```

## Compatibilidad

-  Las facturas antiguas con `FormaPago` (enum) siguen funcionando
- Se muestra correctamente: `FormaPagoPersonalizada?.Nombre ?? FormaPago?.ToString()`
- Las nuevas facturas usan `FormaPagoPersonalizadaId` exclusivamente
- No se rompe funcionalidad existente

## Validaciones

- No se pueden eliminar formas predefinidas
- No se pueden eliminar formas en uso (constraint de BD)
- Nombre es obligatorio (máx 100 caracteres)
- Descripción opcional (máx 500 caracteres)
- Multi-tenant: cada empresa solo ve sus formas

## Aplicar Cambios en Producción

### Opción 1: Con Entity Framework (Recomendado)

```bash
dotnet ef database update
```

### Opción 2: Script SQL Manual

```bash
mysql -u usuario -p FactioX < Database/AddFormasPagoPersonalizadas.sql
```

## Verificación

Después de aplicar los cambios:

```sql
-- Ver formas de pago por empresa
SELECT e.NombreComercial, fp.*
FROM FormasPagoPersonalizadas fp
INNER JOIN Empresas e ON fp.EmpresaId = e.Id;

-- Ver facturas con forma de pago personalizada
SELECT 
    f.NumeroFactura,
    fp.Nombre as FormaPago,
    f.Total
FROM Facturas f
LEFT JOIN FormasPagoPersonalizadas fp ON f.FormaPagoPersonalizadaId = fp.Id;
```

## Archivos Creados

- `Models/FormaPagoPersonalizada.cs`
- `Services/FormaPagoPersonalizadaService.cs`
- `Components/Pages/FormaPagoList.razor`
- `Database/AddFormasPagoPersonalizadas.sql`

## Archivos Modificados

- `Models/Factura.cs`
- `Models/PagoFactura.cs`
- `Components/Pages/FacturaForm.razor`
- `Components/Pages/Configuracion.razor`
- `Data/ApplicationDbContext.cs`
- `Program.cs`
