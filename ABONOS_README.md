# Abonos de venta y compra

## Creacion

- Finanzas > Abonos Venta o Abonos Compra > Nuevo Abono permite introducir un abono manual.
- En un nuevo abono, seleccionar el cliente (venta) o proveedor (compra) y elegir Factura a abonar. El selector ofrece sus facturas originales de la empresa activa, incluyendo anios anteriores, con numero, fecha e importe.
- Al elegir una factura se cargan sus lineas, concepto, impuestos, descuentos, cargos y forma de pago. Se mantienen el motivo, la fecha de emision y el estado elegidos para el nuevo abono. La factura original no se modifica.
- Mientras el abono es nuevo se puede elegir otra factura o cambiar de cliente/proveedor. Cambiar de tercero limpia la referencia y los datos copiados; elegir Abono manual desvincula la factura y permite conservar los importes para editarlos manualmente. En abonos vinculados ya guardados el tercero permanece bloqueado.
- En una factura original, la accion de rectificar (flecha de retorno a la izquierda en venta y a la derecha en compra) crea un borrador de abono con sus lineas y la referencia original. No cambia ni cancela la factura original.
- El borrador no se guarda hasta pulsar Guardar. Se pueden eliminar o modificar lineas para una devolucion parcial.
- El motivo de rectificacion es obligatorio. En un abono manual puede indicarse una referencia externa en Factura original.
- Los importes de los abonos son negativos. La aplicacion normaliza cantidades, descuentos generales, recargos, cargos y retenciones al calcular y guardar.
- La empresa y el cliente o proveedor de un abono vinculado deben coincidir con los de la original. No se puede rectificar otro abono ni eliminar una factura con abonos vinculados.

## Series por empresa

En Configuracion > Informacion de la Empresa estan las secciones Numeracion de Abonos de Venta y Compra.

- Series iniciales: AV y AC, independientes de las series de facturas.
- Ultimo numero utilizado: inicialmente 0; el primer abono usa 0001.
- Longitud: entre 3 y 8 digitos. Inclusion del anio configurable.
- Los prefijos deben ser distintos de las otras series de facturas y abonos de esa empresa.
- Las reservas de numero se serializan con un bloqueo de la configuracion. Puede quedar un salto si falla el guardado despues de reservar el numero; el contador no se retrocede.

## Consulta y documentos

Los abonos se almacenan como Factura con ClaseFactura Rectificativa y TipoFactura Venta o Compra. Tienen listados separados; sus valores negativos se incluyen en los totales netos del dashboard existente. Las rectificativas anteriores, si las hay, aparecen en estos listados sin renumerarlas.

Asesoria tiene acceso de solo lectura, con los mismos filtros de empresa, anio y periodo, exportacion a Excel y consulta de adjuntos. Los PDFs identifican el abono, su factura original y el motivo.

La exportacion FacturaE de abonos esta bloqueada: el generador existente no implementa los metadatos fiscales obligatorios de rectificacion. Esta funcionalidad no incorpora envio fiscal ni certifica el cumplimiento de FacturaE o VeriFactu. Tampoco incorpora compensacion automatica de pagos o devolucion de existencias.

## Actualizacion de la base de datos

El script de produccion es `Database/AddAbonosCompraVenta.sql`. Corresponde a la migracion `20261006145150_AddAbonosCompraVenta`: agrega once columnas, un indice y una relacion protegida hacia la factura original. No crea facturas de ejemplo ni abonos ficticios.

1. Hacer copia de seguridad y detener la aplicacion durante la actualizacion.
2. Abrir una conexion a produccion en MySQL Workbench u otro cliente compatible con DELIMITER y seleccionar la base de FactioX como esquema activo. El script no fija el nombre de la base de datos.
3. Ejecutar el archivo completo con una cuenta que tenga permisos de lectura, ALTER, INDEX, REFERENCES, CREATE, CREATE ROUTINE, ALTER ROUTINE, EXECUTE e INSERT en esa base.
4. Comprobar el resultado final y las series mostradas por empresa antes de iniciar los nuevos binarios.

El script comprueba que existan Facturas y ConfiguracionEmpresa y agrega solamente las columnas, indice y relacion que falten. Las columnas nuevas de configuracion inicializan las empresas existentes con AV/AC, contador 0, cuatro digitos e inclusion del anio. Si ya existen esas columnas, conserva sus valores: no reinicia series ni contadores. Si un prefijo coincide con una serie anterior, cambiarlo en la configuracion de empresa antes de emitir abonos.

Puede volver a ejecutarse para completar una actualizacion parcial, incluso cuando la migracion ya figure aplicada mediante EF. Crea __EFMigrationsHistory si falta y registra esta migracion sin duplicar su fila; no inventa el historial de migraciones anteriores. No suprime errores de permisos, datos o relaciones. En caso de fallo, corregir la causa y repetir el script completo. MySQL hace commits implicitos con DDL, por lo que una transaccion no garantiza revertir los cambios de esquema ya realizados.

Validado en una base MySQL temporal: primera instalacion, segunda ejecucion con valores personalizados y esquema parcialmente actualizado. No se ha ejecutado automaticamente en produccion.

El snapshot actualizado incluye campos historicos que el proyecto ya mantenia mediante scripts SQL. Esta migracion no crea ni elimina Gastos, adjuntos ni la configuracion anterior de compras.