using FactioX.Data;
using FactioX.Models;
using Microsoft.EntityFrameworkCore;

namespace FactioX.Services
{
    public interface IDatosPruebaService
    {
        Task GenerarDatosPruebaAsync(int empresaId);
    }

    public class DatosPruebaService : IDatosPruebaService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

        public DatosPruebaService(IDbContextFactory<ApplicationDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task GenerarDatosPruebaAsync(int empresaId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            
            // 1. Crear configuración de empresa
            await CrearConfiguracionEmpresa(context, empresaId);
            
            // 2. Crear clientes de prueba
            var clientes = await CrearClientes(context, empresaId);
            
            // 3. Crear proveedores de prueba
            var proveedores = await CrearProveedores(context, empresaId);
            
            // 4. Crear productos de prueba
            var productos = await CrearProductos(context, empresaId);
            
            // 5. Crear formas de pago personalizadas
            await CrearFormasPago(context, empresaId);
            
            // 6. Crear presupuestos de prueba
            var presupuestos = await CrearPresupuestos(context, empresaId, clientes, productos);
            
            // 7. Crear facturas de venta de prueba
            await CrearFacturasVenta(context, empresaId, clientes, productos);
            
            // 8. Crear facturas de compra de prueba
            await CrearFacturasCompra(context, empresaId, proveedores, productos);
            
            await context.SaveChangesAsync();
        }

        private async Task CrearConfiguracionEmpresa(ApplicationDbContext context, int empresaId)
        {
            var configuracion = new ConfiguracionEmpresa
            {
                EmpresaId = empresaId,
                NombreEmpresa = "Empresa Demo",
                NIF = "B12345678",
                Direccion = "Calle Principal, 123",
                Ciudad = "Madrid",
                CodigoPostal = "28001",
                Provincia = "Madrid",
                Pais = "ESP",
                Telefono = "912345678",
                Email = "info@empresademo.com",
                Web = "www.empresademo.com",
                NombreComercial = "EmpresaDEMO",
                PersonaContacto = "Juan Pérez"
            };
            
            context.ConfiguracionEmpresa.Add(configuracion);
            await context.SaveChangesAsync();
        }

        private async Task<List<Cliente>> CrearClientes(ApplicationDbContext context, int empresaId)
        {
            var clientes = new List<Cliente>
            {
                new Cliente
                {
                    EmpresaId = empresaId,
                    Tipo = TipoCliente.Particular,
                    Nombre = "Carlos",
                    Apellidos = "González Martínez",
                    NIF = "12345678A",
                    Email = "carlos.gonzalez@example.com",
                    Telefono = "666111222",
                    Direccion = "Calle Mayor, 45",
                    Ciudad = "Madrid",
                    CodigoPostal = "28013",
                    Provincia = "Madrid",
                    Pais = "ESP",
                    Activo = true
                },
                new Cliente
                {
                    EmpresaId = empresaId,
                    Tipo = TipoCliente.Empresa,
                    NombreEmpresa = "Tecnología Avanzada S.L.",
                    NIF = "B87654321",
                    Email = "contacto@tecavanzada.com",
                    Telefono = "915551234",
                    Direccion = "Avenida Innovación, 100",
                    Ciudad = "Barcelona",
                    CodigoPostal = "08019",
                    Provincia = "Barcelona",
                    Pais = "ESP",
                    Activo = true
                },
                new Cliente
                {
                    EmpresaId = empresaId,
                    Tipo = TipoCliente.Particular,
                    Nombre = "María",
                    Apellidos = "López Fernández",
                    NIF = "23456789B",
                    Email = "maria.lopez@example.com",
                    Telefono = "677222333",
                    Direccion = "Plaza España, 8",
                    Ciudad = "Valencia",
                    CodigoPostal = "46001",
                    Provincia = "Valencia",
                    Pais = "ESP",
                    Activo = true
                },
                new Cliente
                {
                    EmpresaId = empresaId,
                    Tipo = TipoCliente.Empresa,
                    NombreEmpresa = "Servicios Profesionales BCN",
                    NIF = "B98765432",
                    Email = "info@servprof.com",
                    Telefono = "933334455",
                    Direccion = "Paseo Gracia, 56",
                    Ciudad = "Barcelona",
                    CodigoPostal = "08007",
                    Provincia = "Barcelona",
                    Pais = "ESP",
                    Activo = true
                },
                new Cliente
                {
                    EmpresaId = empresaId,
                    Tipo = TipoCliente.Particular,
                    Nombre = "Ana",
                    Apellidos = "Ruiz Sánchez",
                    NIF = "34567890C",
                    Email = "ana.ruiz@example.com",
                    Telefono = "688333444",
                    Direccion = "Calle Sol, 22",
                    Ciudad = "Sevilla",
                    CodigoPostal = "41001",
                    Provincia = "Sevilla",
                    Pais = "ESP",
                    Activo = true
                }
            };

            context.Clientes.AddRange(clientes);
            await context.SaveChangesAsync();
            return clientes;
        }

        private async Task<List<Proveedor>> CrearProveedores(ApplicationDbContext context, int empresaId)
        {
            var proveedores = new List<Proveedor>
            {
                new Proveedor
                {
                    EmpresaId = empresaId,
                    Nombre = "Suministros Oficina S.A.",
                    NIF = "A11223344",
                    Email = "ventas@suministros.com",
                    Telefono = "914445566",
                    Direccion = "Polígono Industrial, 1",
                    Ciudad = "Madrid",
                    CodigoPostal = "28850",
                    Provincia = "Madrid",
                    Pais = "ESP",
                    FormaPago = FormaPago.Transferencia,
                    Activo = true
                },
                new Proveedor
                {
                    EmpresaId = empresaId,
                    Nombre = "Tecnología Digital S.L.",
                    NIF = "B22334455",
                    Email = "info@tecnodigital.com",
                    Telefono = "935556677",
                    Direccion = "Calle Tecnológica, 50",
                    Ciudad = "Barcelona",
                    CodigoPostal = "08015",
                    Provincia = "Barcelona",
                    Pais = "ESP",
                    FormaPago = FormaPago.Efectivo,
                    Activo = true
                },
                new Proveedor
                {
                    EmpresaId = empresaId,
                    Nombre = "Logística Express S.A.",
                    NIF = "A33445566",
                    Email = "clientes@logexpress.com",
                    Telefono = "963334455",
                    Direccion = "Avenida Transporte, 200",
                    Ciudad = "Valencia",
                    CodigoPostal = "46020",
                    Provincia = "Valencia",
                    Pais = "ESP",
                    FormaPago = FormaPago.Transferencia,
                    Activo = true
                }
            };

            context.Proveedores.AddRange(proveedores);
            await context.SaveChangesAsync();
            return proveedores;
        }

        private async Task<List<Producto>> CrearProductos(ApplicationDbContext context, int empresaId)
        {
            var productos = new List<Producto>
            {
                new Producto
                {
                    EmpresaId = empresaId,
                    Nombre = "Servicio de Consultoría",
                    Descripcion = "Consultoría profesional por hora",
                    PrecioUnitario = 75.00m,
                    IVA = 21,
                    Stock = 0,
                    StockMinimo = 0,
                    Activo = true,
                    Tipo = TipoProducto.Servicio
                },
                new Producto
                {
                    EmpresaId = empresaId,
                    Nombre = "Software de Gestión Premium",
                    Descripcion = "Licencia anual software gestión empresarial",
                    PrecioUnitario = 299.00m,
                    IVA = 21,
                    Stock = 50,
                    StockMinimo = 10,
                    Activo = true,
                    Tipo = TipoProducto.Producto
                },
                new Producto
                {
                    EmpresaId = empresaId,
                    Nombre = "Mantenimiento Web",
                    Descripcion = "Servicio mensual de mantenimiento web",
                    PrecioUnitario = 120.00m,
                    IVA = 21,
                    Stock = 0,
                    StockMinimo = 0,
                    Activo = true,
                    Tipo = TipoProducto.Servicio
                },
                new Producto
                {
                    EmpresaId = empresaId,
                    Nombre = "Equipo Informático HP",
                    Descripcion = "Ordenador de sobremesa HP con monitor incluido",
                    PrecioUnitario = 850.00m,
                    IVA = 21,
                    Stock = 15,
                    StockMinimo = 5,
                    Activo = true,
                    Tipo = TipoProducto.Producto
                },
                new Producto
                {
                    EmpresaId = empresaId,
                    Nombre = "Curso de Formación Online",
                    Descripcion = "Curso completo de formación digital",
                    PrecioUnitario = 199.00m,
                    IVA = 21,
                    Stock = 100,
                    StockMinimo = 20,
                    Activo = true,
                    Tipo = TipoProducto.Servicio
                },
                new Producto
                {
                    EmpresaId = empresaId,
                    Nombre = "Impresora Multifunción",
                    Descripcion = "Impresora laser multifunción color",
                    PrecioUnitario = 425.00m,
                    IVA = 21,
                    Stock = 8,
                    StockMinimo = 3,
                    Activo = true,
                    Tipo = TipoProducto.Producto
                },
                new Producto
                {
                    EmpresaId = empresaId,
                    Nombre = "Desarrollo Web Personalizado",
                    Descripcion = "Diseño y desarrollo de sitio web corporativo",
                    PrecioUnitario = 1500.00m,
                    IVA = 21,
                    Stock = 0,
                    StockMinimo = 0,
                    Activo = true,
                    Tipo = TipoProducto.Servicio
                },
                new Producto
                {
                    EmpresaId = empresaId,
                    Nombre = "Pack Material Oficina",
                    Descripcion = "Pack completo material oficina (100 uds)",
                    PrecioUnitario = 89.00m,
                    IVA = 21,
                    Stock = 25,
                    StockMinimo = 10,
                    Activo = true,
                    Tipo = TipoProducto.Producto
                }
            };

            context.Productos.AddRange(productos);
            await context.SaveChangesAsync();
            return productos;
        }

        private async Task CrearFormasPago(ApplicationDbContext context, int empresaId)
        {
            var formasPago = new List<FormaPagoPersonalizada>
            {
                new FormaPagoPersonalizada
                {
                    EmpresaId = empresaId,
                    Nombre = "Bizum",
                    Descripcion = "Pago instantáneo por Bizum"
                },
                new FormaPagoPersonalizada
                {
                    EmpresaId = empresaId,
                    Nombre = "PayPal",
                    Descripcion = "Pago a través de PayPal"
                },
                new FormaPagoPersonalizada
                {
                    EmpresaId = empresaId,
                    Nombre = "Stripe",
                    Descripcion = "Pago con tarjeta vía Stripe"
                }
            };

            context.FormasPagoPersonalizadas.AddRange(formasPago);
            await context.SaveChangesAsync();
        }

        private async Task<List<Presupuesto>> CrearPresupuestos(ApplicationDbContext context, int empresaId, 
            List<Cliente> clientes, List<Producto> productos)
        {
            var presupuestos = new List<Presupuesto>();
            var random = new Random();
            
            // Presupuesto 1 - Pendiente
            var presup1 = new Presupuesto
            {
                EmpresaId = empresaId,
                ClienteId = clientes[0].Id,
                Numero = "PRE-2026-001",
                Fecha = DateTime.Now.AddDays(-10),
                FechaValidez = DateTime.Now.AddDays(20),
                Estado = EstadoPresupuesto.Enviado,
                Notas = "Presupuesto para proyecto de consultoría inicial"
            };
            presup1.Lineas = new List<LineaDocumento>
            {
                new LineaDocumento 
                { 
                    ProductoId = productos[0].Id,
                    Descripcion = productos[0].Nombre,
                    Cantidad = 20,
                    PrecioUnitario = productos[0].PrecioUnitario,
                    IVA = productos[0].IVA
                },
                new LineaDocumento 
                { 
                    ProductoId = productos[2].Id,
                    Descripcion = productos[2].Nombre,
                    Cantidad = 3,
                    PrecioUnitario = productos[2].PrecioUnitario,
                    IVA = productos[2].IVA
                }
            };
            presupuestos.Add(presup1);

            // Presupuesto 2 - Aceptado
            var presup2 = new Presupuesto
            {
                EmpresaId = empresaId,
                ClienteId = clientes[1].Id,
                Numero = "PRE-2026-002",
                Fecha = DateTime.Now.AddDays(-5),
                FechaValidez = DateTime.Now.AddDays(25),
                Estado = EstadoPresupuesto.Aceptado,
                Notas = "Presupuesto aceptado - Proceder con la instalación"
            };
            presup2.Lineas = new List<LineaDocumento>
            {
                new LineaDocumento 
                { 
                    ProductoId = productos[1].Id,
                    Descripcion = productos[1].Nombre,
                    Cantidad = 10,
                    PrecioUnitario = productos[1].PrecioUnitario,
                    IVA = productos[1].IVA
                }
            };
            presupuestos.Add(presup2);

            // Presupuesto 3 - Enviado
            var presup3 = new Presupuesto
            {
                EmpresaId = empresaId,
                ClienteId = clientes[2].Id,
                Numero = "PRE-2026-003",
                Fecha = DateTime.Now.AddDays(-5),
                FechaValidez = DateTime.Now.AddDays(25),
                Estado = EstadoPresupuesto.Enviado,
                Notas = "Presupuesto desarrollo web completo"
            };
            presup3.Lineas = new List<LineaDocumento>
            {
                new LineaDocumento 
                { 
                    ProductoId = productos[6].Id,
                    Descripcion = productos[6].Nombre,
                    Cantidad = 1,
                    PrecioUnitario = productos[6].PrecioUnitario,
                    IVA = productos[6].IVA
                },
                new LineaDocumento 
                { 
                    ProductoId = productos[2].Id,
                    Descripcion = productos[2].Nombre + " - 12 meses",
                    Cantidad = 12,
                    PrecioUnitario = productos[2].PrecioUnitario,
                    IVA = productos[2].IVA
                }
            };
            presupuestos.Add(presup3);

            context.Presupuestos.AddRange(presupuestos);
            await context.SaveChangesAsync();
            return presupuestos;
        }

        private async Task CrearFacturasVenta(ApplicationDbContext context, int empresaId, 
            List<Cliente> clientes, List<Producto> productos)
        {
            var facturas = new List<Factura>();
            
            // Factura 1 - Pagada
            var factura1 = new Factura
            {
                EmpresaId = empresaId,
                TipoFactura = TipoFactura.Venta,
                ClienteId = clientes[1].Id,
                NumeroFactura = "FV-2026-001",
                FechaEmision = DateTime.Now.AddDays(-15),
                FormaPago = FormaPago.Transferencia,
                Estado = EstadoFactura.Pagada,
                Observaciones = "Factura de servicios de consultoría diciembre"
            };
            factura1.Lineas = new List<LineaDocumento>
            {
                new LineaDocumento 
                { 
                    ProductoId = productos[0].Id,
                    Descripcion = productos[0].Nombre,
                    Cantidad = 30,
                    PrecioUnitario = productos[0].PrecioUnitario,
                    IVA = productos[0].IVA
                }
            };
            factura1.Pagos = new List<PagoFactura>
            {
                new PagoFactura
                {
                    FechaPago = DateTime.Now.AddDays(-13),
                    Importe = 30 * productos[0].PrecioUnitario * 1.21m,
                    FormaPago = FormaPago.Transferencia,
                    Notas = "Transferencia bancaria"
                }
            };
            facturas.Add(factura1);

            // Factura 2 - Pendiente
            var factura2 = new Factura
            {
                EmpresaId = empresaId,
                TipoFactura = TipoFactura.Venta,
                ClienteId = clientes[0].Id,
                NumeroFactura = "FV-2026-002",
                FechaEmision = DateTime.Now.AddDays(-7),
                FormaPago = FormaPago.Transferencia,
                Estado = EstadoFactura.Emitida,
                Observaciones = "Factura de software - Vto: 30 días"
            };
            factura2.Lineas = new List<LineaDocumento>
            {
                new LineaDocumento 
                { 
                    ProductoId = productos[1].Id,
                    Descripcion = productos[1].Nombre,
                    Cantidad = 5,
                    PrecioUnitario = productos[1].PrecioUnitario,
                    IVA = productos[1].IVA
                }
            };
            facturas.Add(factura2);

            // Factura 3 - Parcialmente pagada
            var factura3 = new Factura
            {
                EmpresaId = empresaId,
                TipoFactura = TipoFactura.Venta,
                ClienteId = clientes[3].Id,
                NumeroFactura = "FV-2026-003",
                FechaEmision = DateTime.Now.AddDays(-10),
                FormaPago = FormaPago.Efectivo,
                Estado = EstadoFactura.Emitida,
                Observaciones = "Factura equipamiento informático"
            };
            factura3.Lineas = new List<LineaDocumento>
            {
                new LineaDocumento 
                { 
                    ProductoId = productos[3].Id,
                    Descripcion = productos[3].Nombre,
                    Cantidad = 3,
                    PrecioUnitario = productos[3].PrecioUnitario,
                    IVA = productos[3].IVA
                },
                new LineaDocumento 
                { 
                    ProductoId = productos[5].Id,
                    Descripcion = productos[5].Nombre,
                    Cantidad = 2,
                    PrecioUnitario = productos[5].PrecioUnitario,
                    IVA = productos[5].IVA
                }
            };
            var totalFactura3 = (3 * productos[3].PrecioUnitario + 2 * productos[5].PrecioUnitario) * 1.21m;
            factura3.Pagos = new List<PagoFactura>
            {
                new PagoFactura
                {
                    FechaPago = DateTime.Now.AddDays(-8),
                    Importe = totalFactura3 / 2, // 50% pagado
                    FormaPago = FormaPago.Efectivo,
                    Notas = "Efectivo - Anticipo"
                }
            };
            facturas.Add(factura3);

            // Factura 4 - Pagada
            var factura4 = new Factura
            {
                EmpresaId = empresaId,
                TipoFactura = TipoFactura.Venta,
                ClienteId = clientes[4].Id,
                NumeroFactura = "FV-2026-004",
                FechaEmision = DateTime.Now.AddDays(-20),
                FormaPago = FormaPago.TarjetaCredito,
                Estado = EstadoFactura.Pagada,
                Observaciones = "Factura curso formación"
            };
            factura4.Lineas = new List<LineaDocumento>
            {
                new LineaDocumento 
                { 
                    ProductoId = productos[4].Id,
                    Descripcion = productos[4].Nombre,
                    Cantidad = 1,
                    PrecioUnitario = productos[4].PrecioUnitario,
                    IVA = productos[4].IVA
                }
            };
            factura4.Pagos = new List<PagoFactura>
            {
                new PagoFactura
                {
                    FechaPago = DateTime.Now.AddDays(-20),
                    Importe = productos[4].PrecioUnitario * 1.21m,
                    FormaPago = FormaPago.TarjetaCredito,
                    Notas = "Tarjeta de crédito"
                }
            };
            facturas.Add(factura4);

            context.Facturas.AddRange(facturas);
            await context.SaveChangesAsync();
        }

        private async Task CrearFacturasCompra(ApplicationDbContext context, int empresaId, 
            List<Proveedor> proveedores, List<Producto> productos)
        {
            var facturas = new List<Factura>();
            
            // Factura de compra 1 - Pagada
            var factura1 = new Factura
            {
                EmpresaId = empresaId,
                TipoFactura = TipoFactura.Compra,
                ProveedorId = proveedores[0].Id,
                NumeroFactura = "FC-2026-001",
                FechaEmision = DateTime.Now.AddDays(-18),
                FormaPago = FormaPago.Transferencia,
                Estado = EstadoFactura.Pagada,
                Observaciones = "Compra material oficina"
            };
            factura1.Lineas = new List<LineaDocumento>
            {
                new LineaDocumento 
                { 
                    ProductoId = productos[7].Id,
                    Descripcion = productos[7].Nombre,
                    Cantidad = 10,
                    PrecioUnitario = productos[7].PrecioUnitario * 0.6m, // Precio de compra
                    IVA = productos[7].IVA
                }
            };
            factura1.Pagos = new List<PagoFactura>
            {
                new PagoFactura
                {
                    FechaPago = DateTime.Now.AddDays(-15),
                    Importe = 30 * productos[0].PrecioUnitario * 1.21m,
                    FormaPago = FormaPago.Transferencia,
                    Notas = "Transferencia bancaria"
                }
            };
            facturas.Add(factura1);

            // Factura de compra 2 - Pendiente
            var factura2 = new Factura
            {
                EmpresaId = empresaId,
                TipoFactura = TipoFactura.Compra,
                ProveedorId = proveedores[1].Id,
                NumeroFactura = "FC-2026-002",
                FechaEmision = DateTime.Now.AddDays(-5),
                FormaPago = FormaPago.Transferencia,
                Estado = EstadoFactura.Emitida,
                Observaciones = "Compra equipamiento informático"
            };
            factura2.Lineas = new List<LineaDocumento>
            {
                new LineaDocumento 
                { 
                    ProductoId = productos[3].Id,
                    Descripcion = productos[3].Nombre + " - Compra mayorista",
                    Cantidad = 5,
                    PrecioUnitario = productos[3].PrecioUnitario * 0.65m, // Precio de compra
                    IVA = productos[3].IVA
                }
            };
            facturas.Add(factura2);

            context.Facturas.AddRange(facturas);
            await context.SaveChangesAsync();
        }
    }
}
