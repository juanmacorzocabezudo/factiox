#!/usr/bin/env python3
import os

# Leer el archivo base de FacturasVenta
facturas_venta_path = '/Users/juanmariacorzo/Documents/FactioX/Components/Pages/FacturasVenta.razor'
with open(facturas_venta_path, 'r', encoding='utf-8') as f:
    contenido_venta = f.read()

# Generar Presupuestos.razor basándose en FacturasVenta
contenido_presup = contenido_venta.replace('@page "/facturas-venta"', '@page "/presupuestos"')
contenido_presup = contenido_presup.replace('@inject IFacturaService FacturaService', '@inject IPresupuestoService PresupuestoService')
contenido_presup = contenido_presup.replace('<PageTitle>Facturas de Venta - FactioX</PageTitle>', '<PageTitle>Presupuestos - FactioX</PageTitle>')
contenido_presup = contenido_presup.replace('<h2><i class="bi bi-receipt"></i> Facturas de Venta</h2>', '<h2><i class="bi bi-file-earmark-text"></i> Presupuestos</h2>')
contenido_presup = contenido_presup.replace('Nueva Factura de Venta', 'Nuevo Presupuesto')
contenido_presup = contenido_presup.replace('Facturas de Venta', 'Presupuestos')
contenido_presup = contenido_presup.replace('background-color: #003366', 'background-color: #f57c00')
contenido_presup = contenido_presup.replace('bg-primary text-white', 'style="background-color: #f57c00; color: white;"')
contenido_presup = contenido_presup.replace('List<Factura>?', 'List<Presupuesto>?')
contenido_presup = contenido_presup.replace('new Factura', 'new Presupuesto')
contenido_presup = contenido_presup.replace('Factura factura', 'Presupuesto presupuesto')
contenido_presup = contenido_presup.replace('@factura.', '@presupuesto.')
contenido_presup = contenido_presup.replace('facturaEditando', 'presupuestoEditando')
contenido_presup = contenido_presup.replace('TipoFactura.Venta', 'TipoPresupuesto.Venta')  # Ajustar seg sea necesario
contenido_presup = contenido_presup.replace('EstadoFactura', 'EstadoPresupuesto')
contenido_presup = contenido_presup.replace('@foreach (var factura in', '@foreach (var presupuesto in')
contenido_presup = contenido_presup.replace('PresupuestoService.Obtener', 'PresupuestoService.Obtener')
contenido_presup = contenido_presup.replace('await FacturaService', 'await PresupuestoService')
contenido_presup = contenido_presup.replace('presupuestos = todasLasFacturas', 'presupuestos = todosLosPresupuestos')
contenido_presup = contenido_presup.replace('IEnumerable<Presupuesto> FacturasFiltradas', 'IEnumerable<Presupuesto> PresupuestosFiltrados')
contenido_presup = contenido_presup.replace('presupuestos.Any()', 'presupuestos.Any()')
contenido_presup = contenido_presup.replace('facturas == null', 'presupuestos == null')
contenido_presup = contenido_presup.replace('facturas.', 'presupuestos.')
contenido_presup = contenido_presup.replace('Numero', 'Numero')  # Los presupuestos pueden tener "Numero" en lugar de "NumeroFactura"
contenido_presup = contenido_presup.replace('f.TipoFactura == TipoFactura.Venta', '')  # Eliminar filtrado por tipo
contenido_presup = contenido_presup.replace('factura de', 'presupuesto de')
contenido_presup = contenido_presup.replace('la factura', 'el presupuesto')

# Campos específicos de presupuesto
contenido_presup = contenido_presup.replace('FechaEmision', 'Fecha')
contenido_presup = contenido_presup.replace('NumeroFactura', 'Numero')

# Guardar Presupuestos.razor
presupuestos_path = '/Users/juanmariacorzo/Documents/FactioX/Components/Pages/Presupuestos.razor'
with open(presupuestos_path, 'w', encoding='utf-8') as f:
    f.write(contenido_presup)

print("Presupuestos.razor actualizado correctamente")
