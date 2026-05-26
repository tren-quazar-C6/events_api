using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using events_api.Entities;

namespace events_api.Data;

public partial class QuasarDbContext : DbContext
{
    public QuasarDbContext(DbContextOptions<QuasarDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Asiento> Asientos { get; set; }

    public virtual DbSet<Auditorium> Auditoria { get; set; }

    public virtual DbSet<EstadoTicket> EstadoTickets { get; set; }

    public virtual DbSet<Evento> Eventos { get; set; }

    public virtual DbSet<EventoAsiento> EventoAsientos { get; set; }

    public virtual DbSet<EventoZona> EventoZonas { get; set; }

    public virtual DbSet<Favorito> Favoritos { get; set; }

    public virtual DbSet<Imagene> Imagenes { get; set; }

    public virtual DbSet<Notificacione> Notificaciones { get; set; }

    public virtual DbSet<Pqr> Pqrs { get; set; }

    public virtual DbSet<PqrsMensaje> PqrsMensajes { get; set; }

    public virtual DbSet<RolStaff> RolStaffs { get; set; }

    public virtual DbSet<Scan> Scans { get; set; }

    public virtual DbSet<Staff> Staff { get; set; }

    public virtual DbSet<Ticket> Tickets { get; set; }

    public virtual DbSet<TipoEvento> TipoEventos { get; set; }

    public virtual DbSet<TransaccionesPago> TransaccionesPagos { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    public virtual DbSet<Venta> Ventas { get; set; }

    public virtual DbSet<WebhookLog> WebhookLogs { get; set; }

    public virtual DbSet<Zona> Zonas { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_0900_ai_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<Asiento>(entity =>
        {
            entity.HasKey(e => e.IdAsiento).HasName("PRIMARY");

            entity.ToTable("ASIENTOS");

            entity.HasIndex(e => new { e.IdZona, e.Fila, e.Numero }, "id_zona").IsUnique();

            entity.Property(e => e.IdAsiento).HasColumnName("id_asiento");
            entity.Property(e => e.Activo)
                .HasDefaultValueSql("'1'")
                .HasColumnName("activo");
            entity.Property(e => e.Fila)
                .HasMaxLength(10)
                .HasColumnName("fila");
            entity.Property(e => e.IdZona).HasColumnName("id_zona");
            entity.Property(e => e.Numero).HasColumnName("numero");
            entity.Property(e => e.PosX).HasColumnName("pos_x");
            entity.Property(e => e.PosY).HasColumnName("pos_y");

            entity.HasOne(d => d.IdZonaNavigation).WithMany(p => p.Asientos)
                .HasForeignKey(d => d.IdZona)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("ASIENTOS_ibfk_1");
        });

        modelBuilder.Entity<Auditorium>(entity =>
        {
            entity.HasKey(e => e.IdAuditoria).HasName("PRIMARY");

            entity.ToTable("AUDITORIA");

            entity.HasIndex(e => e.IdStaff, "idx_auditoria_staff");

            entity.HasIndex(e => new { e.TablaAfectada, e.IdRegistroAfectado }, "idx_auditoria_tabla");

            entity.Property(e => e.IdAuditoria).HasColumnName("id_auditoria");
            entity.Property(e => e.Accion)
                .HasMaxLength(100)
                .HasColumnName("accion");
            entity.Property(e => e.Detalle)
                .HasColumnType("json")
                .HasColumnName("detalle");
            entity.Property(e => e.Fecha)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("fecha");
            entity.Property(e => e.IdRegistroAfectado).HasColumnName("id_registro_afectado");
            entity.Property(e => e.IdStaff).HasColumnName("id_staff");
            entity.Property(e => e.TablaAfectada)
                .HasMaxLength(100)
                .HasColumnName("tabla_afectada");

            entity.HasOne(d => d.IdStaffNavigation).WithMany(p => p.Auditoria)
                .HasForeignKey(d => d.IdStaff)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("AUDITORIA_ibfk_1");
        });

        modelBuilder.Entity<EstadoTicket>(entity =>
        {
            entity.HasKey(e => e.IdEstadoTicket).HasName("PRIMARY");

            entity.ToTable("ESTADO_TICKET");

            entity.HasIndex(e => e.NombreEstado, "nombre_estado").IsUnique();

            entity.Property(e => e.IdEstadoTicket).HasColumnName("id_estado_ticket");
            entity.Property(e => e.NombreEstado)
                .HasMaxLength(50)
                .HasColumnName("nombre_estado");
        });

        modelBuilder.Entity<Evento>(entity =>
        {
            entity.HasKey(e => e.IdEvento).HasName("PRIMARY");

            entity.ToTable("EVENTOS");

            entity.HasIndex(e => e.CreadoPorStaff, "creado_por_staff");

            entity.HasIndex(e => e.IdTipoEvento, "id_tipo_evento");

            entity.HasIndex(e => e.FechaEvento, "idx_eventos_fecha");

            entity.HasIndex(e => new { e.Publicado, e.Activo }, "idx_eventos_publicado");

            entity.Property(e => e.IdEvento).HasColumnName("id_evento");
            entity.Property(e => e.Activo)
                .HasDefaultValueSql("'1'")
                .HasColumnName("activo");
            entity.Property(e => e.CapacidadTotal).HasColumnName("capacidad_total");
            entity.Property(e => e.CreadoPorStaff).HasColumnName("creado_por_staff");
            entity.Property(e => e.Descripcion)
                .HasColumnType("text")
                .HasColumnName("descripcion");
            entity.Property(e => e.FechaCancelacion)
                .HasColumnType("datetime")
                .HasColumnName("fecha_cancelacion");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.FechaEvento)
                .HasColumnType("datetime")
                .HasColumnName("fecha_evento");
            entity.Property(e => e.FechaFinVentas)
                .HasColumnType("datetime")
                .HasColumnName("fecha_fin_ventas");
            entity.Property(e => e.FechaInicioVentas)
                .HasColumnType("datetime")
                .HasColumnName("fecha_inicio_ventas");
            entity.Property(e => e.IdTipoEvento).HasColumnName("id_tipo_evento");
            entity.Property(e => e.MotivoCancelacion)
                .HasColumnType("text")
                .HasColumnName("motivo_cancelacion");
            entity.Property(e => e.NombreEvento)
                .HasMaxLength(150)
                .HasColumnName("nombre_evento");
            entity.Property(e => e.Publicado)
                .HasDefaultValueSql("'1'")
                .HasColumnName("publicado");

            entity.HasOne(d => d.CreadoPorStaffNavigation).WithMany(p => p.Eventos)
                .HasForeignKey(d => d.CreadoPorStaff)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("EVENTOS_ibfk_2");

            entity.HasOne(d => d.IdTipoEventoNavigation).WithMany(p => p.Eventos)
                .HasForeignKey(d => d.IdTipoEvento)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("EVENTOS_ibfk_1");
        });

        modelBuilder.Entity<EventoAsiento>(entity =>
        {
            entity.HasKey(e => e.IdEventoAsiento).HasName("PRIMARY");

            entity.ToTable("EVENTO_ASIENTO");

            entity.HasIndex(e => e.IdAsiento, "id_asiento");

            entity.HasIndex(e => new { e.IdEvento, e.IdAsiento }, "id_evento").IsUnique();

            entity.HasIndex(e => e.Estado, "idx_evento_asiento_estado");

            entity.HasIndex(e => e.IdEvento, "idx_evento_asiento_evento");

            entity.Property(e => e.IdEventoAsiento).HasColumnName("id_evento_asiento");
            entity.Property(e => e.Estado)
                .HasDefaultValueSql("'DISPONIBLE'")
                .HasColumnType("enum('DISPONIBLE','RESERVADO','VENDIDO','BLOQUEADO')")
                .HasColumnName("estado");
            entity.Property(e => e.FechaReserva)
                .HasColumnType("datetime")
                .HasColumnName("fecha_reserva");
            entity.Property(e => e.IdAsiento).HasColumnName("id_asiento");
            entity.Property(e => e.IdEvento).HasColumnName("id_evento");
            entity.Property(e => e.ReservaExpira)
                .HasColumnType("datetime")
                .HasColumnName("reserva_expira");

            entity.HasOne(d => d.IdAsientoNavigation).WithMany(p => p.EventoAsientos)
                .HasForeignKey(d => d.IdAsiento)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("EVENTO_ASIENTO_ibfk_2");

            entity.HasOne(d => d.IdEventoNavigation).WithMany(p => p.EventoAsientos)
                .HasForeignKey(d => d.IdEvento)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("EVENTO_ASIENTO_ibfk_1");
        });

        modelBuilder.Entity<EventoZona>(entity =>
        {
            entity.HasKey(e => e.IdEventoZona).HasName("PRIMARY");

            entity.ToTable("EVENTO_ZONA");

            entity.HasIndex(e => new { e.IdEvento, e.IdZona }, "id_evento").IsUnique();

            entity.HasIndex(e => e.IdZona, "id_zona");

            entity.Property(e => e.IdEventoZona).HasColumnName("id_evento_zona");
            entity.Property(e => e.Activo)
                .HasDefaultValueSql("'1'")
                .HasColumnName("activo");
            entity.Property(e => e.Capacidad).HasColumnName("capacidad");
            entity.Property(e => e.CargoServicio)
                .HasPrecision(10, 2)
                .HasDefaultValueSql("'0.00'")
                .HasColumnName("cargo_servicio");
            entity.Property(e => e.IdEvento).HasColumnName("id_evento");
            entity.Property(e => e.IdZona).HasColumnName("id_zona");
            entity.Property(e => e.Precio)
                .HasPrecision(10, 2)
                .HasColumnName("precio");

            entity.HasOne(d => d.IdEventoNavigation).WithMany(p => p.EventoZonas)
                .HasForeignKey(d => d.IdEvento)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("EVENTO_ZONA_ibfk_1");

            entity.HasOne(d => d.IdZonaNavigation).WithMany(p => p.EventoZonas)
                .HasForeignKey(d => d.IdZona)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("EVENTO_ZONA_ibfk_2");
        });

        modelBuilder.Entity<Favorito>(entity =>
        {
            entity.HasKey(e => e.IdFavorito).HasName("PRIMARY");

            entity.ToTable("FAVORITOS");

            entity.HasIndex(e => new { e.IdUsuario, e.IdEvento }, "id_usuario").IsUnique();

            entity.HasIndex(e => e.IdEvento, "idx_favoritos_evento");

            entity.Property(e => e.IdFavorito).HasColumnName("id_favorito");
            entity.Property(e => e.FechaAgregado)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("fecha_agregado");
            entity.Property(e => e.IdEvento).HasColumnName("id_evento");
            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");

            entity.HasOne(d => d.IdEventoNavigation).WithMany(p => p.Favoritos)
                .HasForeignKey(d => d.IdEvento)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FAVORITOS_ibfk_2");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.Favoritos)
                .HasForeignKey(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FAVORITOS_ibfk_1");
        });

        modelBuilder.Entity<Imagene>(entity =>
        {
            entity.HasKey(e => e.IdImagen).HasName("PRIMARY");

            entity.ToTable("IMAGENES");

            entity.HasIndex(e => e.IdEvento, "id_evento");

            entity.Property(e => e.IdImagen).HasColumnName("id_imagen");
            entity.Property(e => e.Activo)
                .HasDefaultValueSql("'1'")
                .HasColumnName("activo");
            entity.Property(e => e.IdEvento).HasColumnName("id_evento");
            entity.Property(e => e.Orden)
                .HasDefaultValueSql("'0'")
                .HasColumnName("orden");
            entity.Property(e => e.Principal)
                .HasDefaultValueSql("'0'")
                .HasColumnName("principal");
            entity.Property(e => e.RutaUrl)
                .HasMaxLength(255)
                .HasColumnName("ruta_url");

            entity.HasOne(d => d.IdEventoNavigation).WithMany(p => p.Imagenes)
                .HasForeignKey(d => d.IdEvento)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("IMAGENES_ibfk_1");
        });

        modelBuilder.Entity<Notificacione>(entity =>
        {
            entity.HasKey(e => e.IdNotificacion).HasName("PRIMARY");

            entity.ToTable("NOTIFICACIONES");

            entity.HasIndex(e => e.IdEvento, "id_evento");

            entity.HasIndex(e => new { e.IdUsuario, e.Leido }, "idx_notificaciones_usuario");

            entity.Property(e => e.IdNotificacion).HasColumnName("id_notificacion");
            entity.Property(e => e.FechaEnvio)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("fecha_envio");
            entity.Property(e => e.IdEvento).HasColumnName("id_evento");
            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
            entity.Property(e => e.Leido)
                .HasDefaultValueSql("'0'")
                .HasColumnName("leido");
            entity.Property(e => e.Mensaje)
                .HasColumnType("text")
                .HasColumnName("mensaje");
            entity.Property(e => e.Tipo)
                .HasDefaultValueSql("'IN_APP'")
                .HasColumnType("enum('IN_APP','EMAIL')")
                .HasColumnName("tipo");
            entity.Property(e => e.Titulo)
                .HasMaxLength(255)
                .HasColumnName("titulo");

            entity.HasOne(d => d.IdEventoNavigation).WithMany(p => p.Notificaciones)
                .HasForeignKey(d => d.IdEvento)
                .HasConstraintName("NOTIFICACIONES_ibfk_2");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.Notificaciones)
                .HasForeignKey(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("NOTIFICACIONES_ibfk_1");
        });

        modelBuilder.Entity<Pqr>(entity =>
        {
            entity.HasKey(e => e.IdPqrs).HasName("PRIMARY");

            entity.ToTable("PQRS");

            entity.HasIndex(e => e.AsignadoStaff, "asignado_staff");

            entity.HasIndex(e => e.Estado, "idx_pqrs_estado");

            entity.HasIndex(e => e.IdUsuario, "idx_pqrs_usuario");

            entity.Property(e => e.IdPqrs).HasColumnName("id_pqrs");
            entity.Property(e => e.AsignadoStaff).HasColumnName("asignado_staff");
            entity.Property(e => e.Asunto)
                .HasMaxLength(255)
                .HasColumnName("asunto");
            entity.Property(e => e.Estado)
                .HasDefaultValueSql("'ABIERTO'")
                .HasColumnType("enum('ABIERTO','EN_PROCESO','RESPONDIDO','CERRADO')")
                .HasColumnName("estado");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.FechaUltimaRespuesta)
                .HasColumnType("datetime")
                .HasColumnName("fecha_ultima_respuesta");
            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
            entity.Property(e => e.Tipo)
                .HasColumnType("enum('PREGUNTA','QUEJA','RECLAMO','SUGERENCIA')")
                .HasColumnName("tipo");

            entity.HasOne(d => d.AsignadoStaffNavigation).WithMany(p => p.Pqrs)
                .HasForeignKey(d => d.AsignadoStaff)
                .HasConstraintName("PQRS_ibfk_2");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.Pqrs)
                .HasForeignKey(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("PQRS_ibfk_1");
        });

        modelBuilder.Entity<PqrsMensaje>(entity =>
        {
            entity.HasKey(e => e.IdMensaje).HasName("PRIMARY");

            entity.ToTable("PQRS_MENSAJE");

            entity.HasIndex(e => e.IdPqrs, "id_pqrs");

            entity.Property(e => e.IdMensaje).HasColumnName("id_mensaje");
            entity.Property(e => e.Fecha)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("fecha");
            entity.Property(e => e.IdPqrs).HasColumnName("id_pqrs");
            entity.Property(e => e.IdRemitente).HasColumnName("id_remitente");
            entity.Property(e => e.Mensaje)
                .HasColumnType("text")
                .HasColumnName("mensaje");
            entity.Property(e => e.Remitente)
                .HasColumnType("enum('USUARIO','STAFF')")
                .HasColumnName("remitente");

            entity.HasOne(d => d.IdPqrsNavigation).WithMany(p => p.PqrsMensajes)
                .HasForeignKey(d => d.IdPqrs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("PQRS_MENSAJE_ibfk_1");
        });

        modelBuilder.Entity<RolStaff>(entity =>
        {
            entity.HasKey(e => e.IdRolStaff).HasName("PRIMARY");

            entity.ToTable("ROL_STAFF");

            entity.HasIndex(e => e.NombreRol, "nombre_rol").IsUnique();

            entity.Property(e => e.IdRolStaff).HasColumnName("id_rol_staff");
            entity.Property(e => e.Activo)
                .HasDefaultValueSql("'1'")
                .HasColumnName("activo");
            entity.Property(e => e.NombreRol)
                .HasMaxLength(50)
                .HasColumnName("nombre_rol");
        });

        modelBuilder.Entity<Scan>(entity =>
        {
            entity.HasKey(e => e.IdScan).HasName("PRIMARY");

            entity.ToTable("SCAN");

            entity.HasIndex(e => e.IdStaff, "id_staff");

            entity.HasIndex(e => e.FechaScan, "idx_scan_fecha");

            entity.HasIndex(e => e.IdTicket, "idx_scan_ticket");

            entity.Property(e => e.IdScan).HasColumnName("id_scan");
            entity.Property(e => e.FechaScan)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("fecha_scan");
            entity.Property(e => e.IdStaff).HasColumnName("id_staff");
            entity.Property(e => e.IdTicket).HasColumnName("id_ticket");
            entity.Property(e => e.Observacion)
                .HasColumnType("text")
                .HasColumnName("observacion");
            entity.Property(e => e.Resultado)
                .HasColumnType("enum('VALIDO','DUPLICADO','INVALIDO','FALSO')")
                .HasColumnName("resultado");

            entity.HasOne(d => d.IdStaffNavigation).WithMany(p => p.Scans)
                .HasForeignKey(d => d.IdStaff)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("SCAN_ibfk_2");

            entity.HasOne(d => d.IdTicketNavigation).WithMany(p => p.Scans)
                .HasForeignKey(d => d.IdTicket)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("SCAN_ibfk_1");
        });

        modelBuilder.Entity<Staff>(entity =>
        {
            entity.HasKey(e => e.IdStaff).HasName("PRIMARY");

            entity.ToTable("STAFF");

            entity.HasIndex(e => e.Email, "email").IsUnique();

            entity.HasIndex(e => e.IdRolStaff, "id_rol_staff");

            entity.Property(e => e.IdStaff).HasColumnName("id_staff");
            entity.Property(e => e.Activo)
                .HasDefaultValueSql("'1'")
                .HasColumnName("activo");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");
            entity.Property(e => e.FechaRegistro)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("fecha_registro");
            entity.Property(e => e.IdRolStaff).HasColumnName("id_rol_staff");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .HasColumnName("nombre");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .HasColumnName("password_hash");

            entity.HasOne(d => d.IdRolStaffNavigation).WithMany(p => p.Staff)
                .HasForeignKey(d => d.IdRolStaff)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("STAFF_ibfk_1");
        });

        modelBuilder.Entity<Ticket>(entity =>
        {
            entity.HasKey(e => e.IdTicket).HasName("PRIMARY");

            entity.ToTable("TICKETS");

            entity.HasIndex(e => e.CodigoUnico, "codigo_unico").IsUnique();

            entity.HasIndex(e => e.IdEstadoTicket, "id_estado_ticket");

            entity.HasIndex(e => e.IdEventoAsiento, "id_evento_asiento").IsUnique();

            entity.HasIndex(e => e.IdVenta, "id_venta");

            entity.HasIndex(e => e.QrToken, "idx_tickets_qr").IsUnique();

            entity.Property(e => e.IdTicket).HasColumnName("id_ticket");
            entity.Property(e => e.CodigoUnico).HasColumnName("codigo_unico");
            entity.Property(e => e.FechaGeneracion)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("fecha_generacion");
            entity.Property(e => e.FechaImpresion)
                .HasColumnType("datetime")
                .HasColumnName("fecha_impresion");
            entity.Property(e => e.IdEstadoTicket).HasColumnName("id_estado_ticket");
            entity.Property(e => e.IdEventoAsiento).HasColumnName("id_evento_asiento");
            entity.Property(e => e.IdVenta).HasColumnName("id_venta");
            entity.Property(e => e.PrecioPagado)
                .HasPrecision(10, 2)
                .HasColumnName("precio_pagado");
            entity.Property(e => e.QrToken)
                .HasMaxLength(500)
                .HasColumnName("qr_token");

            entity.HasOne(d => d.IdEstadoTicketNavigation).WithMany(p => p.Tickets)
                .HasForeignKey(d => d.IdEstadoTicket)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("TICKETS_ibfk_2");

            entity.HasOne(d => d.IdEventoAsientoNavigation).WithOne(p => p.Ticket)
                .HasForeignKey<Ticket>(d => d.IdEventoAsiento)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("TICKETS_ibfk_3");

            entity.HasOne(d => d.IdVentaNavigation).WithMany(p => p.Tickets)
                .HasForeignKey(d => d.IdVenta)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("TICKETS_ibfk_1");
        });

        modelBuilder.Entity<TipoEvento>(entity =>
        {
            entity.HasKey(e => e.IdTipoEvento).HasName("PRIMARY");

            entity.ToTable("TIPO_EVENTO");

            entity.HasIndex(e => e.NombreTipo, "nombre_tipo").IsUnique();

            entity.Property(e => e.IdTipoEvento).HasColumnName("id_tipo_evento");
            entity.Property(e => e.Activo)
                .HasDefaultValueSql("'1'")
                .HasColumnName("activo");
            entity.Property(e => e.NombreTipo)
                .HasMaxLength(100)
                .HasColumnName("nombre_tipo");
        });

        modelBuilder.Entity<TransaccionesPago>(entity =>
        {
            entity.HasKey(e => e.IdTransaccion).HasName("PRIMARY");

            entity.ToTable("TRANSACCIONES_PAGO");

            entity.HasIndex(e => e.IdVenta, "id_venta");

            entity.HasIndex(e => e.Estado, "idx_transacciones_estado");

            entity.HasIndex(e => e.IdTransaccionExt, "idx_transacciones_ext");

            entity.Property(e => e.IdTransaccion).HasColumnName("id_transaccion");
            entity.Property(e => e.Estado)
                .HasDefaultValueSql("'PENDING'")
                .HasColumnType("enum('PENDING','APPROVED','DECLINED','VOIDED','ERROR','REFUNDED')")
                .HasColumnName("estado");
            entity.Property(e => e.FechaActualizacion)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("fecha_actualizacion");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.IdTransaccionExt).HasColumnName("id_transaccion_ext");
            entity.Property(e => e.IdVenta).HasColumnName("id_venta");
            entity.Property(e => e.MetodoPago)
                .HasMaxLength(50)
                .HasColumnName("metodo_pago");
            entity.Property(e => e.Moneda)
                .HasMaxLength(10)
                .HasDefaultValueSql("'COP'")
                .HasColumnName("moneda");
            entity.Property(e => e.Monto)
                .HasPrecision(10, 2)
                .HasColumnName("monto");
            entity.Property(e => e.ProveedorPago)
                .HasDefaultValueSql("'WOMPI'")
                .HasColumnType("enum('WOMPI')")
                .HasColumnName("proveedor_pago");
            entity.Property(e => e.Referencia)
                .HasMaxLength(255)
                .HasColumnName("referencia");
            entity.Property(e => e.RespuestaJson)
                .HasColumnType("json")
                .HasColumnName("respuesta_json");

            entity.HasOne(d => d.IdVentaNavigation).WithMany(p => p.TransaccionesPagos)
                .HasForeignKey(d => d.IdVenta)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("TRANSACCIONES_PAGO_ibfk_1");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.IdUsuario).HasName("PRIMARY");

            entity.ToTable("USUARIO");

            entity.HasIndex(e => e.Email, "email").IsUnique();

            entity.HasIndex(e => e.GoogleId, "google_id").IsUnique();

            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
            entity.Property(e => e.Activo)
                .HasDefaultValueSql("'1'")
                .HasColumnName("activo");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");
            entity.Property(e => e.FechaRegistro)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("fecha_registro");
            entity.Property(e => e.FotoPerfil)
                .HasMaxLength(255)
                .HasColumnName("foto_perfil");
            entity.Property(e => e.GoogleId).HasColumnName("google_id");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .HasColumnName("nombre");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .HasColumnName("password_hash");
            entity.Property(e => e.Telefono)
                .HasMaxLength(20)
                .HasColumnName("telefono");
        });

        modelBuilder.Entity<Venta>(entity =>
        {
            entity.HasKey(e => e.IdVenta).HasName("PRIMARY");

            entity.ToTable("VENTAS");

            entity.HasIndex(e => e.IdStaff, "id_staff");

            entity.HasIndex(e => e.EstadoPago, "idx_ventas_estado");

            entity.HasIndex(e => e.FechaVenta, "idx_ventas_fecha");

            entity.HasIndex(e => e.IdUsuario, "idx_ventas_usuario");

            entity.HasIndex(e => e.ReferenciaInterna, "referencia_interna").IsUnique();

            entity.Property(e => e.IdVenta).HasColumnName("id_venta");
            entity.Property(e => e.EstadoPago)
                .HasDefaultValueSql("'PENDING'")
                .HasColumnType("enum('PENDING','APPROVED','DECLINED','VOIDED','ERROR','REFUNDED')")
                .HasColumnName("estado_pago");
            entity.Property(e => e.FechaPago)
                .HasColumnType("datetime")
                .HasColumnName("fecha_pago");
            entity.Property(e => e.FechaVenta)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("fecha_venta");
            entity.Property(e => e.IdStaff).HasColumnName("id_staff");
            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
            entity.Property(e => e.MetodoPago)
                .HasMaxLength(50)
                .HasColumnName("metodo_pago");
            entity.Property(e => e.Moneda)
                .HasMaxLength(10)
                .HasDefaultValueSql("'COP'")
                .HasColumnName("moneda");
            entity.Property(e => e.ReferenciaInterna).HasColumnName("referencia_interna");
            entity.Property(e => e.TipoVenta)
                .HasColumnType("enum('ONLINE','TAQUILLA')")
                .HasColumnName("tipo_venta");
            entity.Property(e => e.Total)
                .HasPrecision(10, 2)
                .HasColumnName("total");

            entity.HasOne(d => d.IdStaffNavigation).WithMany(p => p.Venta)
                .HasForeignKey(d => d.IdStaff)
                .HasConstraintName("VENTAS_ibfk_2");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.Venta)
                .HasForeignKey(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("VENTAS_ibfk_1");
        });

        modelBuilder.Entity<WebhookLog>(entity =>
        {
            entity.HasKey(e => e.IdLog).HasName("PRIMARY");

            entity.ToTable("WEBHOOK_LOGS");

            entity.Property(e => e.IdLog).HasColumnName("id_log");
            entity.Property(e => e.Error)
                .HasColumnType("text")
                .HasColumnName("error");
            entity.Property(e => e.Evento)
                .HasMaxLength(100)
                .HasColumnName("evento");
            entity.Property(e => e.Fecha)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("fecha");
            entity.Property(e => e.PayloadJson)
                .HasColumnType("json")
                .HasColumnName("payload_json");
            entity.Property(e => e.Procesado)
                .HasDefaultValueSql("'0'")
                .HasColumnName("procesado");
            entity.Property(e => e.Proveedor)
                .HasMaxLength(50)
                .HasColumnName("proveedor");
        });

        modelBuilder.Entity<Zona>(entity =>
        {
            entity.HasKey(e => e.IdZona).HasName("PRIMARY");

            entity.ToTable("ZONAS");

            entity.Property(e => e.IdZona).HasColumnName("id_zona");
            entity.Property(e => e.Activo)
                .HasDefaultValueSql("'1'")
                .HasColumnName("activo");
            entity.Property(e => e.ColorHex)
                .HasMaxLength(10)
                .HasColumnName("color_hex");
            entity.Property(e => e.NombreZona)
                .HasMaxLength(100)
                .HasColumnName("nombre_zona");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
