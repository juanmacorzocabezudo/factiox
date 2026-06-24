#!/usr/bin/env python3
import os

# Leer el archivo base de FacturasVenta
facturas_venta_path = '/Users/juanmariacorzo/Documents/FactioX/Components/Pages/FacturasVenta.razor'
with open(facturas_venta_path, 'r', encoding='utf-8') as f:
    contenido_venta = f.read()

# Generar FacturasCompra.razor
contenido_compra = contenido_venta.replace('@page "/facturas-venta"', '@page "/facturas-compra"')
contenido_compra = contenido_compra.replace('IClienteService ClienteService', 'IProveedorService ProveedorService')
contenido_compra = contenido_compra.replace('<PageTitle>Facturas de Venta - FactioX</PageTitle>', '<PageTitle>Facturas de Compra - Fact ioX</PageTitle>')
contenido_compra = contenido_compra.replace('<h2><i class="bi bi-receipt"></i> Facturas de Venta</h2>', '<h2><i class="bi bi-receipt-cutoff"></i> Facturas de Compra</h2>')
contenido_compra = contenido_compra.replace('Nueva Factura de Venta', 'Nueva Factura de Compra')
contenido_compra = contenido_compra.replace('Facturas de Venta', 'Facturas de Compra')
contenido_compra = contenido_compra.replace('background-color: #003366', 'background-color: #d32f2f')
contenido_compra = contenido_compra.replace('bg-primary text-white', 'style="background-color: #d32f2f; color: white;"')
contenido_compra = contenido_compra.replace('class="btn btn-primary', 'class="btn btn-danger')
contenido_compra = contenido_compra.replace('style="background-color: #003366; color: white;"', 'style="background-color: #d32f2f; color: white;"')
contenido_compra = contenido_compra.replace('buscar por número o cliente', 'buscar por número o proveedor')
contenido_compra = contenido_compra.replace('Cliente', 'Proveedor')
contenido_compra = contenido_compra.replace('@factura.Proveedor?.Nombre', '@factura.Proveedor?.Nombre')
contenido_compra = contenido_compra.replace('TipoFactura.Venta', 'TipoFactura.Compra')
contenido_compra = contenido_compra.replace('ProveedorId', 'ProveedorId')
contenido_compra = contenido_compra.replace('List<Proveedor>? clientes', 'List<Proveedor>? proveedores')
contenido_compra = contenido_compra.replace('clientes = await ProveedorService.ObtenerTodosAsync()', 'proveedores = await ProveedorService.ObtenerTodosAsync()')
contenido_compra = contenido_compra.replace('id="cliente"', 'id="proveedor"')
contenido_compra = contenido_compra.replace('for="cliente"', 'for="proveedor"')
contenido_compra = contenido_compra.replace('Seleccione un cliente', 'Seleccione un proveedor')
contenido_compra = contenido_compra.replace('@foreach (var cliente in clientes)', '@foreach (var proveedor in proveedores)')
contenido_compra = contenido_compra.replace('<option value="@cliente.Id">@cliente.Nombre</option>', '<option value="@proveedor.Id">@proveedor.Nombre</option>')
contenido_compra = contenido_compra.replace('(f.Proveedor?.Nombre?.Contains', '(f.Proveedor?.Nombre?.Contains')
contenido_compra = contenido_compra.replace('if (clientes != null)', 'if (proveedores != null)')
contenido_compra = contenido_compra.replace('@bind-Value="facturaEditando.ProveedorId"', '@bind-Value="facturaEditando.ProveedorId"')
contenido_compra = contenido_compra.replace('Debe seleccionar un cliente', 'Debe seleccionar un proveedor')
contenido_compra = contenido_compra.replace('bg-success text-white', 'style="color: #d32f2f;"')
contenido_compra = contenido_compra.replace('.text-success', '.fs-4" style="color: #d32f2f;"')

# Guardar FacturasCompra.razor
facturas_compra_path = '/Users/juanmariacorzo/Documents/FactioX/Components/Pages/FacturasCompra.razor'
with open(facturas_compra_path, 'w', encoding='utf-8') as f:
    f.write(contenido_compra)

print("FacturasCompra.razor actualizado correctamente")
