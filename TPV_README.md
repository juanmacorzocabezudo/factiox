# Módulo TPV

Terminal punto de venta integrado con Productos, Clientes, Presupuestos y Facturas. Se activa por empresa como módulo adicional.

## Activación

1. Aplicar la base de datos: migración EF `20261013150423_AddModuloTPV` o el script idempotente `Database/AddModuloTPV.sql`, que se puede repetir sin errores.
2. Como **SuperAdministrador**, ir a **Empresas** y activar el interruptor **TPV** de la empresa. Solo funciona si la empresa está **Activa**.
3. Como **Administrador** de la empresa, abrir **TPV → Configuración TPV**:
   - Crear al menos un terminal (caja), con cliente genérico y forma de pago por defecto opcionales. Se puede asignar una serie propia al terminal; si se deja vacía, usa la serie general.
   - Revisar las series `T` (tickets) y `TD` (devoluciones), si se muestran precios con IVA incluido, las opciones de stock y el texto al pie del ticket.

Si el módulo no está activo, el menú muestra "Módulos disponibles → TPV" y lleva a la página informativa. Todas las páginas y métodos de `TpvService` comprueban en el servidor el módulo y la empresa.

## Pantallas

| Ruta | Uso |
|------|-----|
| `/tpv` | Venta táctil: lector de códigos (`3*código` para indicar cantidad), categorías, carrito, cobro mixto con cambio, aparcar/recuperar tickets, cargar presupuestos. Atajos: F2 buscar, F4 cobrar, F8 aparcar, Esc cerrar. |
| `/tpv/caja` | Apertura con fondo, entradas y salidas de efectivo, informe X, arqueo por billetes y monedas, cierre con informe Z. |
| `/tpv/tickets` | Histórico filtrable: reimprimir, devolver, anular aparcados, canjear uno o varios tickets por una factura completa. |
| `/tpv/configuracion` | Terminales, series y opciones (solo Administrador). |

## Reglas de negocio

- **Tickets como facturas simplificadas.** El número se asigna al cobrar, dentro de una transacción con bloqueo de `ConfiguracionEmpresa`, para que la serie sea correlativa y sin huecos. Los tickets aparcados o anulados no consumen número.
- **Tickets cobrados.** No se modifican ni se borran: se corrigen con una devolución (serie `TD`, importes negativos), que repone el stock.
- **Stock.** Se descuenta al cobrar solo en productos de tipo `Producto` con `Stock` informado. Si no se permite stock negativo, el cobro falla con "Stock insuficiente".
- **Precios.** Los usuarios sin rol Administrador no pueden cambiar precios ni añadir líneas libres; el servidor aplica el precio y el IVA de la ficha del producto.
- **Series por terminal.** Cada serie propia usa su contador independiente; los tickets de terminales sin serie propia comparten el contador general. Las devoluciones siempre usan la serie `TD`.
- **Presupuestos.** Se pueden cargar los aceptados o enviados, desde el TPV o desde el botón "Cobrar en TPV" del listado de presupuestos. Se les aplica el IVA del producto, porque los presupuestos se guardan sin IVA. Cada presupuesto solo se puede cobrar una vez.
- **Canje por factura.** Crea una factura de venta con la numeración normal, marcada como Pagada, con los pagos del ticket. El ticket pasa a `Facturado`. En Facturas de venta se ve el origen "TPV – Ticket Nº …".
- **Devolución de un ticket facturado.** Genera automáticamente un abono de venta (AV) vinculado a la factura original.
- **Huella.** Cada ticket guarda una huella SHA-256 encadenada con el anterior, preparada para Verifactu. No se envía nada a la AEAT.

## Impresión

Los tickets y los informes X/Z se generan con QuestPDF en rollo de 58 u 80 mm (según el terminal) y se imprimen desde el navegador. Para no tener que confirmar el diálogo de impresión en cada ticket, se recomienda configurar la impresora térmica como predeterminada y, en Chrome o Edge, usar el modo `--kiosk-printing`.

## Limitaciones conocidas

- Los precios se guardan sin IVA con 2 decimales, igual que en el resto de la aplicación. Al editar un PVP, el importe con IVA puede variar en ±0,01 €.
- La opción de IVA incluido controla cómo se muestran e introducen los precios en la venta; los importes siguen guardándose sin IVA.
- La detección de efectivo (cambio y arqueo) se basa en que el nombre de la forma de pago contenga "Efectivo" o "Contado".
