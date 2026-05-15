using EmpresaApp.Models;

namespace EmpresaApp.Utils
{
    public static class Session
    {
        public static Usuario? UsuarioActual { get; set; }
        public static bool EsAdmin => UsuarioActual?.Rol == UserRole.Administrador;

        public static void Cerrar()
        {
            UsuarioActual = null;
        }
    }

    public static class Colores
    {
        public static System.Drawing.Color Primario      = System.Drawing.Color.FromArgb(26,  54,  93);
        public static System.Drawing.Color Secundario    = System.Drawing.Color.FromArgb(37,  99,  235);
        public static System.Drawing.Color Acento        = System.Drawing.Color.FromArgb(16,  185, 129);
        public static System.Drawing.Color Alerta        = System.Drawing.Color.FromArgb(239, 68,  68);
        public static System.Drawing.Color Advertencia   = System.Drawing.Color.FromArgb(245, 158, 11);
        public static System.Drawing.Color Fondo         = System.Drawing.Color.FromArgb(241, 245, 249);
        public static System.Drawing.Color FondoPanel    = System.Drawing.Color.White;
        public static System.Drawing.Color TextoPrimario = System.Drawing.Color.FromArgb(15,  23,  42);
        public static System.Drawing.Color TextoSecundario= System.Drawing.Color.FromArgb(100, 116, 139);
    }
}
