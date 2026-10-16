---
description: "Desarrollar el módulo TPV (Terminal Punto de Venta) de FactioX, activable por empresa e integrado con Productos, Presupuestos y Facturas."
agent: agent
---

# Objetivo

Desarrolla en FactioX un **módulo TPV** (punto de venta) que funcione como **módulo adicional activable por empresa**. El TPV no es una aplicación aparte: es otra forma de operar sobre las entidades existentes (Productos, Clientes, Presupuestos, Facturas, Pagos), optimizada para venta rápida en mostrador, con pantalla táctil, lector de código de barras e impresión de tickets.

Trabaja por fases. Al terminar cada fase, compila y corrige errores antes de continuar. No publiques ni toques el servidor de producción.

# Contexto de la aplicación (verifícalo antes de empezar)

- **Stack**: .NET 8, Blazor Interactive Server, EF Core con Pomelo MySQL 8.0.2, Bootstrap 5.3 + Bootstrap Icons, QuestPDF para PDF, cultura `es-ES`.
- **Datos**: `IDbContextFactory<ApplicationDbContext>` en [Data/ApplicationDbContext.cs](../../Data/ApplicationDbContext.cs); servicios scoped registrados en [Program.cs](../../Program.cs).
- **Multiempresa**: [Models/Empresa.cs](../../Models/Empresa.cs) (`Activa`, `PlanSuscripcion`, `MaxUsuarios`). El filtrado por `EmpresaId` se hace **en la capa de servicios** usando `ITenantService` ([Services/TenantService.cs](../../Services/TenantService.cs)); el SuperAdministrador ve todo. Sigue exactamente el patrón de [Services/FacturaService.cs](../../Services/FacturaService.cs).
- **Roles** ([Models/Usuario.cs](../../Models/Usuario.cs)): `SuperAdministrador`, `Administrador`, `Usuario`, `Asesoría` (con `UsuarioEmpresa`). Autenticación propia con `IAuthService` + `ProtectedSessionStorage`; páginas protegidas con `AuthGuard`.
- **Entidades relacionadas**:
  - [Models/Producto.cs](../../Models/Producto.cs): `CodigoBarras`, `Referencia`, `Categoria`, `PrecioUnitario`, `IVA`, `Descuento`, `Stock`, `StockMinimo`, `Imagen`, `TipoProducto` (Producto/Servicio), `Activo`.
  - [Models/LineaDocumento.cs](../../Models/LineaDocumento.cs): líneas compartidas por Factura y Presupuesto (`FacturaId?`, `PresupuestoId?`, `ProductoId?`, `CalcularImportes()`).
  - [Models/Factura.cs](../../Models/Factura.cs): `TipoFactura`, `ClaseFactura` (Original/Rectificativa), `FacturaOriginalId`, `Estado`, `Pagos`, `FormaPagoPersonalizadaId`.
  - [Models/Presupuesto.cs](../../Models/Presupuesto.cs): `Estado` (Borrador/Enviado/Aceptado/Rechazado/Caducado).
  - [Models/PagoFactura.cs](../../Models/PagoFactura.cs) y [Models/FormaPagoPersonalizada.cs](../../Models/FormaPagoPersonalizada.cs).
  - [Models/ConfiguracionEmpresa.cs](../../Models/ConfiguracionEmpresa.cs): series y contadores (`SerieFactura`, `SerieAbonoVenta`, `SeriePresupuesto`…). La numeración se genera en `FacturaService.GenerarNumeroAsync`.
- **Marcador actual del TPV**: [Components/Layout/NavMenu.razor](../../Components/Layout/NavMenu.razor) (menú "Módulos disponibles", enlace `modulo-no-disponible/TPV`) y [Components/Pages/ModuloNoDisponible.razor](../../Components/Pages/ModuloNoDisponible.razor).
- **Abonos**: ver [ABONOS_README.md](../../ABONOS_README.md). Reutiliza ese mecanismo para devoluciones de tickets ya facturados.
- **Verifactu**: revisa [Components/Pages/Verifactu.razor](../../Components/Pages/Verifactu.razor) y cualquier servicio relacionado. Los tickets son legalmente **facturas simplificadas** (art. 4 y 7 del RD 1619/2012): numeración correlativa propia, sin huecos, inmutables una vez emitidos. Si existe lógica Verifactu/registro de facturas, integra los tickets de forma coherente; si no existe, deja el modelo preparado (hash/encadenamiento opcional) pero no inventes envíos a la AEAT.

# Fase 1 — Sistema de módulos por empresa

1. Crea un sistema genérico de módulos (reutilizable para Contabilidad, Almacén y Producción en el futuro):
   - Enum `TipoModulo { TPV, Contabilidad, Almacen, Produccion }`.
   - Entidad `EmpresaModulo` (`Id`, `EmpresaId`, `Modulo`, `Activo`, `FechaActivacion`, `FechaDesactivacion?`, `UsuarioActivacionId?`), índice único `(EmpresaId, Modulo)`.
