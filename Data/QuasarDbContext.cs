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

    public virtual DbSet<Cache> Caches { get; set; }

    public virtual DbSet<CacheLock> CacheLocks { get; set; }

    public virtual DbSet<EstadoTicket> EstadoTickets { get; set; }

    public virtual DbSet<Evento> Eventos { get; set; }

    public virtual DbSet<EventoAsiento> EventoAsientos { get; set; }

    public virtual DbSet<EventoZona> EventoZonas { get; set; }

    public virtual DbSet<FailedJob> FailedJobs { get; set; }

    public virtual DbSet<Favorito> Favoritos { get; set; }

    public virtual DbSet<Job> Jobs { get; set; }

    public virtual DbSet<JobBatch> JobBatches { get; set; }

    public virtual DbSet<Migration> Migrations { get; set; }

    public virtual DbSet<Notificacione> Notificaciones { get; set; }

    public virtual DbSet<Passkey> Passkeys { get; set; }

    public virtual DbSet<PasswordResetToken> PasswordResetTokens { get; set; }

    public virtual DbSet<PersonalAccessToken> PersonalAccessTokens { get; set; }

    public virtual DbSet<Pqr> Pqrs { get; set; }

    public virtual DbSet<PqrsMensaje> PqrsMensajes { get; set; }

    public virtual DbSet<RolStaff> RolStaffs { get; set; }

    public virtual DbSet<Scan> Scans { get; set; }

    public virtual DbSet<ScanAlert> ScanAlerts { get; set; }

    public virtual DbSet<Session> Sessions { get; set; }

    public virtual DbSet<Staff> Staff { get; set; }

    public virtual DbSet<Ticket> Tickets { get; set; }

    public virtual DbSet<TipoEvento> TipoEventos { get; set; }

    public virtual DbSet<TransaccionesPago> TransaccionesPagos { get; set; }

    public virtual DbSet<User> Users { get; set; }

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

        modelBuilder.Entity<Cache>(entity =>
        {
            entity.HasKey(e => e.Key).HasName("PRIMARY");

            entity
                .ToTable("cache")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.Expiration, "cache_expiration_index");

            entity.Property(e => e.Key).HasColumnName("key");
            entity.Property(e => e.Expiration).HasColumnName("expiration");
            entity.Property(e => e.Value)
                .HasColumnType("mediumtext")
                .HasColumnName("value");
        });

        modelBuilder.Entity<CacheLock>(entity =>
        {
            entity.HasKey(e => e.Key).HasName("PRIMARY");

            entity
                .ToTable("cache_locks")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.Expiration, "cache_locks_expiration_index");

            entity.Property(e => e.Key).HasColumnName("key");
            entity.Property(e => e.Expiration).HasColumnName("expiration");
            entity.Property(e => e.Owner)
                .HasMaxLength(255)
                .HasColumnName("owner");
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
            entity.Property(e => e.RutaUrl)
                .HasMaxLength(255)
                .HasColumnName("ruta_url");

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

        modelBuilder.Entity<FailedJob>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("failed_jobs")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => new { e.Connection, e.Queue, e.FailedAt }, "failed_jobs_connection_queue_failed_at_index");

            entity.HasIndex(e => e.Uuid, "failed_jobs_uuid_unique").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Connection).HasColumnName("connection");
            entity.Property(e => e.Exception).HasColumnName("exception");
            entity.Property(e => e.FailedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("failed_at");
            entity.Property(e => e.Payload).HasColumnName("payload");
            entity.Property(e => e.Queue).HasColumnName("queue");
            entity.Property(e => e.Uuid).HasColumnName("uuid");
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
                .HasConstraintName("FAVORITOS_ibfk_1");
        });

        modelBuilder.Entity<Job>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("jobs")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.Queue, "jobs_queue_index");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Attempts).HasColumnName("attempts");
            entity.Property(e => e.AvailableAt).HasColumnName("available_at");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.Payload).HasColumnName("payload");
            entity.Property(e => e.Queue).HasColumnName("queue");
            entity.Property(e => e.ReservedAt).HasColumnName("reserved_at");
        });

        modelBuilder.Entity<JobBatch>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("job_batches")
                .UseCollation("utf8mb4_unicode_ci");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CancelledAt).HasColumnName("cancelled_at");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.FailedJobIds).HasColumnName("failed_job_ids");
            entity.Property(e => e.FailedJobs).HasColumnName("failed_jobs");
            entity.Property(e => e.FinishedAt).HasColumnName("finished_at");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.Options)
                .HasColumnType("mediumtext")
                .HasColumnName("options");
            entity.Property(e => e.PendingJobs).HasColumnName("pending_jobs");
            entity.Property(e => e.TotalJobs).HasColumnName("total_jobs");
        });

        modelBuilder.Entity<Migration>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("migrations")
                .UseCollation("utf8mb4_unicode_ci");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Batch).HasColumnName("batch");
            entity.Property(e => e.Migration1)
                .HasMaxLength(255)
                .HasColumnName("migration");
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
                .HasConstraintName("NOTIFICACIONES_ibfk_1");
        });

        modelBuilder.Entity<Passkey>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("passkeys")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.CredentialId, "passkeys_credential_id_unique").IsUnique();

            entity.HasIndex(e => e.UserId, "passkeys_user_id_index");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.Credential)
                .HasColumnType("json")
                .HasColumnName("credential");
            entity.Property(e => e.CredentialId).HasColumnName("credential_id");
            entity.Property(e => e.LastUsedAt)
                .HasColumnType("timestamp")
                .HasColumnName("last_used_at");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.Passkeys)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("passkeys_user_id_foreign");
        });

        modelBuilder.Entity<PasswordResetToken>(entity =>
        {
            entity.HasKey(e => e.Email).HasName("PRIMARY");

            entity
                .ToTable("password_reset_tokens")
                .UseCollation("utf8mb4_unicode_ci");

            entity.Property(e => e.Email).HasColumnName("email");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.Token)
                .HasMaxLength(255)
                .HasColumnName("token");
        });

        modelBuilder.Entity<PersonalAccessToken>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("personal_access_tokens")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.ExpiresAt, "personal_access_tokens_expires_at_index");

            entity.HasIndex(e => e.Token, "personal_access_tokens_token_unique").IsUnique();

            entity.HasIndex(e => new { e.TokenableType, e.TokenableId }, "personal_access_tokens_tokenable_type_tokenable_id_index");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Abilities)
                .HasColumnType("text")
                .HasColumnName("abilities");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.ExpiresAt)
                .HasColumnType("timestamp")
                .HasColumnName("expires_at");
            entity.Property(e => e.LastUsedAt)
                .HasColumnType("timestamp")
                .HasColumnName("last_used_at");
            entity.Property(e => e.Name)
                .HasColumnType("text")
                .HasColumnName("name");
            entity.Property(e => e.Token)
                .HasMaxLength(64)
                .HasColumnName("token");
            entity.Property(e => e.TokenableId).HasColumnName("tokenable_id");
            entity.Property(e => e.TokenableType).HasColumnName("tokenable_type");
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");
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
            entity.Property(e => e.Dispositivo)
                .HasMaxLength(255)
                .HasColumnName("dispositivo");
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
                .HasConstraintName("SCAN_ibfk_1");
        });

        modelBuilder.Entity<ScanAlert>(entity =>
        {
            entity.HasKey(e => e.IdScanAlert).HasName("PRIMARY");

            entity.ToTable("SCAN_ALERTS");

            entity.HasIndex(e => e.IdScan, "idx_scan_alerts_scan");

            entity.HasIndex(e => e.IdStaff, "idx_scan_alerts_staff");

            entity.HasIndex(e => e.IdTicket, "idx_scan_alerts_ticket");

            entity.Property(e => e.IdScanAlert).HasColumnName("id_scan_alert");
            entity.Property(e => e.Detalle)
                .HasColumnType("text")
                .HasColumnName("detalle");
            entity.Property(e => e.Dispositivo)
                .HasMaxLength(255)
                .HasColumnName("dispositivo");
            entity.Property(e => e.FechaAlerta)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("fecha_alerta");
            entity.Property(e => e.IdScan).HasColumnName("id_scan");
            entity.Property(e => e.IdStaff).HasColumnName("id_staff");
            entity.Property(e => e.IdTicket).HasColumnName("id_ticket");
            entity.Property(e => e.PayloadJson)
                .HasColumnType("json")
                .HasColumnName("payload_json");
            entity.Property(e => e.QrToken)
                .HasMaxLength(500)
                .HasColumnName("qr_token");
            entity.Property(e => e.TipoAlerta)
                .HasMaxLength(100)
                .HasColumnName("tipo_alerta");

            entity.HasOne(d => d.IdScanNavigation).WithMany(p => p.ScanAlerts)
                .HasForeignKey(d => d.IdScan)
                .HasConstraintName("SCAN_ALERTS_ibfk_1");

            entity.HasOne(d => d.IdStaffNavigation).WithMany(p => p.ScanAlerts)
                .HasForeignKey(d => d.IdStaff)
                .HasConstraintName("SCAN_ALERTS_ibfk_3");

            entity.HasOne(d => d.IdTicketNavigation).WithMany(p => p.ScanAlerts)
                .HasForeignKey(d => d.IdTicket)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("SCAN_ALERTS_ibfk_2");
        });

        modelBuilder.Entity<Session>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("sessions")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.LastActivity, "sessions_last_activity_index");

            entity.HasIndex(e => e.UserId, "sessions_user_id_index");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.IpAddress)
                .HasMaxLength(45)
                .HasColumnName("ip_address");
            entity.Property(e => e.LastActivity).HasColumnName("last_activity");
            entity.Property(e => e.Payload).HasColumnName("payload");
            entity.Property(e => e.UserAgent)
                .HasColumnType("text")
                .HasColumnName("user_agent");
            entity.Property(e => e.UserId).HasColumnName("user_id");
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

            entity.HasIndex(e => e.IdVenta, "TICKETS_ibfk_1");

            entity.HasIndex(e => e.CodigoUnico, "codigo_unico").IsUnique();

            entity.HasIndex(e => e.IdEstadoTicket, "id_estado_ticket");

            entity.HasIndex(e => e.IdEventoAsiento, "id_evento_asiento").IsUnique();

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

            entity.HasIndex(e => e.IdVenta, "TRANSACCIONES_PAGO_ibfk_1");

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
                .HasConstraintName("TRANSACCIONES_PAGO_ibfk_1");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("users")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.Email, "users_email_unique").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.CurrentTeamId).HasColumnName("current_team_id");
            entity.Property(e => e.Email).HasColumnName("email");
            entity.Property(e => e.EmailVerifiedAt)
                .HasColumnType("timestamp")
                .HasColumnName("email_verified_at");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.Password)
                .HasMaxLength(255)
                .HasColumnName("password");
            entity.Property(e => e.ProfilePhotoPath)
                .HasMaxLength(2048)
                .HasColumnName("profile_photo_path");
            entity.Property(e => e.RememberToken)
                .HasMaxLength(100)
                .HasColumnName("remember_token");
            entity.Property(e => e.TwoFactorConfirmedAt)
                .HasColumnType("timestamp")
                .HasColumnName("two_factor_confirmed_at");
            entity.Property(e => e.TwoFactorRecoveryCodes)
                .HasColumnType("text")
                .HasColumnName("two_factor_recovery_codes");
            entity.Property(e => e.TwoFactorSecret)
                .HasColumnType("text")
                .HasColumnName("two_factor_secret");
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");
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
