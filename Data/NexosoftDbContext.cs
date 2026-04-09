using System;
using System.Collections.Generic;
using ECOMMERCE_NEXOSOFT.Models;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Scaffolding.Internal;

namespace ECOMMERCE_NEXOSOFT.Data;

public partial class NexosoftDbContext : DbContext
{
    public NexosoftDbContext()
    {

    }

    public NexosoftDbContext(DbContextOptions<NexosoftDbContext> options)
        : base(options)
    {

    }

    public virtual DbSet<Categorium> Categoria { get; set; }

    public virtual DbSet<Cliente> Clientes { get; set; }

    public virtual DbSet<Detallepedido> Detallepedidos { get; set; }

    public virtual DbSet<Domicilio> Domicilios { get; set; }

    public virtual DbSet<Facturacion> Facturacions { get; set; }

    public virtual DbSet<Pago> Pagos { get; set; }

    public virtual DbSet<Pedido> Pedidos { get; set; }

    public virtual DbSet<Producto> Productos { get; set; }

    public virtual DbSet<Rol> Rols { get; set; }

    public virtual DbSet<Stock> Stocks { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    public virtual DbSet<Ventum> Venta { get; set; }

    public virtual DbSet<Subcategorium> Subcategoria { get; set; }

    public virtual DbSet<Vendedor> Vendedors { get; set; }

    public virtual DbSet<Tienda> Tiendas { get; set; }

    public virtual DbSet<SolicitudVendedor> SolicitudVendedors { get; set; }

    public virtual DbSet<RolTienda> RolTiendas { get; set; }

    public virtual DbSet<MiembroTienda> MiembroTiendas { get; set; }

    public virtual DbSet<MovimientoInventario> MovimientoInventarios { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    #warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseMySql("server=localhost;port=3306;database=NexosoftDB;user=root;password=123", Microsoft.EntityFrameworkCore.ServerVersion.Parse("8.0.44-mysql"));

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_0900_ai_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<Categorium>(entity =>
        {
            entity.HasKey(e => e.IdCategoria).HasName("PRIMARY");

            entity.ToTable("categoria");

            entity.HasIndex(e => e.CodCategoria, "CodCategoria").IsUnique();

            entity.Property(e => e.Descripcion).HasMaxLength(100);
            entity.Property(e => e.NombreCategoria).HasMaxLength(100);
            entity.Property(e => e.VisiblePublico);
        });

        modelBuilder.Entity<Subcategorium>(entity =>
        {
            entity.HasKey(e => e.IdSubcategoria).HasName("PRIMARY");

            entity.ToTable("subcategoria");

            entity.HasIndex(e => e.CodSubcategoria, "CodSubcategoria").IsUnique();
            entity.HasIndex(e => e.IdCategoria, "IdCategoria");

            entity.Property(e => e.NombreSubcategoria).HasMaxLength(100);
            entity.Property(e => e.Descripcion).HasMaxLength(150);

            entity.HasOne(d => d.IdCategoriaNavigation).WithMany(p => p.Subcategoria)
                .HasForeignKey(d => d.IdCategoria)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Subcategoria_Categoria");
        });

        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.HasKey(e => e.IdCliente).HasName("PRIMARY");

            entity.ToTable("cliente");

            entity.HasIndex(e => e.CodCliente, "CodCliente").IsUnique();

            entity.HasIndex(e => e.IdUsuario, "IdUsuario").IsUnique();

            entity.Property(e => e.Direccion1).HasMaxLength(150);
            entity.Property(e => e.Direccion2).HasMaxLength(150);
            entity.Property(e => e.Direccion3).HasMaxLength(150);
            entity.Property(e => e.EstadoCliente).HasColumnType("enum('activo','inactivo','bloqueado')");
            entity.Property(e => e.FechaRegistroCliente).HasColumnType("datetime");

            entity.HasOne(d => d.IdUsuarioNavigation).WithOne(p => p.Cliente)
                .HasForeignKey<Cliente>(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("cliente_ibfk_1");
        });

        modelBuilder.Entity<Vendedor>(entity =>
        {
            entity.HasKey(e => e.IdVendedor).HasName("PRIMARY");

            entity.ToTable("vendedor");

            entity.HasIndex(e => e.CodVendedor, "UQ_CodVendedor").IsUnique();
            entity.HasIndex(e => e.IdUsuario, "UQ_Vendedor_IdUsuario").IsUnique();

            entity.Property(e => e.EstadoVendedor).HasColumnType("enum('activo','inactivo','bloqueado')");
            entity.Property(e => e.FechaRegistro).HasColumnType("datetime");

            entity.HasOne(d => d.IdUsuarioNavigation).WithOne(p => p.Vendedor)
                .HasForeignKey<Vendedor>(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Vendedor_Usuario");
        });

        modelBuilder.Entity<Tienda>(entity =>
        {
            entity.HasKey(e => e.IdTienda).HasName("PRIMARY");

            entity.ToTable("tienda");

            entity.HasIndex(e => e.CodTienda, "UQ_CodTienda").IsUnique();
            entity.HasIndex(e => e.IdVendedor, "IX_Tienda_IdVendedor");

            entity.Property(e => e.NombreTienda).HasMaxLength(100);
            entity.Property(e => e.Descripcion).HasMaxLength(150);
            entity.Property(e => e.LogoUrl).HasMaxLength(255);
            entity.Property(e => e.FechaRegistro).HasColumnType("datetime");

            entity.HasOne(d => d.IdVendedorNavigation).WithMany(p => p.Tienda)
                .HasForeignKey(d => d.IdVendedor)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Tienda_Vendedor");
        });

        modelBuilder.Entity<Detallepedido>(entity =>
        {
            entity.HasKey(e => e.IdDetallePedido).HasName("PRIMARY");

            entity.ToTable("detallepedido");

            entity.HasIndex(e => e.CodDetallePedido, "CodDetallePedido").IsUnique();

            entity.HasIndex(e => e.IdPedido, "IdPedido");

            entity.HasIndex(e => e.IdProducto, "IdProducto");

            entity.Property(e => e.PrecioUnitario).HasPrecision(10, 2);
            entity.Property(e => e.Subtotal).HasPrecision(10, 2);

            entity.HasOne(d => d.IdPedidoNavigation).WithMany(p => p.Detallepedidos)
                .HasForeignKey(d => d.IdPedido)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("detallepedido_ibfk_1");

            entity.HasOne(d => d.IdProductoNavigation).WithMany(p => p.Detallepedidos)
                .HasForeignKey(d => d.IdProducto)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("detallepedido_ibfk_2");
        });

        modelBuilder.Entity<Domicilio>(entity =>
        {
            entity.HasKey(e => e.IdDomicilio).HasName("PRIMARY");

            entity.ToTable("domicilio");

            entity.HasIndex(e => e.CodDomicilio, "CodDomicilio").IsUnique();

            entity.HasIndex(e => e.IdCliente, "IdCliente");

            entity.Property(e => e.CostoEnvio).HasPrecision(10, 2);
            entity.Property(e => e.DireccionEntrega).HasMaxLength(150);
            entity.Property(e => e.EstadoEnvio).HasColumnType("enum('pendiente','en camino','entregado','cancelado')");
            entity.Property(e => e.HoraEnvio).HasColumnType("datetime");
            entity.Property(e => e.Transportadora).HasMaxLength(100);

            entity.HasOne(d => d.IdClienteNavigation).WithMany(p => p.Domicilios)
                .HasForeignKey(d => d.IdCliente)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("domicilio_ibfk_1");
        });

        modelBuilder.Entity<Facturacion>(entity =>
        {
            entity.HasKey(e => e.IdFactura).HasName("PRIMARY");

            entity.ToTable("facturacion");

            entity.HasIndex(e => e.CodFactura, "CodFactura").IsUnique();

            entity.HasIndex(e => e.IdPago, "IdPago").IsUnique();

            entity.HasIndex(e => e.NumeroFactura, "NumeroFactura").IsUnique();

            entity.Property(e => e.EstadoFactura).HasColumnType("enum('generada','anulada')");
            entity.Property(e => e.FechaFactura).HasColumnType("datetime");
            entity.Property(e => e.Impuestos).HasPrecision(10, 2);
            entity.Property(e => e.NumeroFactura).HasMaxLength(30);
            entity.Property(e => e.Subtotal).HasPrecision(10, 2);
            entity.Property(e => e.TotalFactura).HasPrecision(10, 2);

            entity.HasOne(d => d.IdPagoNavigation).WithOne(p => p.Facturacion)
                .HasForeignKey<Facturacion>(d => d.IdPago)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("facturacion_ibfk_1");
        });

        modelBuilder.Entity<Pago>(entity =>
        {
            entity.HasKey(e => e.IdPago).HasName("PRIMARY");

            entity.ToTable("pago");

            entity.HasIndex(e => e.CodPago, "CodPago").IsUnique();

            entity.HasIndex(e => e.CodigoAutorizacion, "CodigoAutorizacion").IsUnique();

            entity.HasIndex(e => e.IdVenta, "IdVenta").IsUnique();

            entity.Property(e => e.CodigoAutorizacion).HasMaxLength(50);
            entity.Property(e => e.Descripcion).HasMaxLength(150);
            entity.Property(e => e.EstadoPago).HasColumnType("enum('aprobado','desaprobado','enproceso','reembolsado')");
            entity.Property(e => e.FechaPago).HasColumnType("datetime");
            entity.Property(e => e.MetodoPago).HasColumnType("enum('efectivo','tarjeta credito','tarjeta debito','transferencias')");
            entity.Property(e => e.MontoPagado).HasPrecision(10, 2);

            entity.HasOne(d => d.IdVentaNavigation).WithOne(p => p.Pago)
                .HasForeignKey<Pago>(d => d.IdVenta)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("pago_ibfk_1");
        });

        modelBuilder.Entity<Pedido>(entity =>
        {
            entity.HasKey(e => e.IdPedido).HasName("PRIMARY");

            entity.ToTable("pedido");

            entity.HasIndex(e => e.CodPedido, "CodPedido").IsUnique();

            entity.HasIndex(e => e.IdUsuario, "IdUsuario");

            entity.HasIndex(e => e.IdTienda, "IdTienda");

            entity.Property(e => e.CostoEnvio).HasPrecision(10, 2);
            entity.Property(e => e.EstadoPedido).HasColumnType("enum('pendiente','en preparación','en camino','entregado','cancelado')");
            entity.Property(e => e.FechaCreacion).HasColumnType("datetime");
            entity.Property(e => e.MetodoEntrega).HasMaxLength(100);
            entity.Property(e => e.Subtotal).HasPrecision(10, 2);
            entity.Property(e => e.Total).HasPrecision(10, 2);

            entity.HasOne(d => d.IdTiendaNavigation).WithMany(p => p.Pedidos)
                .HasForeignKey(d => d.IdTienda)
                .HasConstraintName("FK_Pedido_Tienda");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.Pedidos)
                .HasForeignKey(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("pedido_ibfk_1");
        });

        modelBuilder.Entity<Producto>(entity =>
        {
            entity.HasKey(e => e.IdProducto).HasName("PRIMARY");

            entity.ToTable("producto");

            entity.HasIndex(e => e.CodProducto, "CodProducto").IsUnique();

            entity.HasIndex(e => e.CodigoBarrasProducto, "CodigoBarrasProducto").IsUnique();

            entity.HasIndex(e => e.IdCategoria, "IdCategoria");

            entity.HasIndex(e => e.SkuProducto, "SkuProducto").IsUnique();

            entity.HasIndex(e => e.IdSubcategoria, "IdSubcategoria");

            entity.HasIndex(e => e.IdTienda, "IdTienda");

            entity.Property(e => e.CodigoBarrasProducto).HasMaxLength(20);
            entity.Property(e => e.DescripcionCorta).HasMaxLength(255);
            entity.Property(e => e.MarcaProducto).HasMaxLength(100);
            entity.Property(e => e.NombreProducto).HasMaxLength(100);
            entity.Property(e => e.PrecioVentaProducto).HasPrecision(10, 2);
            entity.Property(e => e.VisiblePublico);
            entity.Property(e => e.SkuProducto).HasMaxLength(20);
            entity.Property(e => e.UnidadMedidaProducto).HasColumnType("enum('unidad','caja','paquete','metro','metro_cuadrado','litro','galon','kilogramo')");

            entity.HasOne(d => d.IdSubcategoriaNavigation).WithMany(p => p.Productos)
                .HasForeignKey(d => d.IdSubcategoria)
                .HasConstraintName("FK_Producto_Subcategoria");

            entity.HasOne(d => d.IdTiendaNavigation).WithMany(p => p.Productos)
                .HasForeignKey(d => d.IdTienda)
                .HasConstraintName("FK_Producto_Tienda");

            entity.HasOne(d => d.IdCategoriaNavigation).WithMany(p => p.Productos)
                .HasForeignKey(d => d.IdCategoria)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("producto_ibfk_1");
        });

        modelBuilder.Entity<Rol>(entity =>
        {
            entity.HasKey(e => e.IdRol).HasName("PRIMARY");

            entity.ToTable("rol");

            entity.HasIndex(e => e.CodRol, "CodRol").IsUnique();

            entity.Property(e => e.Descripcion).HasMaxLength(100);
            entity.Property(e => e.NombreRol).HasColumnType("enum('cliente','administrador','vendedor')");
        });

        modelBuilder.Entity<Stock>(entity =>
        {
            entity.HasKey(e => e.IdInventario).HasName("PRIMARY");

            entity.ToTable("stock");

            entity.HasIndex(e => e.CodInventario, "CodInventario").IsUnique();
            entity.HasIndex(e => e.IdProducto, "IdProducto").IsUnique();

            entity.Property(e => e.PrecioCompraStock).HasPrecision(10, 2);

            entity.HasOne(d => d.IdProductoNavigation).WithOne(p => p.Stock)
                .HasForeignKey<Stock>(d => d.IdProducto)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("stock_ibfk_1");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.IdUsuario).HasName("PRIMARY");

            entity.ToTable("usuarios");

            entity.HasIndex(e => e.CodUsuario, "CodUsuario").IsUnique();
            entity.HasIndex(e => e.CorreoElectronico, "CorreoElectronico").IsUnique();
            entity.HasIndex(e => e.IdRol, "IdRol");
            entity.HasIndex(e => e.NumeroIdentificacion, "NumeroIdentificacion").IsUnique();

            entity.Property(e => e.Apellido).HasMaxLength(100);
            entity.Property(e => e.Contrasena).HasMaxLength(255);
            entity.Property(e => e.CorreoElectronico).HasMaxLength(100);
            entity.Property(e => e.Nombre).HasMaxLength(100);
            entity.Property(e => e.NumeroIdentificacion).HasMaxLength(20);
            entity.Property(e => e.Telefono).HasMaxLength(50);
            entity.Property(e => e.TipoIdentificacion).HasColumnType("enum('cc','ti','ppt','pasaporte')");

            entity.HasOne(d => d.IdRolNavigation).WithMany(p => p.Usuarios)
                .HasForeignKey(d => d.IdRol)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("usuarios_ibfk_1");
        });

        modelBuilder.Entity<Ventum>(entity =>
        {
            entity.HasKey(e => e.IdVenta).HasName("PRIMARY");
            
            entity.ToTable("venta");
            
            entity.HasIndex(e => e.CodVenta, "CodVenta").IsUnique();
            entity.HasIndex(e => e.IdCliente, "IdCliente");
            entity.HasIndex(e => e.IdPedido, "IdPedido").IsUnique();
           
            entity.Property(e => e.EstadoVenta).HasColumnType("enum('pendiente','pagada','cancelada')");
            entity.Property(e => e.FechaVenta).HasColumnType("datetime");

            entity.HasOne(d => d.IdClienteNavigation).WithMany(p => p.Venta)
                .HasForeignKey(d => d.IdCliente)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("venta_ibfk_1");

            entity.HasOne(d => d.IdPedidoNavigation).WithOne(p => p.Ventum)
                .HasForeignKey<Ventum>(d => d.IdPedido)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("venta_ibfk_2");
        });

        modelBuilder.Entity<SolicitudVendedor>(entity =>
        {
            entity.HasKey(e => e.IdSolicitudVendedor).HasName("PRIMARY");
            
            entity.ToTable("solicitud_vendedor");
            
            entity.HasIndex(e => e.CodSolicitudVendedor, "UQ_CodSolicitudVendedor").IsUnique();
            entity.HasIndex(e => e.IdUsuario, "IX_SolicitudVendedor_IdUsuario");
            
            entity.Property(e => e.NombreTiendaSolicitada).HasMaxLength(100);
            entity.Property(e => e.DescripcionTienda).HasMaxLength(150);
            entity.Property(e => e.EstadoSolicitud).HasColumnType("enum('pendiente','aprobada','rechazada')");
            entity.Property(e => e.ObservacionAdmin).HasMaxLength(200);
            entity.Property(e => e.FechaSolicitud).HasColumnType("datetime");
            entity.Property(e => e.FechaRespuesta).HasColumnType("datetime");
            entity.Property(e => e.RazonSocial).HasMaxLength(150);
            entity.Property(e => e.NitRut).HasMaxLength(20);
            entity.Property(e => e.NombreRepresentante).HasMaxLength(100);
            entity.Property(e => e.TelefonoContacto).HasMaxLength(10);
            entity.Property(e => e.CorreoContacto).HasMaxLength(100);
            entity.Property(e => e.DireccionComercial).HasMaxLength(150);

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.SolicitudVendedors)
                .HasForeignKey(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SolicitudVendedor_Usuario");
        });

        modelBuilder.Entity<RolTienda>(entity =>
        {
            entity.HasKey(e => e.IdRolTienda);

            entity.ToTable("rol_tienda");

            entity.Property(e => e.NombreRol)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(e => e.Descripcion)
                .HasMaxLength(100);
        });

        modelBuilder.Entity<MiembroTienda>(entity =>
        {
            entity.HasKey(e => e.IdMiembroTienda);

            entity.ToTable("miembro_tienda");

            entity.HasIndex(e => e.IdUsuario);
            entity.HasIndex(e => e.IdTienda);
            entity.HasIndex(e => e.IdRolTienda);

            entity.Property(e => e.FechaIngreso)
                .HasColumnType("datetime");

            entity.HasOne(d => d.IdUsuarioNavigation)
                .WithMany(p => p.MiembroTiendas)
                .HasForeignKey(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.IdTiendaNavigation)
                .WithMany(p => p.MiembroTiendas)
                .HasForeignKey(d => d.IdTienda)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.IdRolTiendaNavigation)
                .WithMany(p => p.Miembros)
                .HasForeignKey(d => d.IdRolTienda)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<MovimientoInventario>(entity =>
        {
            entity.HasKey(e => e.IdMovimiento);

            entity.ToTable("movimiento_inventario");

            entity.HasIndex(e => e.IdProducto);

            entity.Property(e => e.TipoMovimiento)
                .HasColumnType("enum('entrada','salida','ajuste')");

            entity.Property(e => e.Motivo)
                .HasMaxLength(150);

            entity.Property(e => e.FechaMovimiento)
                .HasColumnType("datetime");

            entity.HasOne(d => d.IdProductoNavigation)
                .WithMany()
                .HasForeignKey(d => d.IdProducto)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.IdUsuarioNavigation)
                .WithMany()
                .HasForeignKey(d => d.IdUsuario);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