2. Servicio `IModuloService` / `ModuloService`:
   - `EstaActivoAsync(TipoModulo modulo)` para la empresa actual: devuelve `true` solo si la **empresa está `Activa`** y el módulo está activo. Cachea el resultado por circuito y ofrece invalidación.
   - `ActivarAsync` / `DesactivarAsync(empresaId, modulo)`: **solo SuperAdministrador**.
3. En [Components/Pages/Empresas.razor](../../Components/Pages/Empresas.razor), añade a cada empresa una sección "Módulos" con interruptores para activarlos o desactivarlos.
4. En `NavMenu.razor`: si el TPV está activo para la empresa actual, el enlace TPV lleva a `/tpv` (con submenú: Venta, Caja, Tickets, Configuración TPV); si no, mantiene `modulo-no-disponible/TPV`. Oculta el TPV a `Asesoría`.
5. **Seguridad**: no basta con ocultar el menú. Todas las páginas TPV y **todos los métodos de los servicios TPV** deben comprobar en el servidor `EstaActivoAsync(TipoModulo.TPV)` y la pertenencia del registro a la empresa actual. Si el módulo no está activo, redirige a `modulo-no-disponible/TPV`.

# Fase 2 — Modelo de datos TPV

Crea los modelos en `Models/` siguiendo el estilo existente (DataAnnotations, `EmpresaId` + navegación `Empresa`, `decimal` con precisión 18,2):

- **`TpvTerminal`**: caja física/punto de venta. `Nombre`, `Codigo`, `Activo`, `SerieTicket` (opcional, para usar una serie propia por terminal), `ClienteGenericoId?` (cliente "Contado" por defecto), `FormaPagoPorDefectoId?`, `ImpresoraAnchoMm` (58/80).
- **`TpvSesionCaja`**: apertura y cierre de caja. `TerminalId`, `UsuarioAperturaId`, `FechaApertura`, `FondoInicial`, `UsuarioCierreId?`, `FechaCierre?`, `EfectivoContado?`, `EfectivoEsperado`, `Descuadre`, `Estado` (Abierta/Cerrada), `Observaciones`. Solo puede haber una sesión abierta por terminal.
- **`TpvMovimientoCaja`**: entradas y salidas de efectivo que no son ventas (cambio, retiradas, pagos menores). `SesionCajaId`, `Tipo` (Entrada/Salida), `Importe`, `Concepto`, `UsuarioId`, `Fecha`.
- **`TicketTpv`**: `Numero` (serie propia), `Fecha`, `TerminalId`, `SesionCajaId`, `UsuarioId`, `ClienteId?`, `Estado` (`Abierto`, `Aparcado`, `Cobrado`, `Anulado`, `Facturado`), `TipoTicket` (`Venta`, `Devolucion`), `TicketOriginalId?` (para devoluciones), `PresupuestoOrigenId?`, `FacturaId?` (canje a factura completa), `BaseImponible`, `TotalIVA`, `DescuentoGlobal`, `Total`, `Entregado`, `Cambio`, `Notas`.
  - Líneas: **reutiliza `LineaDocumento`** añadiendo `TicketTpvId?` (mismo patrón que Factura y Presupuesto) para conservar `CalcularImportes()` y un desglose de IVA coherente con el resto de la app. Si al revisarlo ves un motivo técnico fuerte para crear `LineaTicketTpv`, justifícalo antes de hacerlo.
- **`PagoTicketTpv`**: pagos mixtos. `TicketTpvId`, `FormaPagoPersonalizadaId` (efectivo, tarjeta, Bizum… usando las formas de pago de la empresa), `Importe`, `EsEfectivo`, `Referencia`.
- **`ConfiguracionEmpresa`**: añade `SerieTicket` (defecto `"T"`), `NumeroTicketActual`, `LongitudNumeroTicket`, `IncluirAnioEnSerieTicket`, `SerieDevolucionTicket` (defecto `"TD"`) con sus contadores, `TpvPermitirStockNegativo` (bool), `TpvDescontarStock` (bool, defecto true), `TpvPreciosIncluyenIVA` (bool, defecto true), `TpvTextoPieTicket`.
- Registra los `DbSet` y las relaciones en `ApplicationDbContext` (`OnDelete Restrict` en relaciones con Facturas, Presupuestos y Productos; nunca en cascada sobre documentos fiscales).

# Fase 3 — Servicios

Crea `Services/TpvService.cs` (y, si crece demasiado, `TpvCajaService`), registrados como scoped en `Program.cs`:

