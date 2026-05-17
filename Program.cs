using System;
using System.Windows.Forms;
using EmpresaApp.Data;
using EmpresaApp.Forms;

namespace EmpresaApp
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // ── Paso 1: Cargar servidor guardado o pedir configuración ──
            string? connGuardada = ServerConfig.CargarServidor();

            if (connGuardada != null)
            {
                DbConfig.ConnectionString = connGuardada;

                // Verificar que la conexión siga siendo válida
                if (!ConexionValida(connGuardada))
                {
                    connGuardada = null;
                    ServerConfig.EliminarServidor();
                }
            }

            if (connGuardada == null)
            {
                var servidorForm = new ServidorForm();
                var result = servidorForm.ShowDialog();

                if (result != DialogResult.OK || !servidorForm.ServidorConfigurado)
                    return; // usuario cerró sin configurar
            }

            // ── Paso 2: Login normal ────────────────────────────────────
            Application.Run(new LoginForm());
        }

        private static bool ConexionValida(string connStr)
        {
            try
            {
                using var conn = new Microsoft.Data.SqlClient.SqlConnection(connStr);
                conn.Open();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME='Usuarios'";
                return (int)cmd.ExecuteScalar()! > 0;
            }
            catch { return false; }
        }
    }
}
