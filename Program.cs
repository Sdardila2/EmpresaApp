using System;
using System.Windows.Forms;
using EmpresaApp.Data;
using EmpresaApp.Forms;
using EmpresaApp.Services;

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
                // Only validate if there is actually a saved connection string
                var lanValidator = LANNetworkValidator.Instance;

                if (!ConexionValida(connGuardada) || !lanValidator.ValidateDatabaseServer(connGuardada))
                {
                    connGuardada = null;
                    ServerConfig.EliminarServidor();
                    MessageBox.Show(
                        "La conexión guardada ya no es válida o el servidor no está en la LAN.\n" +
                        "Por favor configura el servidor nuevamente.",
                        "Conexión perdida", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    DbConfig.ConnectionString = connGuardada;
                }
            }

            if (connGuardada == null)
            {
                // No saved config — go straight to setup, no connection attempted
                var servidorForm = new ServidorForm();
                var result = servidorForm.ShowDialog();

                if (result != DialogResult.OK || !servidorForm.ServidorConfigurado)
                    return; // usuario cerró sin configurar
            }

            // ── Paso 2: Inicializar DataManager y SyncService ──
            DataManager.Inicializar(DbConfig.ConnectionString);

            // ── Paso 3: Login normal ────────────────────────────────────
            var loginForm = new LoginFormModern();
            if (loginForm.ShowDialog() == DialogResult.OK)
            {
                Application.Run(new DashboardFormModern());
            }
        }

        private static bool ConexionValida(string connStr)
        {
            try
            {
                using var conn = new Microsoft.Data.SqlClient.SqlConnection(connStr);
                conn.Open();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME='Usuarios'";
                cmd.CommandTimeout = 5;
                return (int)cmd.ExecuteScalar()! > 0;
            }
            catch { return false; }
        }
    }
}