- **Productos**: `BuscarProductoPorCodigoAsync(codigo)` (busca por `CodigoBarras` y después por `Referencia`), `BuscarProductosAsync(texto, categoria)`, `ObtenerCategoriasAsync()`. Solo productos `Activo` de la empresa. Usa proyecciones o `AsNoTracking` para que sea rápido.
- **Caja**: `AbrirCajaAsync`, `CerrarCajaAsync` (calcula el efectivo esperado = fondo + cobros en efectivo − cambios ± movimientos), `RegistrarMovimientoAsync`, `ObtenerSesionAbiertaAsync(terminalId)`, `GenerarInformeZAsync(sesionId)` (totales por forma de pago, por tipo de IVA, número de tickets, devoluciones y descuadre).
- **Tickets**:
  - `CrearTicketAsync`, `AgregarLineaAsync`, `ModificarLineaAsync`, `EliminarLineaAsync`, `AparcarAsync`, `RecuperarAparcadoAsync`.
  - `CobrarAsync(ticketId, pagos)`: en **una única transacción** asigna el número correlativo (incrementando el contador de `ConfiguracionEmpresa` con bloqueo/concurrencia segura, sin huecos), valida que los pagos cubren el total, calcula el cambio, descuenta el stock de los productos de tipo `Producto` (respetando `TpvPermitirStockNegativo`) y marca el ticket como `Cobrado`. Requiere una sesión de caja abierta.
  - `AnularAsync`: solo tickets no cobrados. Un ticket cobrado **no se borra ni se modifica**: se corrige con una devolución.
  - `DevolverAsync(ticketOriginalId, lineas)`: crea un ticket `Devolucion` con importes negativos y la serie de devolución, repone el stock y genera el reembolso. Si el ticket original ya estaba `Facturado`, genera un **abono de venta** (rectificativa) vinculado a la factura mediante el mecanismo existente de abonos.
- **Integración con Presupuestos**: `CargarPresupuestoEnTicketAsync(presupuestoId)`: copia las líneas de un presupuesto `Aceptado` (o `Enviado`, con confirmación) de la empresa al ticket y guarda `PresupuestoOrigenId`. Al cobrar, deja el presupuesto trazado (no cambies su estado si no existe un estado adecuado; propón uno si hace falta).
- **Integración con Facturas**:
  - `ConvertirTicketEnFacturaAsync(ticketId, clienteId)`: canje de factura simplificada por factura completa. Crea una `Factura` de venta con la numeración normal (`GenerarNumeroAsync`), copia las líneas, registra como `PagoFactura` los pagos del ticket (la factura queda `Pagada`), enlaza `FacturaId` y pasa el ticket a `Facturado`. Exige un cliente con NIF.
  - `FacturarTicketsAsync(clienteId, ticketIds)`: agrupa varios tickets cobrados del mismo cliente en una sola factura.
  - Reutiliza `IFacturaService` en lugar de duplicar lógica de numeración, cálculo o pagos.
- **Impresión**: `ITicketPdfService` con QuestPDF en formato de rollo (58/80 mm): datos fiscales de la empresa, número, fecha, terminal, operador, líneas, desglose de IVA por tipo, total, formas de pago, entregado/cambio y texto de pie. Imprime desde el navegador con un endpoint que devuelva el PDF o con HTML + `@media print` y `window.print()` mediante JS interop. Elige la opción más simple que funcione con impresoras térmicas en Windows/IIS.

# Fase 4 — Interfaz (Blazor, Bootstrap, táctil)

Crea las páginas en `Components/Pages/Tpv/` con `AuthGuard` y la comprobación del módulo:

