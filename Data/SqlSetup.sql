-- ═══════════════════════════════════════════════════════════════
--  EmpresaApp — Script de creación SQL Server (v3 - corregido)
-- ═══════════════════════════════════════════════════════════════

-- ── Departamentos ─────────────────────────────────────────────
CREATE TABLE [dbo].[Departamentos] (
    [Id]     INT           NOT NULL IDENTITY(1,1) PRIMARY KEY,
    [Nombre] NVARCHAR(100) NOT NULL,
    [Activo] BIT           NOT NULL DEFAULT 1,
    CONSTRAINT UQ_Departamento_Nombre UNIQUE ([Nombre])
);

INSERT INTO [Departamentos] (Nombre) VALUES
    ('Sistemas'), ('Recursos Humanos'), ('Ventas'),
    ('Finanzas'), ('Operaciones'), ('Marketing'), ('Logística');

-- ── Usuarios ──────────────────────────────────────────────────
CREATE TABLE [dbo].[Usuarios] (
    [Id]            NVARCHAR(36)  NOT NULL PRIMARY KEY DEFAULT NEWID(),
    [Nombre]        NVARCHAR(100) NOT NULL,
    [Apellido]      NVARCHAR(100) NOT NULL DEFAULT '',
    [Email]         NVARCHAR(200) NOT NULL DEFAULT '',
    [Usuario_Login] NVARCHAR(100) NOT NULL,
    [Password]      NVARCHAR(256) NOT NULL,
    [Departamento]  NVARCHAR(100) NOT NULL DEFAULT '',
    [Cargo]         NVARCHAR(100) NOT NULL DEFAULT '',
    [Rol]           TINYINT       NOT NULL DEFAULT 1,  -- 0=Admin, 1=Empleado
    [Activo]        BIT           NOT NULL DEFAULT 1,
    [FechaCreacion] DATETIME2     NOT NULL DEFAULT GETDATE(),
    CONSTRAINT UQ_Usuario_Login UNIQUE ([Usuario_Login])
);
CREATE INDEX IX_Usuarios_Departamento ON [dbo].[Usuarios] ([Departamento]);

INSERT INTO [Usuarios]
    (Id, Nombre, Apellido, Email, Usuario_Login, Password, Departamento, Cargo, Rol)
VALUES
    (NEWID(), 'Admin', 'Sistema', 'admin@empresa.com',
     'admin', 'admin123', 'Sistemas', 'Administrador General', 0);

-- ── Asistencia ────────────────────────────────────────────────
CREATE TABLE [dbo].[Asistencia] (
    [Id]          NVARCHAR(36) NOT NULL PRIMARY KEY DEFAULT NEWID(),
    [UsuarioId]   NVARCHAR(36) NOT NULL REFERENCES [Usuarios]([Id]),
    [HoraEntrada] DATETIME2    NOT NULL DEFAULT GETDATE(),
    [HoraSalida]  DATETIME2    NULL
);
CREATE INDEX IX_Asistencia_Usuario_Fecha
    ON [dbo].[Asistencia] ([UsuarioId], [HoraEntrada]);

-- ── Mensajes ──────────────────────────────────────────────────
CREATE TABLE [dbo].[Mensajes] (
    [Id]                   NVARCHAR(36)  NOT NULL PRIMARY KEY DEFAULT NEWID(),
    [RemitenteId]          NVARCHAR(36)  NOT NULL REFERENCES [Usuarios]([Id]),
    [RemitenteNombre]      NVARCHAR(200) NOT NULL,
    [TipoDestino]          TINYINT       NOT NULL DEFAULT 0, -- 0=Individual, 1=Depto
    [DestinatarioId]       NVARCHAR(36)  NOT NULL,
    [DestinatarioNombre]   NVARCHAR(200) NOT NULL DEFAULT '',
    [DepartamentoDestino]  NVARCHAR(100) NULL,
    [Asunto]               NVARCHAR(500) NOT NULL,
    [Contenido]            NVARCHAR(MAX) NOT NULL,
    [Tipo]                 TINYINT       NOT NULL DEFAULT 0, -- 0=Mensaje, 1=Tarea, 2=Alerta
    [Estado]               TINYINT       NOT NULL DEFAULT 0, -- 0=Nuevo, 1=Leido, 2=Completado
    [FechaEnvio]           DATETIME2     NOT NULL DEFAULT GETDATE(),
    [FechaVencimiento]     DATETIME2     NULL
);
CREATE INDEX IX_Mensajes_Destinatario
    ON [dbo].[Mensajes] ([DestinatarioId], [Estado]);
CREATE INDEX IX_Mensajes_DeptDestino
    ON [dbo].[Mensajes] ([DepartamentoDestino], [TipoDestino], [Estado]);

