using Microsoft.EntityFrameworkCore;
using FactioX.Models;

namespace FactioX.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // DbSets para todas las entidades
    public DbSet<Empresa> Empresas { get; set; }
    public DbSet<Usuario> Usuarios { get; set; }
    public DbSet<UsuarioEmpresa> UsuarioEmpresas { get; set; }
    public DbSet<ConfiguracionEmpresa> ConfiguracionEmpresa { get; set; }
    public DbSet<Cliente> Clientes { get; set; }
    public DbSet<Proveedor> Proveedores { get; set; }
    public DbSet<Producto> Productos { get; set; }
    public DbSet<Presupuesto> Presupuestos { get; set; }
    public DbSet<Factura> Facturas { get; set; }
    public DbSet<LineaDocumento> LineasDocumento { get; set; }
    public DbSet<PagoFactura> PagosFactura { get; set; }
    public DbSet<FormaPagoPersonalizada> FormasPagoPersonalizadas { get; set; }
    public DbSet<Gasto> Gastos { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configuración de Empresa
        modelBuilder.Entity<Empresa>(entity =>
        {
            entity.ToTable("Empresas");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.NombreComercial).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Slug).HasMaxLength(100).IsRequired();
            entity.HasIndex(e => e.Slug).IsUnique();
            entity.Property(e => e.PlanSuscripcion).HasMaxLength(50);
            
            // Relaciones
            entity.HasMany(e => e.Usuarios)
                .WithOne(u => u.Empresa)
                .HasForeignKey(u => u.EmpresaId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasMany(e => e.Clientes)
                .WithOne(c => c.Empresa)
                .HasForeignKey(c => c.EmpresaId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasMany(e => e.Facturas)
                .WithOne(f => f.Empresa)
                .HasForeignKey(f => f.EmpresaId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasMany(e => e.Presupuestos)
                .WithOne(p => p.Empresa)
                .HasForeignKey(p => p.EmpresaId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasMany(e => e.Productos)
                .WithOne(p => p.Empresa)
                .HasForeignKey(p => p.EmpresaId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasMany(e => e.Proveedores)
                .WithOne(p => p.Empresa)
                .HasForeignKey(p => p.EmpresaId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // Relación uno-a-uno con ConfiguracionEmpresa
            entity.HasOne(e => e.ConfiguracionEmpresa)
                .WithOne(c => c.Empresa)
                .HasForeignKey<ConfiguracionEmpresa>(c => c.EmpresaId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configuración de Usuario
        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.ToTable("Usuarios");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Username).HasMaxLength(50).IsRequired();
            entity.HasIndex(e => e.Username).IsUnique();
            entity.Property(e => e.Password).HasMaxLength(255).IsRequired();
            entity.Property(e => e.NombreCompleto).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Email).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Rol).HasMaxLength(30);
        });

        // Configuración de UsuarioEmpresa (many-to-many)
        modelBuilder.Entity<UsuarioEmpresa>(entity =>
        {
            entity.ToTable("UsuarioEmpresas");
            entity.HasKey(e => e.Id);
            
            // Relación con Usuario
            entity.HasOne(ue => ue.Usuario)
                .WithMany(u => u.EmpresasAsociadas)
                .HasForeignKey(ue => ue.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // Relación con Empresa
            entity.HasOne(ue => ue.Empresa)
                .WithMany(e => e.UsuariosAsociados)
                .HasForeignKey(ue => ue.EmpresaId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // Índice único para evitar duplicados
            entity.HasIndex(ue => new { ue.UsuarioId, ue.EmpresaId }).IsUnique();
        });

        // Configuración de ConfiguracionEmpresa
        modelBuilder.Entity<ConfiguracionEmpresa>(entity =>
        {
            entity.ToTable("ConfiguracionEmpresa");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.NombreEmpresa).HasMaxLength(200).IsRequired();
            entity.Property(e => e.NIF).HasMaxLength(20).IsRequired();
            entity.Property(e => e.IBAN).HasMaxLength(34);
            entity.Property(e => e.SerieFactura).HasMaxLength(10);
            entity.Property(e => e.SeriePresupuesto).HasMaxLength(10);
            
            // Convertir enums a strings para la base de datos
            entity.Property(e => e.TipoPersona)
                .HasConversion<string>()
                .HasMaxLength(20);
            entity.Property(e => e.TipoResidencia)
                .HasConversion<string>()
                .HasMaxLength(20);
        });

        // Configuración de Cliente
        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.ToTable("Clientes");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nombre).HasMaxLength(200);
            entity.Property(e => e.Apellidos).HasMaxLength(200);
            entity.Property(e => e.NombreEmpresa).HasMaxLength(200);
            entity.Property(e => e.NIF).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.CuentaBancaria).HasMaxLength(34);
            entity.Property(e => e.Pais).HasMaxLength(3);
            entity.Property(e => e.Telefono).HasMaxLength(20);
            entity.Property(e => e.CodigoPostal).HasMaxLength(10);
            entity.Property(e => e.Fax).HasMaxLength(20);
            
            // Convertir enums a strings para la base de datos
            entity.Property(e => e.Tipo)
                .HasConversion<string>()
                .HasMaxLength(20);
            
            entity.Property(e => e.TipoPersona)
                .HasConversion<string>()
                .HasMaxLength(20);
            
            entity.Property(e => e.TipoResidencia)
                .HasConversion<string>()
                .HasMaxLength(20);
        });

        // Configuración de Proveedor
        modelBuilder.Entity<Proveedor>(entity =>
        {
            entity.ToTable("Proveedores");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nombre).HasMaxLength(200).IsRequired();
            entity.Property(e => e.NIF).HasMaxLength(20);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.Telefono).HasMaxLength(20);
            entity.Property(e => e.Fax).HasMaxLength(20);
            entity.Property(e => e.CodigoPostal).HasMaxLength(10);
            entity.Property(e => e.Pais).HasMaxLength(3);
            entity.Property(e => e.CuentaBancaria).HasMaxLength(34);
        });

        // Configuración de Producto
        modelBuilder.Entity<Producto>(entity =>
        {
            entity.ToTable("Productos");
            entity.HasKey(e => e.Id);
            
            // Campos de texto
            entity.Property(e => e.Nombre).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Referencia).HasMaxLength(50);
            entity.Property(e => e.CodigoBarras).HasMaxLength(100);
            entity.Property(e => e.Categoria).HasMaxLength(100);
            entity.Property(e => e.Marca).HasMaxLength(100);
            entity.Property(e => e.Descripcion).HasMaxLength(1000);
            entity.Property(e => e.Observaciones).HasMaxLength(1000);
            entity.Property(e => e.Imagen).HasMaxLength(500);
            
            // Campos decimales con precisión
            entity.Property(e => e.PrecioUnitario).HasColumnType("decimal(18,2)");
            entity.Property(e => e.PrecioCompra).HasColumnType("decimal(18,2)");
            entity.Property(e => e.IVA).HasColumnType("decimal(5,2)");
            entity.Property(e => e.Descuento).HasColumnType("decimal(5,2)");
            entity.Property(e => e.Peso).HasColumnType("decimal(10,3)");
            entity.Property(e => e.Alto).HasColumnType("decimal(10,2)");
            entity.Property(e => e.Ancho).HasColumnType("decimal(10,2)");
            entity.Property(e => e.Largo).HasColumnType("decimal(10,2)");
            
            // Convertir enums a string
            entity.Property(e => e.Tipo)
                .HasConversion<string>()
                .HasMaxLength(20);
            
            entity.Property(e => e.UnidadMedida)
                .HasConversion<string>()
                .HasMaxLength(20);
        });

        // Configuración de Presupuesto
        modelBuilder.Entity<Presupuesto>(entity =>
        {
            entity.ToTable("Presupuestos");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Numero).HasMaxLength(50).IsRequired();
            
            // Convertir enum a string
            entity.Property(e => e.Estado)
                .HasConversion<string>()
                .HasMaxLength(20);
            
            entity.HasOne(p => p.Cliente)
                .WithMany()
                .HasForeignKey(p => p.ClienteId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configuración de Factura
        modelBuilder.Entity<Factura>(entity =>
        {
            entity.ToTable("Facturas");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.NumeroFactura).HasMaxLength(50).IsRequired();
            
            // Convertir enums a strings
            entity.Property(e => e.TipoFactura)
                .HasConversion<string>()
                .HasMaxLength(20);
            
            entity.Property(e => e.Estado)
                .HasConversion<string>()
                .HasMaxLength(20);
            
            entity.Property(e => e.FormaPago)
                .HasConversion<string>()
                .HasMaxLength(30);
            
            entity.Property(e => e.ClaseFactura)
                .HasConversion<string>()
                .HasMaxLength(20);
            
            // Importes decimales
            entity.Property(e => e.BaseImponible).HasColumnType("decimal(18,2)");
            entity.Property(e => e.PorcentajeIVA).HasColumnType("decimal(5,2)");
            entity.Property(e => e.ImporteIVA).HasColumnType("decimal(18,2)");
            entity.Property(e => e.CargosAdicionales).HasColumnType("decimal(18,2)");
            entity.Property(e => e.PorcentajeRetencion).HasColumnType("decimal(5,2)");
            entity.Property(e => e.ImporteRetencion).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Total).HasColumnType("decimal(18,2)");
            entity.Property(e => e.ImporteConcepto).HasColumnType("decimal(18,2)");
            entity.Property(e => e.DescuentosGenerales).HasColumnType("decimal(18,2)");
            entity.Property(e => e.RecargosGenerales).HasColumnType("decimal(18,2)");
            
            // Relaciones
            entity.HasOne(f => f.Cliente)
                .WithMany(c => c.Facturas)
                .HasForeignKey(f => f.ClienteId)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(false);
            
            entity.HasOne(f => f.Proveedor)
                .WithMany()
                .HasForeignKey(f => f.ProveedorId)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(false);
            
            entity.HasOne(f => f.Presupuesto)
                .WithMany()
                .HasForeignKey(f => f.PresupuestoId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Configuración de LineaDocumento
        modelBuilder.Entity<LineaDocumento>(entity =>
        {
            entity.ToTable("LineasDocumento");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Descripcion).HasMaxLength(500).IsRequired();
            entity.Property(e => e.Cantidad).HasColumnType("decimal(18,2)");
            entity.Property(e => e.PrecioUnitario).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Descuento).HasColumnType("decimal(5,2)");
            entity.Property(e => e.IVA).HasColumnType("decimal(5,2)");
            entity.Property(e => e.ImporteIVA).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Subtotal).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Total).HasColumnType("decimal(18,2)");
            
            // Relaciones
            entity.HasOne(l => l.Factura)
                .WithMany(f => f.Lineas)
                .HasForeignKey(l => l.FacturaId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(l => l.Presupuesto)
                .WithMany(p => p.Lineas)
                .HasForeignKey(l => l.PresupuestoId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(l => l.Producto)
                .WithMany()
                .HasForeignKey(l => l.ProductoId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Configuración de PagoFactura
        modelBuilder.Entity<PagoFactura>(entity =>
        {
            entity.ToTable("PagosFactura");
            entity.HasKey(e => e.Id);
            
            // Campos decimales
            entity.Property(e => e.Importe).HasColumnType("decimal(18,2)");
            
            // Campos de texto
            entity.Property(e => e.Notas).HasMaxLength(500);
            entity.Property(e => e.Referencia).HasMaxLength(100);
            
            // Convertir enum a string
            entity.Property(e => e.FormaPago)
                .HasConversion<string>()
                .HasMaxLength(30);
            
            // Relación con Factura
            entity.HasOne(p => p.Factura)
                .WithMany(f => f.Pagos)
                .HasForeignKey(p => p.FacturaId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // Relación con FormaPagoPersonalizada
            entity.HasOne(p => p.FormaPagoPersonalizada)
                .WithMany()
                .HasForeignKey(p => p.FormaPagoPersonalizadaId)
                .OnDelete(DeleteBehavior.SetNull);
        });
        
        // Configuración de FormaPagoPersonalizada
        modelBuilder.Entity<FormaPagoPersonalizada>(entity =>
        {
            entity.ToTable("FormasPagoPersonalizadas");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Nombre).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Descripcion).HasMaxLength(500);
            
            // Relación con Empresa
            entity.HasOne(t => t.Empresa)
                .WithMany()
                .HasForeignKey(t => t.EmpresaId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        // Configuración de Gasto
        modelBuilder.Entity<Gasto>(entity =>
        {
            entity.ToTable("Gastos");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Concepto).HasMaxLength(500).IsRequired();
            entity.Property(e => e.Categoria).HasMaxLength(100);
            entity.Property(e => e.Observaciones).HasMaxLength(1000);
            entity.Property(e => e.Importe).HasColumnType("decimal(18,2)");
            
            // Relación con Empresa
            entity.HasOne(g => g.Empresa)
                .WithMany()
                .HasForeignKey(g => g.EmpresaId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // Índice para búsquedas por empresa y fecha
            entity.HasIndex(g => new { g.EmpresaId, g.Fecha });
        });
    }
}
