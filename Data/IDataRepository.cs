using System;
using System.Collections.Generic;
using EmpresaApp.Models;

namespace EmpresaApp.Data
{
    public interface IDataRepository
    {
        // ── Departamentos ─────────────────────────────────────
        List<Departamento> ObtenerDepartamentos();
        void AgregarDepartamento(Departamento d);
        void EliminarDepartamento(int id);

        // ── Usuarios ──────────────────────────────────────────
        List<Usuario> ObtenerUsuarios();
        Usuario? ObtenerUsuarioPorLogin(string login, string password);
        void AgregarUsuario(Usuario u);
        void ActualizarUsuario(Usuario u);
        void EliminarUsuario(string id);

        // ── Asistencia ────────────────────────────────────────
        List<RegistroAsistencia> ObtenerAsistencia();
        RegistroAsistencia? ObtenerRegistroAbierto(string usuarioId);
        void RegistrarEntrada(string usuarioId);
        void RegistrarSalida(string usuarioId);

        // ── Mensajes ──────────────────────────────────────────
        List<Mensaje> ObtenerMensajes();
        List<Mensaje> ObtenerMensajesDeUsuario(string usuarioId);
        int ContarMensajesNuevos(string usuarioId);
        void AgregarMensaje(Mensaje m);
        void MarcarMensajeLeido(string mensajeId);
        void MarcarTareaCompletada(string mensajeId);

        // ── Grafo ─────────────────────────────────────────────
        List<AristaGrafo> ObtenerGrafo();
        void ActualizarGrafo(string remitenteId, string destinatarioId);

        // ── Reportes ──────────────────────────────────────────
        List<ReporteDiario> ObtenerReportes();
        void AgregarReporte(ReporteDiario r);
        bool TieneReporteHoy(string usuarioId);

        // ── Notificaciones ────────────────────────────────────
        List<Notificacion> ObtenerNotificaciones();
        void AgregarNotificacion(Notificacion n);
        List<Notificacion> ObtenerNotificacionesParaAdmin();
        int ContarNotificacionesNoLeidas(string usuarioId);
        void MarcarNotificacionLeida(string id);
    }
}