-- ── Grafo dirigido ponderado ──────────────────────────────────
CREATE TABLE [dbo].[MensajeriaGrafo] (
    [Id]                       NVARCHAR(36)  NOT NULL PRIMARY KEY DEFAULT NEWID(),
    [RemitenteId]              NVARCHAR(36)  NOT NULL REFERENCES [Usuarios]([Id]),
    [RemitenteNombre]          NVARCHAR(200) NOT NULL,
    [RemitenteDepartamento]    NVARCHAR(100) NOT NULL DEFAULT '',
    [DestinatarioId]           NVARCHAR(36)  NOT NULL REFERENCES [Usuarios]([Id]),
    [DestinatarioNombre]       NVARCHAR(200) NOT NULL,
    [DestinatarioDepartamento] NVARCHAR(100) NOT NULL DEFAULT '',
    [Peso]                     INT           NOT NULL DEFAULT 1,
    [UltimaInteraccion]        DATETIME2     NOT NULL DEFAULT GETDATE(),
    CONSTRAINT UQ_Arista UNIQUE ([RemitenteId], [DestinatarioId])
);

-- ── Reportes ──────────────────────────────────────────────────
CREATE TABLE [dbo].[Reportes] (
    [Id]                    NVARCHAR(36)  NOT NULL PRIMARY KEY DEFAULT NEWID(),
    [UsuarioId]             NVARCHAR(36)  NOT NULL REFERENCES [Usuarios]([Id]),
    [UsuarioNombre]         NVARCHAR(200) NOT NULL,
    [Departamento]          NVARCHAR(100) NOT NULL DEFAULT '',
    [Fecha]                 DATETIME2     NOT NULL DEFAULT GETDATE(),
    [ActividadesRealizadas] NVARCHAR(MAX) NOT NULL,
    [LogrosDelDia]          NVARCHAR(MAX) NOT NULL,
    [Pendientes]            NVARCHAR(MAX) NOT NULL DEFAULT '',
    [Observaciones]         NVARCHAR(MAX) NOT NULL DEFAULT '',
    [NivelProductividad]    TINYINT       NOT NULL DEFAULT 3
);
CREATE INDEX IX_Reportes_Usuario_Fecha
    ON [dbo].[Reportes] ([UsuarioId], [Fecha]);

-- ── Notificaciones ────────────────────────────────────────────
CREATE TABLE [dbo].[Notificaciones] (
    [Id]                    NVARCHAR(36)  NOT NULL PRIMARY KEY DEFAULT NEWID(),
    [RemitenteId]           NVARCHAR(36)  NOT NULL,
    [RemitenteNombre]       NVARCHAR(200) NOT NULL,
    [RemitenteDepartamento] NVARCHAR(100) NOT NULL DEFAULT '',
    [Mensaje]               NVARCHAR(MAX) NOT NULL,
    [Tipo]                  NVARCHAR(20)  NOT NULL DEFAULT 'Info',
    [Fecha]                 DATETIME2     NOT NULL DEFAULT GETDATE(),
    [Leida]                 BIT           NOT NULL DEFAULT 0,
    [DestinatarioId]        NVARCHAR(36)  NOT NULL DEFAULT '' -- ''=todos los admins; GUID=usuario específico
);
CREATE INDEX IX_Notificaciones_Destinatario
    ON [dbo].[Notificaciones] ([DestinatarioId], [Leida]);

-- ── SP al final: Usuarios y MensajeriaGrafo ya existen ────────
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_ActualizarGrafo]
    @remitenteId    NVARCHAR(36),
    @destinatarioId NVARCHAR(36)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @remNombre  NVARCHAR(200), @remDepto   NVARCHAR(100),
            @destNombre NVARCHAR(200), @destDepto  NVARCHAR(100);

    SELECT @remNombre = Nombre + ' ' + Apellido,
           @remDepto  = Departamento
    FROM   Usuarios WHERE Id = @remitenteId;

    SELECT @destNombre = Nombre + ' ' + Apellido,
           @destDepto  = Departamento
    FROM   Usuarios WHERE Id = @destinatarioId;

    IF EXISTS (
        SELECT 1 FROM MensajeriaGrafo
        WHERE RemitenteId = @remitenteId AND DestinatarioId = @destinatarioId)
    BEGIN
        UPDATE MensajeriaGrafo
        SET Peso = Peso + 1, UltimaInteraccion = GETDATE()
        WHERE RemitenteId = @remitenteId AND DestinatarioId = @destinatarioId;
    END
    ELSE
    BEGIN
        INSERT INTO MensajeriaGrafo
            (RemitenteId, RemitenteNombre, RemitenteDepartamento,
             DestinatarioId, DestinatarioNombre, DestinatarioDepartamento, Peso)
        VALUES
            (@remitenteId, @remNombre, ISNULL(@remDepto, ''),
             @destinatarioId, @destNombre, ISNULL(@destDepto, ''), 1);
    END
END;
GO