1. **`/tpv`: pantalla de venta** (pantalla completa, optimizada para tableta de 10" o más y para ratón):
   - Selector de terminal (recordado en `localStorage`) y aviso si la caja está cerrada (botón "Abrir caja").
   - Campo de código de barras **siempre enfocado**: el lector actúa como teclado + Enter. Con Enter se añade el producto o se incrementa su cantidad. Admite el prefijo `3*` para indicar cantidad.
   - Cuadrícula de productos por categoría (pestañas), con imagen y precio, y buscador por nombre o referencia.
   - Carrito: cantidad +/−, precio y descuento editables por línea (solo `Administrador` puede cambiar el precio), descuento global, cliente opcional (por defecto el genérico del terminal) y totales en grande.
   - Botones: Cobrar, Aparcar, Recuperar aparcados, Cargar presupuesto, Vaciar.
   - Modal de cobro: botones rápidos de importe (exacto, 5, 10, 20, 50 €), pago mixto entre formas de pago, cálculo del cambio en tiempo real. Tras cobrar, opción de imprimir ticket o convertir a factura.
   - Atajos de teclado (F2 buscar, F4 cobrar, Esc cancelar) mediante JS interop mínimo.
2. **`/tpv/caja`**: apertura y cierre, movimientos de efectivo, arqueo por denominaciones (billetes y monedas) e informe Z imprimible.
3. **`/tpv/tickets`**: histórico filtrable (fecha, terminal, usuario, estado, cliente) con acciones de reimprimir, devolver, convertir a factura y ver la factura vinculada.
4. **`/tpv/configuracion`** (solo `Administrador`): terminales, series de tickets y opciones TPV de `ConfiguracionEmpresa`.
5. **Integración inversa** en las pantallas existentes:
   - En `FacturasVenta.razor`, muestra el origen "TPV – Ticket Nº X" en las facturas que vengan de tickets.
   - En `Presupuestos.razor`, añade la acción "Cobrar en TPV" (solo si el módulo está activo).
   - En `Productos.razor`/`ProductoForm.razor`, no cambies nada salvo que haga falta para el TPV (por ejemplo, validar que `CodigoBarras` sea único por empresa si no lo es ya).
6. Mantén el estilo visual actual (Bootstrap e iconos `bi-*`). Usa CSS aislado (`.razor.css`) para el layout del TPV. No añadas librerías UI nuevas.

# Fase 5 — Base de datos

1. Genera la migración EF `AddModuloTPV` con EF CLI **8.0.11** (hay una herramienta temporal en `/tmp/factiox-ef8-20261006/dotnet-ef`; si no existe, instálala localmente en esa versión). **Revisa las operaciones generadas y elimina cualquier cambio ajeno al TPV**: el snapshot ha omitido históricamente cambios aplicados por SQL.
2. Crea el script idempotente `Database/AddModuloTPV.sql` siguiendo exactamente el patrón de [Database/AddAbonosCompraVenta.sql](../../Database/AddAbonosCompraVenta.sql):
   - Procedimiento temporal con validación de `DATABASE()`, comprobaciones en `information_schema` antes de cada `ALTER TABLE ADD COLUMN`, índice o clave foránea, y `CREATE TABLE IF NOT EXISTS` para las tablas nuevas.
   - **No uses** `ADD COLUMN IF NOT EXISTS` ni `CHAR(36)` en columnas `string` (usa `VARCHAR(36)`).
   - Inserta el registro en `__EFMigrationsHistory` con el `MigrationId` real y `ProductVersion` `8.0.11`.
   - Opcionalmente, crea un terminal por defecto ("Caja 1") para las empresas que tengan el módulo activo.
3. Añade la entidad `EmpresaModulo` al mismo script (o a uno previo `AddModulosEmpresa.sql` si lo separas en otra migración).

# Fase 6 — Verificación

- Compila con: `dotnet build FactioX.csproj --no-restore --verbosity quiet -clp:ErrorsOnly\;Summary`.
- Aplica la migración en local y arranca con: `ASPNETCORE_ENVIRONMENT=Development dotnet run --no-build --no-launch-profile --urls http://localhost:5188`.
- Comprueba manualmente (o con el navegador integrado si está disponible):
  1. Empresa sin módulo: el TPV lleva a "módulo no disponible" y `/tpv` por URL directa también queda bloqueado.
  2. SuperAdministrador activa el TPV en una empresa; desactivar la empresa (`Activa = false`) bloquea el TPV.
  3. Abrir caja → escanear un código → cobro mixto efectivo + tarjeta → número de ticket correlativo → stock descontado → ticket impreso.
  4. Aparcar y recuperar un ticket.
  5. Cargar un presupuesto aceptado en el ticket y cobrarlo.
  6. Convertir un ticket en factura: la factura aparece en Facturas de venta como pagada y con el origen TPV.
  7. Devolver un ticket facturado: se crea el abono de venta vinculado y se repone el stock.
  8. Cierre de caja con informe Z y descuadre correcto.
  9. Aislamiento: un usuario de la empresa A no puede ver ni operar terminales o tickets de la empresa B (prueba manipulando IDs en la URL).
- Crea `TPV_README.md` con el mismo formato que `ABONOS_README.md`: funcionalidad, activación, script SQL y flujo de uso.

# Restricciones

- No modifiques el comportamiento existente de Facturas, Presupuestos ni Abonos salvo en los puntos de integración descritos.
- No dupliques la lógica de numeración, de cálculo de importes ni de pagos: reutiliza los servicios existentes y extráela a métodos compartidos solo si es imprescindible.
- Toda operación que afecte a numeración, stock o pagos debe ser transaccional.
- No publiques en IIS ni ejecutes `publicar_iis.sh`.
- Si alguna decisión de diseño no está clara tras revisar el código (por ejemplo, reutilizar `LineaDocumento` o el estado del presupuesto tras cobrarlo), pregunta antes de implementar esa parte.
