using System;
using System.Windows.Forms;
using EmpresaApp.Data;

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

            // El LoginForm llama a DataManager.Inicializar(DbConfig.ConnectionString)
            // al hacer clic en "Iniciar Sesión", luego de validar credenciales.
            Application.Run(new EmpresaApp.Forms.LoginForm());
        }
    }
}
