# Script de Datos de Prueba - Empresa Test 1

## Descripción
Este script SQL crea un conjunto completo de datos de prueba para la aplicación FactioX, incluyendo una empresa llamada "Empresa Test 1" con todos sus datos relacionados.

## Datos creados

### 1. **Empresa Test 1**
- **NIF**: B12345678
- **Dirección**: Calle Principal 123, Madrid
- **Email**: info@empresatest1.com
- **Web**: www.empresatest1.com

### 2. **Usuario Administrador**
- **Email**: admin@empresatest1.com
- **Password**: Test1234
- **Rol**: Administrador

### 3. **4 Clientes**
- Cliente Premium S.L. (Madrid)
- Distribuciones López (Barcelona)
- Comercial García e Hijos (Valencia)
- TechSolutions Barcelona

### 4. **3 Proveedores**
- Suministros Industriales S.A.
- Tecnología y Componentes S.L.
- Distribuidora Nacional

### 5. **8 Productos/Servicios**
- Servicio de Consultoría IT (85€/hora)
- Licencia Software Gestión Empresarial (1.200€)
- Servidor Dell PowerEdge R740 (3.500€)
- Router Cisco RV340 (425€)
- Switch TP-Link 24 puertos (95€)
- Mantenimiento mensual Hardware (250€)
- Portátil HP EliteBook 850 G8 (1.450€)
- Monitor LG 27" 4K (380€)

### 6. **3 Presupuestos**
- PRE-2026-0001: Enviado (Cliente Premium) - 11.386,10€
- PRE-2026-0002: Aceptado (Distribuciones López) - 21.035,85€
- PRE-2026-0003: Borrador (Comercial García) - 10.164,00€

### 7. **3 Facturas de Venta**
- FAC-2026-0001: Pagada - 21.035,85€ (del presupuesto PRE-2026-0002)
- FAC-2026-0002: Emitida - 6.171,00€
- FAC-2026-0003: Enviada - 5.142,50€

### 8. **3 Facturas de Compra**
- FC-2026-0001: Pagada - 8.470,00€
- FC-2026-0002: Emitida - 17.545,00€
- FC-2026-0003: Enviada - 7.132,95€

## Cómo ejecutar el script

### Opción 1: Desde línea de comandos
```bash
mysql -u root -p factiox < Database/DatosPruebaEmpresaTest.sql
```

### Opción 2: Desde MySQL Workbench
1. Abre MySQL Workbench
2. Conecta a tu servidor MySQL
3. Abre el archivo `DatosPruebaEmpresaTest.sql`
4. Ejecuta el script completo

### Opción 3: Desde phpMyAdmin
1. Accede a phpMyAdmin
2. Selecciona la base de datos `factiox`
3. Ve a la pestaña "SQL"
4. Pega el contenido del archivo y ejecuta

## Verificación

Después de ejecutar el script, verás un resumen con:
- ID de la empresa creada
- Total de clientes: 4
- Total de proveedores: 3
- Total de productos: 8
- Total de presupuestos: 3
- Facturas de venta: 3
- Facturas de compra: 3

## Login en la aplicación

Puedes iniciar sesión con:
- **Email**: admin@empresatest1.com
- **Contraseña**: Test1234

## Notas importantes

- El script utiliza variables MySQL (`@empresa_id`, etc.) para mantener las relaciones entre entidades
- Todas las líneas de documentos tienen sus importes calculados correctamente
- Las fechas están configuradas para febrero 2026
- Los stocks de productos están inicializados con valores realistas
- El presupuesto PRE-2026-0002 está vinculado a la factura FAC-2026-0001

## Eliminar datos de prueba

Si necesitas eliminar estos datos de prueba:

```sql
-- Primero obtén el ID de la empresa
SET @test_empresa_id = (SELECT Id FROM empresas WHERE NIF = 'B12345678');

-- Eliminar en orden por dependencias
DELETE FROM lineasdocumento WHERE FacturaId IN (SELECT Id FROM facturas WHERE EmpresaId = @test_empresa_id);
DELETE FROM lineasdocumento WHERE PresupuestoId IN (SELECT Id FROM presupuestos WHERE EmpresaId = @test_empresa_id);
DELETE FROM facturas WHERE EmpresaId = @test_empresa_id;
DELETE FROM presupuestos WHERE EmpresaId = @test_empresa_id;
DELETE FROM productos WHERE EmpresaId = @test_empresa_id;
DELETE FROM proveedores WHERE EmpresaId = @test_empresa_id;
DELETE FROM clientes WHERE EmpresaId = @test_empresa_id;
DELETE FROM usuarios WHERE EmpresaId = @test_empresa_id;
DELETE FROM configuracionempresas WHERE EmpresaId = @test_empresa_id;
DELETE FROM empresas WHERE Id = @test_empresa_id;
```
