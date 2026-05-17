// ═══════════════════════════════════════════════════════════════
//  DataManager.cs  —  Fachada estática sobre IDataRepository.
//  Incluye integración con SyncService para real-time updates.
//  Inicializar en Program.cs:
//    DataManager.Inicializar("Server=...;Database=EmpresaApp;...");
// ═══════════════════════════════════════════════════════════════
using System.Collections.Generic;
using EmpresaApp.Models;
using EmpresaApp.Services;

namespace EmpresaApp.Data
{
    public static class DataManager
    {
        private static IDataRepository? _repo;
        private static SyncService? _syncService;

        public static void Inicializar(string connectionString)
        {
            _repo = new SqlDataRepository(connectionString);
            _syncService = SyncService.Instance;
        }

        private static IDataRepository R =>
            _repo ?? throw new System.InvalidOperationException(
                "DataManager no está inicializado. Llame a DataManager.Inicializar(connStr) primero.");

        public static SyncService Sync => _syncService ?? SyncService.Instance;

        // ── Departamentos ─────────────────────────────────────
        public static List<Departamento> ObtenerDepartamentos()           => R.ObtenerDepartamentos();
        public static void AgregarDepartamento(Departamento d)            => R.AgregarDepartamento(d);
        public static void EliminarDepartamento(int id)                   => R.EliminarDepartamento(id);

        // ── Usuarios ──────────────────────────────────────────
        public static List<Usuario> ObtenerUsuarios()                     => R.ObtenerUsuarios();
        public static Usuario? ObtenerUsuarioPorLogin(string l, string p) => R.ObtenerUsuarioPorLogin(l, p);
        public static void AgregarUsuario(Usuario u)                      => R.AgregarUsuario(u);
        public static void ActualizarUsuario(Usuario u)                   => R.ActualizarUsuario(u);
        public static void EliminarUsuario(string id)                     => R.EliminarUsuario(id);

        // ── Asistencia ────────────────────────────────────────
        public static List<RegistroAsistencia> ObtenerAsistencia()        => R.ObtenerAsistencia();
        public static RegistroAsistencia? ObtenerRegistroAbierto(string u)=> R.ObtenerRegistroAbierto(u);
        public static void RegistrarEntrada(string uid)
        {
            R.RegistrarEntrada(uid);
            var registro = R.ObtenerRegistroAbierto(uid);
            if (registro != null)
                _syncService?.NotifyAsistenciaEntrada(registro);
        }
        public static void RegistrarSalida(string uid)
        {
            R.RegistrarSalida(uid);
            var registros = R.ObtenerAsistencia()
                .Where(r => r.UsuarioId == uid && r.HoraEntrada.Date == System.DateTime.Today)
                .FirstOrDefault();
            if (registros != null)
                _syncService?.NotifyAsistenciaSalida(registros);
        }

        // ── Mensajes ──────────────────────────────────────────
        public static List<Mensaje> ObtenerMensajes()                     => R.ObtenerMensajes();
        public static List<Mensaje> ObtenerMensajesDeUsuario(string uid)  => R.ObtenerMensajesDeUsuario(uid);
        public static int ContarMensajesNuevos(string uid)                => R.ContarMensajesNuevos(uid);
        public static void AgregarMensaje(Mensaje m)
        {
            R.AgregarMensaje(m);
            _syncService?.NotifyMensajeNuevo(m);
        }
        public static void MarcarMensajeLeido(string id)
        {
            R.MarcarMensajeLeido(id);
            _syncService?.NotifyMensajeLeido(id);
        }
        public static void MarcarTareaCompletada(string id)
        {
            R.MarcarTareaCompletada(id);
            _syncService?.NotifyTareaCompletada(id);
        }

        // ── Grafo ─────────────────────────────────────────────
        public static List<AristaGrafo> ObtenerGrafo()                    => R.ObtenerGrafo();

        // ── Reportes ──────────────────────────────────────────
        public static List<ReporteDiario> ObtenerReportes()               => R.ObtenerReportes();
        public static void AgregarReporte(ReporteDiario r)
        {
            R.AgregarReporte(r);
            _syncService?.NotifyReporteEnviado(r);
        }
        public static bool TieneReporteHoy(string uid)                    => R.TieneReporteHoy(uid);

        // ── Notificaciones ────────────────────────────────────
        public static List<Notificacion> ObtenerNotificaciones()          => R.ObtenerNotificaciones();
        public static void AgregarNotificacion(Notificacion n)
        {
            R.AgregarNotificacion(n);
            _syncService?.NotifyNotificacionNueva(n);
        }
        public static List<Notificacion> ObtenerNotificacionesParaAdmin() => R.ObtenerNotificacionesParaAdmin();
        public static int ContarNotificacionesNoLeidas(string uid)        => R.ContarNotificacionesNoLeidas(uid);
        public static void MarcarNotificacionLeida(string id)             => R.MarcarNotificacionLeida(id);
    }
}
