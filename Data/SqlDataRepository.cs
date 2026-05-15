// ═══════════════════════════════════════════════════════════════
//  SqlDataRepository.cs
//  Implementación 100 % SQL Server con Dapper.
//  Configurar la cadena de conexión en DbConfig.ConnectionString
//  antes de usar (ej. desde LoginForm o Program.cs).
// ═══════════════════════════════════════════════════════════════
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;
using Microsoft.Data.SqlClient;
using EmpresaApp.Models;

namespace EmpresaApp.Data
{
    public class SqlDataRepository : IDataRepository
    {
        private readonly string _connStr;
        private IDbConnection Conn() => new SqlConnection(_connStr);

        public SqlDataRepository(string connectionString)
        {
            _connStr = connectionString;
        }

        // ── Departamentos ─────────────────────────────────────
        public List<Departamento> ObtenerDepartamentos()
        {
            using var c = Conn();
            return c.Query<Departamento>(
                "SELECT Id, Nombre, Activo FROM Departamentos WHERE Activo = 1 ORDER BY Nombre")
                .ToList();
        }

        public void AgregarDepartamento(Departamento d)
        {
            using var c = Conn();
            c.Execute(
                "INSERT INTO Departamentos (Nombre, Activo) VALUES (@Nombre, 1)",
                new { d.Nombre });
        }

        public void EliminarDepartamento(int id)
        {
            using var c = Conn();
            c.Execute("UPDATE Departamentos SET Activo = 0 WHERE Id = @id", new { id });
        }

