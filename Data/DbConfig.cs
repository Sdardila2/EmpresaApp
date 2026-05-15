// ═══════════════════════════════════════════════════════════════
//  DbConfig.cs
//  Edita ConnectionString antes de compilar, o cámbialo en tiempo
//  de ejecución desde la pantalla de configuración del LoginForm.
// ═══════════════════════════════════════════════════════════════
namespace EmpresaApp.Data
{
    public static class DbConfig
    {
        /// <summary>
        /// Cadena de conexión a SQL Server.
        /// Ejemplos:
        ///   Instancia local:     "Server=.;Database=EmpresaApp;Trusted_Connection=True;TrustServerCertificate=True;"
        ///   Con usuario/pass:    "Server=MI_SERVIDOR;Database=EmpresaApp;User Id=sa;Password=MiPass;TrustServerCertificate=True;"
        ///   LocalDB (VS):        "Server=(localdb)\\mssqllocaldb;Database=EmpresaApp;Trusted_Connection=True;"
        /// </summary>
        public static string ConnectionString { get; set; } =
            "Server=(localdb)\\Servidor;Database=EmpresaApp;Trusted_Connection=True;";
    }
}
