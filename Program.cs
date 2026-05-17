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
                string? error = ValidarConexionGuardada(connGuardada);
                if (error != null)
                {
                    connGuardada = null;
                    ServerConfig.EliminarServidor();
                    MessageBox.Show(
                        error + "\n\nConfigure el servidor de nuevo.",
                        "Conexion", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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

            var loginForm = new LoginForm();
            if (loginForm.ShowDialog() == DialogResult.OK)
                Application.Run(new DashboardForm());
        }

        /// <summary>
        /// null si la conexion es valida; mensaje de error si no.
        /// </summary>
        private static string? ValidarConexionGuardada(string connStr)
        {
            try
            {
                using var conn = new Microsoft.Data.SqlClient.SqlConnection(connStr);
                conn.Open();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME='Usuarios'";
                cmd.CommandTimeout = 10;
                if ((int)cmd.ExecuteScalar()! == 0)
                    return "La base de datos no tiene el esquema de EmpresaApp (tabla Usuarios).";

                var validator = LANNetworkValidator.Instance;
                var builder = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(connStr);
                if (!validator.IsLocalSqlServer(builder.DataSource ?? ""))
                {
                    var ip = validator.ExtractServerIP(connStr);
                    if (!validator.IsIPOnLAN(ip))
                        return "El servidor no esta en una red local permitida (" + ip + ").";
                }

                return null;
            }
            catch (Exception ex)
            {
                return "No se pudo conectar a SQL Server: " + ex.Message;
            }
        }
    }
}