        // ── Usuarios ──────────────────────────────────────────
        public List<Usuario> ObtenerUsuarios()
        {
            using var c = Conn();
            return c.Query<Usuario>(
                @"SELECT Id, Nombre, Apellido, Email, Usuario_Login, Password,
                         Departamento, Cargo, Rol, Activo, FechaCreacion
                  FROM Usuarios
                  ORDER BY Departamento, Nombre").ToList();
        }

        public Usuario? ObtenerUsuarioPorLogin(string login, string password)
        {
            using var c = Conn();
            return c.QueryFirstOrDefault<Usuario>(
                @"SELECT Id, Nombre, Apellido, Email, Usuario_Login, Password,
                         Departamento, Cargo, Rol, Activo, FechaCreacion
                  FROM Usuarios
                  WHERE Usuario_Login = @login AND Password = @password AND Activo = 1",
                new { login, password });
        }

        public void AgregarUsuario(Usuario u)
        {
            using var c = Conn();
            c.Execute(
                @"INSERT INTO Usuarios
                    (Id, Nombre, Apellido, Email, Usuario_Login, Password,
                     Departamento, Cargo, Rol, Activo, FechaCreacion)
                  VALUES
                    (@Id, @Nombre, @Apellido, @Email, @Usuario_Login, @Password,
                     @Departamento, @Cargo, @Rol, @Activo, @FechaCreacion)",
                new
                {
                    u.Id, u.Nombre, u.Apellido, u.Email, u.Usuario_Login, u.Password,
                    u.Departamento, u.Cargo, Rol = (int)u.Rol, u.Activo, u.FechaCreacion
                });
        }

        public void ActualizarUsuario(Usuario u)
        {
            using var c = Conn();
            c.Execute(
                @"UPDATE Usuarios SET
                    Nombre        = @Nombre,
                    Apellido      = @Apellido,
                    Email         = @Email,
                    Usuario_Login = @Usuario_Login,
                    Password      = @Password,
                    Departamento  = @Departamento,
                    Cargo         = @Cargo,
                    Rol           = @Rol,
                    Activo        = @Activo
                  WHERE Id = @Id",
                new
                {
                    u.Nombre, u.Apellido, u.Email, u.Usuario_Login, u.Password,
                    u.Departamento, u.Cargo, Rol = (int)u.Rol, u.Activo, u.Id
                });
        }

        public void EliminarUsuario(string id)
        {
            using var c = Conn();
            c.Execute("UPDATE Usuarios SET Activo = 0 WHERE Id = @id", new { id });
        }

        // ── Asistencia ────────────────────────────────────────
        public List<RegistroAsistencia> ObtenerAsistencia()
        {
            using var c = Conn();
            return c.Query<RegistroAsistencia>(
                "SELECT Id, UsuarioId, HoraEntrada, HoraSalida FROM Asistencia ORDER BY HoraEntrada DESC")
                .ToList();
        }

        public RegistroAsistencia? ObtenerRegistroAbierto(string usuarioId)
        {
            using var c = Conn();
            return c.QueryFirstOrDefault<RegistroAsistencia>(
                @"SELECT Id, UsuarioId, HoraEntrada, HoraSalida
                  FROM Asistencia
                  WHERE UsuarioId = @usuarioId
                    AND CAST(HoraEntrada AS DATE) = CAST(GETDATE() AS DATE)
                    AND HoraSalida IS NULL",
                new { usuarioId });
        }

        public void RegistrarEntrada(string usuarioId)
        {
            using var c = Conn();
            c.Execute(
                "INSERT INTO Asistencia (Id, UsuarioId, HoraEntrada) VALUES (@Id, @usuarioId, GETDATE())",
                new { Id = Guid.NewGuid().ToString(), usuarioId });
        }

        public void RegistrarSalida(string usuarioId)
        {
            using var c = Conn();
            c.Execute(
                @"UPDATE Asistencia SET HoraSalida = GETDATE()
                  WHERE UsuarioId = @usuarioId
                    AND CAST(HoraEntrada AS DATE) = CAST(GETDATE() AS DATE)
                    AND HoraSalida IS NULL",
                new { usuarioId });
        }

        // ── Mensajes ──────────────────────────────────────────
        public List<Mensaje> ObtenerMensajes()
        {
            using var c = Conn();
            return c.Query<Mensaje>(
                @"SELECT Id, RemitenteId, RemitenteNombre, TipoDestino, DestinatarioId,
                         DestinatarioNombre, DepartamentoDestino, Asunto, Contenido,
                         Tipo, Estado, FechaEnvio, FechaVencimiento
                  FROM Mensajes ORDER BY FechaEnvio DESC").ToList();
        }

        public List<Mensaje> ObtenerMensajesDeUsuario(string usuarioId)
        {
            using var c = Conn();
            return c.Query<Mensaje>(
                @"SELECT m.Id, m.RemitenteId, m.RemitenteNombre, m.TipoDestino,
                         m.DestinatarioId, m.DestinatarioNombre, m.DepartamentoDestino,
                         m.Asunto, m.Contenido, m.Tipo, m.Estado, m.FechaEnvio, m.FechaVencimiento
                  FROM Mensajes m
                  LEFT JOIN Usuarios u ON u.Id = @usuarioId
                  WHERE (m.TipoDestino = 0 AND m.DestinatarioId = @usuarioId)
                     OR (m.TipoDestino = 1 AND m.DepartamentoDestino = u.Departamento)
                  ORDER BY m.FechaEnvio DESC",
                new { usuarioId }).ToList();
        }

        public int ContarMensajesNuevos(string usuarioId)
        {
            using var c = Conn();
            return c.ExecuteScalar<int>(
                @"SELECT COUNT(*)
                  FROM Mensajes m
                  LEFT JOIN Usuarios u ON u.Id = @usuarioId
                  WHERE m.Estado = 0
                    AND ((m.TipoDestino = 0 AND m.DestinatarioId = @usuarioId)
                      OR (m.TipoDestino = 1 AND m.DepartamentoDestino = u.Departamento))",
                new { usuarioId });
        }

        public void AgregarMensaje(Mensaje m)
        {
            using var c = Conn();
            c.Execute(
                @"INSERT INTO Mensajes
                    (Id, RemitenteId, RemitenteNombre, TipoDestino, DestinatarioId,
                     DestinatarioNombre, DepartamentoDestino, Asunto, Contenido,
                     Tipo, Estado, FechaEnvio, FechaVencimiento)
                  VALUES
                    (@Id, @RemitenteId, @RemitenteNombre, @TipoDestino, @DestinatarioId,
                     @DestinatarioNombre, @DepartamentoDestino, @Asunto, @Contenido,
                     @Tipo, @Estado, @FechaEnvio, @FechaVencimiento)",
                new
                {
                    m.Id, m.RemitenteId, m.RemitenteNombre,
                    TipoDestino    = (int)m.TipoDestino,
                    m.DestinatarioId, m.DestinatarioNombre, m.DepartamentoDestino,
                    m.Asunto, m.Contenido,
                    Tipo           = (int)m.Tipo,
                    Estado         = (int)m.Estado,
                    m.FechaEnvio, m.FechaVencimiento
                });

            if (m.TipoDestino == TipoDestino.Individual &&
                !string.IsNullOrEmpty(m.RemitenteId) &&
                !string.IsNullOrEmpty(m.DestinatarioId))
                ActualizarGrafo(m.RemitenteId, m.DestinatarioId);
        }

        public void MarcarMensajeLeido(string mensajeId)
        {
            using var c = Conn();
            c.Execute("UPDATE Mensajes SET Estado = 1 WHERE Id = @mensajeId AND Estado = 0",
                new { mensajeId });
        }

        public void MarcarTareaCompletada(string mensajeId)
        {
            using var c = Conn();
            c.Execute("UPDATE Mensajes SET Estado = 2 WHERE Id = @mensajeId", new { mensajeId });
        }

        // ── Grafo ─────────────────────────────────────────────
        public List<AristaGrafo> ObtenerGrafo()
        {
            using var c = Conn();
            return c.Query<AristaGrafo>(
                @"SELECT Id, RemitenteId, RemitenteNombre, RemitenteDepartamento,
                         DestinatarioId, DestinatarioNombre, DestinatarioDepartamento,
                         Peso, UltimaInteraccion
                  FROM MensajeriaGrafo
                  ORDER BY Peso DESC").ToList();
        }

        public void ActualizarGrafo(string remitenteId, string destinatarioId)
        {
            using var c = Conn();
            c.Execute("EXEC sp_ActualizarGrafo @remitenteId, @destinatarioId",
                new { remitenteId, destinatarioId });
        }

        // ── Reportes ──────────────────────────────────────────
        public List<ReporteDiario> ObtenerReportes()
        {
            using var c = Conn();
            return c.Query<ReporteDiario>(
                @"SELECT Id, UsuarioId, UsuarioNombre, Departamento, Fecha,
                         ActividadesRealizadas, LogrosDelDia, Pendientes,
                         Observaciones, NivelProductividad
                  FROM Reportes ORDER BY Fecha DESC").ToList();
        }

        public void AgregarReporte(ReporteDiario r)
        {
            using var c = Conn();
            c.Execute(
                @"INSERT INTO Reportes
                    (Id, UsuarioId, UsuarioNombre, Departamento, Fecha,
                     ActividadesRealizadas, LogrosDelDia, Pendientes,
                     Observaciones, NivelProductividad)
                  VALUES
                    (@Id, @UsuarioId, @UsuarioNombre, @Departamento, @Fecha,
                     @ActividadesRealizadas, @LogrosDelDia, @Pendientes,
                     @Observaciones, @NivelProductividad)", r);
        }

        public bool TieneReporteHoy(string usuarioId)
        {
            using var c = Conn();
            return c.ExecuteScalar<int>(
                @"SELECT COUNT(*) FROM Reportes
                  WHERE UsuarioId = @usuarioId
                    AND CAST(Fecha AS DATE) = CAST(GETDATE() AS DATE)",
                new { usuarioId }) > 0;
        }

        // ── Notificaciones ────────────────────────────────────
        public List<Notificacion> ObtenerNotificaciones()
        {
            using var c = Conn();
            return c.Query<Notificacion>(
                @"SELECT Id, RemitenteId, RemitenteNombre, RemitenteDepartamento,
                         Mensaje, Tipo, Fecha, Leida, DestinatarioId
                  FROM Notificaciones ORDER BY Fecha DESC").ToList();
        }

        public void AgregarNotificacion(Notificacion n)
        {
            using var c = Conn();
            c.Execute(
                @"INSERT INTO Notificaciones
                    (Id, RemitenteId, RemitenteNombre, RemitenteDepartamento,
                     Mensaje, Tipo, Fecha, Leida, DestinatarioId)
                  VALUES
                    (@Id, @RemitenteId, @RemitenteNombre, @RemitenteDepartamento,
                     @Mensaje, @Tipo, @Fecha, @Leida, @DestinatarioId)", n);
        }

        public List<Notificacion> ObtenerNotificacionesParaAdmin()
        {
            using var c = Conn();
            return c.Query<Notificacion>(
                @"SELECT Id, RemitenteId, RemitenteNombre, RemitenteDepartamento,
                         Mensaje, Tipo, Fecha, Leida, DestinatarioId
                  FROM Notificaciones
                  WHERE DestinatarioId = ''
                  ORDER BY Fecha DESC").ToList();
        }

        public int ContarNotificacionesNoLeidas(string usuarioId)
        {
            using var c = Conn();
            return c.ExecuteScalar<int>(
                @"SELECT COUNT(*) FROM Notificaciones
                  WHERE Leida = 0
                    AND (DestinatarioId = @usuarioId OR DestinatarioId = '')",
                new { usuarioId });
        }

        public void MarcarNotificacionLeida(string id)
        {
            using var c = Conn();
            c.Execute("UPDATE Notificaciones SET Leida = 1 WHERE Id = @id", new { id });
        }
    }
}
