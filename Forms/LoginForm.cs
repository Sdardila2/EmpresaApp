using System;
using System.Drawing;
using System.Windows.Forms;
using EmpresaApp.Data;
using EmpresaApp.Utils;

namespace EmpresaApp.Forms
{
    public class LoginForm : Form
    {
        private TextBox txtUsuario  = null!;
        private TextBox txtPassword = null!;
        private Button  btnLogin    = null!;
        private Label   lblError    = null!;

        public LoginForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text            = "EmpresaApp - Iniciar Sesión";
            this.Size            = new Size(460, 640);
            this.StartPosition   = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox     = false;
            this.BackColor       = Colores.Primario;
            this.Font            = new Font("Segoe UI", 9.5f);

            // Header
            var pnlHeader = new Panel { Dock = DockStyle.Top, Height = 160, BackColor = Colores.Primario };
            new Label { Text = "🏢", Font = new Font("Segoe UI Emoji", 42), ForeColor = Color.White, TextAlign = ContentAlignment.MiddleCenter, Dock = DockStyle.Top, Height = 90 }.Parent = pnlHeader;
            new Label { Text = "EmpresaApp", Font = new Font("Segoe UI", 22, FontStyle.Bold), ForeColor = Color.White, TextAlign = ContentAlignment.MiddleCenter, Dock = DockStyle.Top, Height = 45 }.Parent = pnlHeader;
            new Label { Text = "Sistema de Gestión Empresarial", Font = new Font("Segoe UI", 9), ForeColor = Color.FromArgb(148, 187, 233), TextAlign = ContentAlignment.MiddleCenter, Dock = DockStyle.Top, Height = 25 }.Parent = pnlHeader;

            // Panel principal
            var panelMain = new Panel { BackColor = Colores.Fondo, Dock = DockStyle.Fill };

            new Label { Text = "Iniciar Sesión", Font = new Font("Segoe UI", 16, FontStyle.Bold), ForeColor = Colores.TextoPrimario, Location = new Point(40, 28), AutoSize = true }.Parent = panelMain;

            new Label { Text = "Usuario", Location = new Point(40, 75), AutoSize = true, ForeColor = Colores.TextoSecundario, Font = new Font("Segoe UI", 9, FontStyle.Bold) }.Parent = panelMain;
            txtUsuario = new TextBox { Location = new Point(40, 95), Size = new Size(360, 35), Font = new Font("Segoe UI", 11), BorderStyle = BorderStyle.FixedSingle, PlaceholderText = "Ingrese su usuario" };
            panelMain.Controls.Add(txtUsuario);

