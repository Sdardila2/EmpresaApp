using System;

namespace EmpresaApp.Models
{
    public enum UserRole { Administrador, Empleado }

    // ─────────────────────────────────────────────
    //  DEPARTAMENTO
    // ─────────────────────────────────────────────
    public class Departamento
    {
        public int    Id     { get; set; }
        public string Nombre { get; set; } = "";
        public bool   Activo { get; set; } = true;
    }

    // ─────────────────────────────────────────────
    //  USUARIO
    // ─────────────────────────────────────────────
    public class Usuario
    {
        public string   Id              { get; set; } = Guid.NewGuid().ToString();
        public string   Nombre          { get; set; } = "";
        public string   Apellido        { get; set; } = "";
        public string   Email           { get; set; } = "";
        public string   Usuario_Login   { get; set; } = "";
        public string   Password        { get; set; } = "";
        public string   Departamento    { get; set; } = "";
        public string   Cargo           { get; set; } = "";
        public UserRole Rol             { get; set; } = UserRole.Empleado;
        public bool     Activo          { get; set; } = true;
        public DateTime FechaCreacion   { get; set; } = DateTime.Now;
        public string   NombreCompleto  => $"{Nombre} {Apellido}";
    }

    // ─────────────────────────────────────────────
    //  ASISTENCIA
    // ─────────────────────────────────────────────
    public class RegistroAsistencia
    {
        public string    Id           { get; set; } = Guid.NewGuid().ToString();
        public string    UsuarioId    { get; set; } = "";
        public DateTime  HoraEntrada  { get; set; }
        public DateTime? HoraSalida   { get; set; }
        public string    Fecha        => HoraEntrada.ToString("yyyy-MM-dd");
        public string    TiempoTrabajado
        {
            get
            {
                if (!HoraSalida.HasValue) return "En curso";
                var d = HoraSalida.Value - HoraEntrada;
                return $"{(int)d.TotalHours}h {d.Minutes}m";
            }
        }
    }

    // ─────────────────────────────────────────────
    //  MENSAJERÍA
    // ─────────────────────────────────────────────
    public enum TipoMensaje   { Mensaje, Tarea, Alerta }
    public enum EstadoMensaje { Nuevo, Leido, Completado }
    public enum TipoDestino   { Individual, Departamento }

    public class Mensaje
    {
        public string       Id                  { get; set; } = Guid.NewGuid().ToString();
        public string       RemitenteId         { get; set; } = "";
        public string       RemitenteNombre     { get; set; } = "";
        public TipoDestino  TipoDestino         { get; set; } = TipoDestino.Individual;
        public string       DestinatarioId      { get; set; } = "";
        public string       DestinatarioNombre  { get; set; } = "";
        public string?      DepartamentoDestino { get; set; }
        public string       Asunto              { get; set; } = "";
        public string       Contenido           { get; set; } = "";
        public TipoMensaje  Tipo                { get; set; } = TipoMensaje.Mensaje;
        public EstadoMensaje Estado             { get; set; } = EstadoMensaje.Nuevo;
        public DateTime     FechaEnvio          { get; set; } = DateTime.Now;
        public DateTime?    FechaVencimiento    { get; set; }
    }

    // ─────────────────────────────────────────────
    //  GRAFO DIRIGIDO PONDERADO
    // ─────────────────────────────────────────────
    public class AristaGrafo
    {
        public string   Id                       { get; set; } = Guid.NewGuid().ToString();
        public string   RemitenteId              { get; set; } = "";
        public string   RemitenteNombre          { get; set; } = "";
        public string   RemitenteDepartamento    { get; set; } = "";
        public string   DestinatarioId           { get; set; } = "";
        public string   DestinatarioNombre       { get; set; } = "";
        public string   DestinatarioDepartamento { get; set; } = "";
        public int      Peso                     { get; set; } = 0;
        public DateTime UltimaInteraccion        { get; set; } = DateTime.Now;
    }

    // ─────────────────────────────────────────────
    //  REPORTE DIARIO
    // ─────────────────────────────────────────────
    public class ReporteDiario
    {
        public string   Id                    { get; set; } = Guid.NewGuid().ToString();
        public string   UsuarioId             { get; set; } = "";
        public string   UsuarioNombre         { get; set; } = "";
        public string   Departamento          { get; set; } = "";
        public DateTime Fecha                 { get; set; } = DateTime.Now;
        public string   ActividadesRealizadas { get; set; } = "";
        public string   LogrosDelDia          { get; set; } = "";
        public string   Pendientes            { get; set; } = "";
        public string   Observaciones         { get; set; } = "";
        public int      NivelProductividad    { get; set; } = 3;
    }

    // ─────────────────────────────────────────────
    //  NOTIFICACIÓN
    // ─────────────────────────────────────────────
    public class Notificacion
    {
        public string   Id                    { get; set; } = Guid.NewGuid().ToString();
        public string   RemitenteId           { get; set; } = "";
        public string   RemitenteNombre       { get; set; } = "";
        public string   RemitenteDepartamento { get; set; } = "";
        public string   Mensaje               { get; set; } = "";
        public string   Tipo                  { get; set; } = "Info";
        public DateTime Fecha                 { get; set; } = DateTime.Now;
        public bool     Leida                 { get; set; } = false;
        public string   DestinatarioId        { get; set; } = "";
    }
}