            new Label { Text = "Contraseña", Location = new Point(40, 148), AutoSize = true, ForeColor = Colores.TextoSecundario, Font = new Font("Segoe UI", 9, FontStyle.Bold) }.Parent = panelMain;
            txtPassword = new TextBox { Location = new Point(40, 168), Size = new Size(360, 35), Font = new Font("Segoe UI", 11), BorderStyle = BorderStyle.FixedSingle, PasswordChar = '●', PlaceholderText = "Ingrese su contraseña" };
            panelMain.Controls.Add(txtPassword);
            txtPassword.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) BtnLogin_Click(s, e); };

            lblError = new Label { Location = new Point(40, 215), Size = new Size(360, 25), ForeColor = Colores.Alerta, Font = new Font("Segoe UI", 9), Visible = false };
            panelMain.Controls.Add(lblError);

            btnLogin = new Button
            {
                Text = "INICIAR SESIÓN", Location = new Point(40, 250), Size = new Size(360, 48),
                BackColor = Colores.Secundario, ForeColor = Color.White, FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11, FontStyle.Bold), Cursor = Cursors.Hand
            };
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.Click += BtnLogin_Click;
            panelMain.Controls.Add(btnLogin);

            // ── Botón configurar conexión ─────────────────────
            var btnConexion = new Button
            {
                Text = "⚙️  Configurar conexión SQL",
                Location = new Point(40, 348),
                Size = new Size(360, 34),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9),
                ForeColor = Colores.TextoSecundario,
                BackColor = Color.FromArgb(226, 232, 240),
                Cursor = Cursors.Hand
            };
            btnConexion.FlatAppearance.BorderSize = 0;
            btnConexion.Click += (s, e) =>
            {
                var dlg = new ConexionForm();
                dlg.ShowDialog();
            };
            panelMain.Controls.Add(btnConexion);

            // ── Botón cambiar servidor ────────────────────────────
            var btnCambiarServidor = new Button
            {
                Text = "🖥️  Cambiar servidor",
                Location = new Point(40, 390),
                Size = new Size(360, 30),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8.5f),
                ForeColor = Colores.TextoSecundario,
                BackColor = Color.Transparent,
                Cursor = Cursors.Hand
            };
            btnCambiarServidor.FlatAppearance.BorderSize = 0;
            btnCambiarServidor.Click += (s, e) =>
            {
                EmpresaApp.Data.ServerConfig.EliminarServidor();
                var dlg = new ServidorForm();
                if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    // Refrescar el hint de conexión en el label
                }
            };
            panelMain.Controls.Add(btnCambiarServidor);

            this.Controls.Add(panelMain);
            this.Controls.Add(pnlHeader);
        }

        private void BtnLogin_Click(object? sender, EventArgs e)
        {
            lblError.Visible = false;
            if (string.IsNullOrWhiteSpace(txtUsuario.Text) || string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                lblError.Text = "⚠ Complete todos los campos.";
                lblError.Visible = true;
                return;
            }

            try
            {
                DataManager.Inicializar(DbConfig.ConnectionString);
                var usuario = DataManager.ObtenerUsuarioPorLogin(
                    txtUsuario.Text.Trim(), txtPassword.Text);

                if (usuario == null)
                {
                    lblError.Text = "✗ Usuario o contraseña incorrectos.";
                    lblError.Visible = true;
                    return;
                }

                Session.UsuarioActual = usuario;
                DataManager.RegistrarEntrada(usuario.Id);

                this.Hide();
                var dashboard = new DashboardForm();
                dashboard.FormClosed += (s, args) => this.Close();
                dashboard.Show();
            }
            catch (Exception ex)
            {
                lblError.Text = "✗ Error de conexión a la base de datos.";
                lblError.Visible = true;
                MessageBox.Show(
                    $"No se pudo conectar a SQL Server:\n\n{ex.Message}\n\n" +
                    "Use el botón 'Configurar conexión SQL' para ajustar la cadena de conexión.",
                    "Error de conexión", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    // ─────────────────────────────────────────────────────────
    //  Formulario de configuración de cadena de conexión
    // ─────────────────────────────────────────────────────────
    public class ConexionForm : Form
    {
        public ConexionForm()
        {
            this.Text            = "⚙️ Configurar conexión SQL Server";
            this.Size            = new Size(560, 420);
            this.StartPosition   = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox     = false;
            this.BackColor       = Colores.Fondo;

            var pnlHeader = new Panel { Dock = DockStyle.Top, Height = 50, BackColor = Colores.Primario };
            new Label { Text = "⚙️  Conexión a SQL Server", Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = Color.White, Location = new Point(15, 12), AutoSize = true }.Parent = pnlHeader;

            // Campos rápidos
            int y = 65;
            Label L(string t) { var l = new Label { Text = t, Location = new Point(25, y), AutoSize = true, ForeColor = Colores.TextoSecundario, Font = new Font("Segoe UI", 8.5f, FontStyle.Bold) }; this.Controls.Add(l); return l; }
            TextBox F(string placeholder, string val = "")
            {
                var tb = new TextBox { Location = new Point(25, y + 20), Size = new Size(500, 28), Font = new Font("Segoe UI", 10), BorderStyle = BorderStyle.FixedSingle, Text = val };
                this.Controls.Add(tb); y += 56; return tb;
            }

            L("Servidor (Server)");
            var txtServer = F("ej: . ó MI_SERVIDOR\\SQLEXPRESS", ".");
            L("Base de datos (Database)");
            var txtDb = F("ej: EmpresaApp", "EmpresaApp");
            L("Usuario SQL (dejar vacío para autenticación Windows)");
            var txtUser = F("");
            L("Contraseña SQL");
            var txtPass = F(""); txtPass.PasswordChar = '●';

            // Cadena completa (editable)
            var lblFull = new Label { Text = "Cadena completa (editable directamente):", Location = new Point(25, y), AutoSize = true, ForeColor = Colores.TextoSecundario, Font = new Font("Segoe UI", 8.5f, FontStyle.Bold) };
            this.Controls.Add(lblFull); y += 20;
            var txtFull = new TextBox { Location = new Point(25, y), Size = new Size(500, 28), Font = new Font("Segoe UI", 9), BorderStyle = BorderStyle.FixedSingle, Text = DbConfig.ConnectionString };
            this.Controls.Add(txtFull); y += 40;

            // Regenerar cadena al cambiar campos rápidos
            void Regenerar()
            {
                bool winAuth = string.IsNullOrWhiteSpace(txtUser.Text);
                string auth  = winAuth
                    ? "Trusted_Connection=True;TrustServerCertificate=True;"
                    : $"User Id={txtUser.Text};Password={txtPass.Text};TrustServerCertificate=True;";
                txtFull.Text = $"Server={txtServer.Text};Database={txtDb.Text};{auth}";
            }
            txtServer.TextChanged += (s, e) => Regenerar();
            txtDb.TextChanged     += (s, e) => Regenerar();
            txtUser.TextChanged   += (s, e) => Regenerar();
            txtPass.TextChanged   += (s, e) => Regenerar();

            var btnGuardar = new Button
            {
                Text = "💾  Guardar",
                Location = new Point(25, y + 8),
                Size = new Size(140, 38),
                BackColor = Colores.Acento,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnGuardar.FlatAppearance.BorderSize = 0;
            btnGuardar.Click += (s, e) =>
            {
                DbConfig.ConnectionString = txtFull.Text.Trim();
                MessageBox.Show("✅ Cadena de conexión guardada para esta sesión.\n\nSi desea que sea permanente, edite DbConfig.cs.",
                    "Guardado", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close();
            };

            var btnProbar = new Button
            {
                Text = "🔌  Probar conexión",
                Location = new Point(175, y + 8),
                Size = new Size(175, 38),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10),
                Cursor = Cursors.Hand
            };
            btnProbar.Click += (s, e) =>
            {
                try
                {
                    using var conn = new Microsoft.Data.SqlClient.SqlConnection(txtFull.Text.Trim());
                    conn.Open();
                    MessageBox.Show("✅ Conexión exitosa.", "Prueba OK",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"✗ Falló:\n{ex.Message}", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            this.Controls.Add(pnlHeader);
            this.Controls.Add(btnGuardar);
            this.Controls.Add(btnProbar);
        }
    }
}
